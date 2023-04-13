using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Ovation.BgzMR
{
    public record BgzMemberBoundary(long StartPos, long EndPos);

    public class InvalidHeaderException : Exception
    {
        public InvalidHeaderException() : base()
        {
        }

        public InvalidHeaderException(string message) : base(message)
        {
        }

        public InvalidHeaderException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }


    public class BgzMemberFinder
    {
        private record BgzHeaderResult(long relativeHeaderOffset, long blockSize);

        private readonly string filePath;

        private readonly long startPos;

        private readonly long endPos;

        public BgzMemberFinder(string filePath, long startPos, long endPos)
        {
            this.filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
            this.startPos = startPos;
            this.endPos = endPos;
        }

        public IList<BgzMemberBoundary> FindMemberBoundaries()
        {
            var memberBoundaries = new List<BgzMemberBoundary>();

            var fileStream = File.OpenRead(filePath);
            var searchStart = startPos;
            while (searchStart < endPos)
            {
                fileStream.Seek(searchStart, SeekOrigin.Begin);
                var header = ParseNextHeader(fileStream: fileStream, maxBytesToSkip: endPos - searchStart);
                if (header == null)
                {
                    break;
                }

                Trace.Assert(searchStart + header.relativeHeaderOffset <= endPos, $"Found header starting at {searchStart + header.relativeHeaderOffset} which is greater than endPos {endPos}");

                memberBoundaries.Add(new BgzMemberBoundary(StartPos: searchStart + header.relativeHeaderOffset, EndPos: searchStart + header.relativeHeaderOffset + header.blockSize));

                searchStart += header.relativeHeaderOffset + header.blockSize;
            }

            return memberBoundaries;
        }

        private enum MatchState
        {
            NoMatch,
            FoundId1,
            FoundId2,
            FoundCm,
            FoundFlg,
            FoundMtime,
            FoundXfl,
            FoundOs,
            FoundXlen,
        }

        private static BgzHeaderResult? ParseNextHeader(FileStream fileStream, long maxBytesToSkip)
        {
            const int headerSize = 1 + // ID1
                                   1 + // ID2
                                   1 + // CM
                                   1 + // FLG
                                   4 + // MTIME
                                   1 + // XFL
                                   1 + // OS
                                   2 + // XLEN
                                   1 + // SI1
                                   1 + // SI2
                                   2 + // LEN
                                   2; // Value of LEN

            var curState = MatchState.NoMatch;
            var numBytesSeen = 0;
            var xlen = -1;

            while (curState != MatchState.FoundXlen)
            {
                if (curState == MatchState.NoMatch && numBytesSeen > maxBytesToSkip)
                {
                    return null;
                }

                var curByte = fileStream.ReadByte();
                numBytesSeen += 1;
                if (curByte == -1)
                {
                    return null;
                }

                MatchState nextState = MatchState.NoMatch;
                var mostRecentByte = curByte;

                if (curState == MatchState.NoMatch && curByte == 0x1f)
                {
                    nextState = MatchState.FoundId1;
                }
                else if (curState == MatchState.FoundId1 && curByte == 0x8b)
                {
                    nextState = MatchState.FoundId2;
                }
                else if (curState == MatchState.FoundId2 && curByte == 8)
                {
                    nextState = MatchState.FoundCm;
                }
                else if (curState == MatchState.FoundCm && (curByte & (1 << 2)) > 0)
                {
                    nextState = MatchState.FoundFlg;
                }
                else if (curState == MatchState.FoundFlg)
                {
                    // TODO: finding an ID1 byte in MTIME means we may need to restart our state machine at FoundId1
                    if (curByte == 0x1f)
                    {
                        throw new InvalidHeaderException(
                            "Found an ID1 byte as the first byte of MTIME, can't handle this yet");
                    }

                    var byte2 = fileStream.ReadByte();
                    numBytesSeen += 1;
                    switch (byte2)
                    {
                        case -1:
                            return null;
                        case 0x1f:
                            throw new InvalidHeaderException(
                                "Found an ID1 byte as the second byte of MTIME, can't handle this yet");
                    }

                    var byte3 = fileStream.ReadByte();
                    numBytesSeen += 1;
                    switch (byte3)
                    {
                        case -1:
                            return null;
                        case 0x1f:
                            throw new InvalidHeaderException(
                                "Found an ID1 byte as the third byte of MTIME, can't handle this yet");
                    }

                    var byte4 = fileStream.ReadByte();
                    numBytesSeen += 1;
                    switch (byte4)
                    {
                        case -1:
                            return null;
                        case 0x1f:
                            throw new InvalidHeaderException(
                                "Found an ID1 byte as the fourth byte of MTIME, can't handle this yet");
                        default:
                            nextState = MatchState.FoundMtime;
                            mostRecentByte = byte4;
                            break;
                    }
                }
                else if (curState == MatchState.FoundMtime)
                {
                    // TODO: do we need to check that this is a specific value?
                    nextState = MatchState.FoundXfl;
                }
                else if (curState == MatchState.FoundXfl)
                {
                    // TODO: do we need to check that this is a specific value? The spec has a list of allowed values, but
                    // TODO: not sure if it's up-to-date or not.
                    nextState = MatchState.FoundOs;
                }
                else if (curState == MatchState.FoundOs)
                {
                    var nextByte = fileStream.ReadByte();
                    numBytesSeen += 1;
                    if (nextByte == -1)
                    {
                        return null;
                    }

                    xlen = BinaryPrimitives.ReadUInt16LittleEndian(new[] { (byte)curByte, (byte)nextByte });
                    nextState = MatchState.FoundXlen;
                    mostRecentByte = nextByte;
                }

                // In the case where the parsing bailed out somewhere but we ended up on an ID1 byte we need to set the
                // next state to FoundId1, otherwise we might possibly miss the start of a header.
                if (nextState == MatchState.NoMatch && mostRecentByte == 0x1f)
                {
                    nextState = MatchState.FoundId1;
                }

                curState = nextState;
            }

            // At this point we have found what we believe to be a valid BGZF header and our file stream is one byte
            // past the XLEN byte
            Trace.Assert(xlen != -1, "xlen's value is still -1 even though it should have been set to a valid ushort");
            if (xlen != 6)
            {
                // TODO: Technically there could be more XLEN fields other than just the BGZF one, this code just throws
                // TODO: in that scenario for now.
                throw new InvalidHeaderException($"Expected XLEN to equal 6, found {xlen}");
            }

            var markerByte1 = fileStream.ReadByte();
            var markerByte2 = fileStream.ReadByte();
            numBytesSeen += 2;
            if (markerByte1 != 66 || markerByte2 != 67)
            {
                throw new InvalidHeaderException(
                    $"Expected to find marker bytes of BC for BGZF metadata, found {markerByte1} and {markerByte2}");
            }

            var slenByte1 = fileStream.ReadByte();
            var slenByte2 = fileStream.ReadByte();
            numBytesSeen += 2;
            if (slenByte1 == -1 || slenByte2 == -1)
            {
                throw new InvalidHeaderException("SLEN bytes not found");
            }

            var slen = BinaryPrimitives.ReadUInt16LittleEndian(new[] { (byte)slenByte1, (byte)slenByte2 });
            if (slen != 2)
            {
                throw new InvalidHeaderException($"BGZF SLEN value must be 2, found {slen}");
            }

            var bsizeByte1 = fileStream.ReadByte();
            var bsizeByte2 = fileStream.ReadByte();
            numBytesSeen += 2;
            if (bsizeByte1 == -1 || bsizeByte2 == -1)
            {
                throw new InvalidHeaderException("BSIZE bytes not found");
            }

            var bsize = BinaryPrimitives.ReadUInt16LittleEndian(new[] { (byte)bsizeByte1, (byte)bsizeByte2 });

            return new BgzHeaderResult(relativeHeaderOffset: numBytesSeen - headerSize, blockSize: (long)bsize + 1);
        }
    }
}
