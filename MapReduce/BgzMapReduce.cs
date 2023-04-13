using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using Serilog;

namespace Ovation.BgzMR
{
    public class BgzMapReduce
    {
        private record MapBoundary(long StartPos, long EndPos);

        private readonly string filePath;

        private readonly int numThreads;

        private readonly ILogger logger;

        public BgzMapReduce(string filePath, int numThreads, ILogger logger)
        {
            this.filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
            this.numThreads = numThreads;
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public BgzMapStep<T> Map<T>(Func<Stream, int, T> mapFunction)
        {
            return new BgzMapStep<T>(filePath, numThreads, mapFunction, logger);
        }

        public class BgzMapStep<TM>
        {

            private readonly string filePath;

            private readonly int numThreads;

            private readonly Func<Stream, int, TM> mapFunction;

            private readonly ILogger logger;

            internal BgzMapStep(string filePath, int numThreads, Func<Stream, int, TM> mapFunction, ILogger logger)
            {
                this.filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
                this.numThreads = numThreads;
                this.mapFunction = mapFunction;
                this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            }

            public TR Reduce<TR>(Func<IReadOnlyCollection<TM>, TR> reduceFunction)
            {
                ThreadPool.SetMaxThreads(numThreads, 1);

                var mapBoundaries = ComputeMapBoundaries();

                var mapJobs = mapBoundaries.Select<MapBoundary, Func<TM>>((mapBoundary, idx) => () =>
                {
                    var stream = new GZipStream(new FileSliceStream(filePath, mapBoundary.StartPos, mapBoundary.EndPos),
                        CompressionMode.Decompress);
                    return mapFunction(stream, idx);
                })
                    .ToList();

                logger.Debug("Starting {MapJobCount} map jobs", mapJobs.Count);

                var mapResults = ThreadPoolUtils.RunOnThreadPool(mapJobs);

                logger.Debug("Completed all map jobs");

                return reduceFunction(mapResults);
            }

            private IList<MapBoundary> ComputeMapBoundaries()
            {
                var fileLength = new FileInfo(filePath).Length;

                // To find all member boundaries in parallel we first compute the ranges of the file that each thread is
                // allowed to look at.
                var searchRanges = new List<(long SearchStart, long SearchEnd)>();
                var sizePerRange = fileLength / numThreads;
                long startPos = 0;
                while (true)
                {
                    var endPos = startPos + sizePerRange;
                    if (endPos >= fileLength - numThreads)
                    {
                        searchRanges.Add((startPos, fileLength));
                        break;
                    }
                    else
                    {
                        searchRanges.Add((startPos, endPos));
                        startPos += sizePerRange;
                    }
                }

                // Now assign each range to a thread. Each thread is allowed to return boundary information for members
                // that start within its range.
                var searchJobs = searchRanges.Select<(long SearchStart, long SearchEnd), Func<IList<BgzMemberBoundary>>>(range => () =>
                {
                    var memberFinder = new BgzMemberFinder(filePath, range.SearchStart, range.SearchEnd);
                    return memberFinder.FindMemberBoundaries();
                }).ToList();

                logger.Debug("Running {SearchJobsCount} gzip member search jobs", searchJobs.Count);

                var memberBoundaries = ThreadPoolUtils.RunOnThreadPool(searchJobs)
                    .SelectMany(boundaries => boundaries)
                    .ToList();
                memberBoundaries.Sort((a, b) => a.StartPos.CompareTo(b.StartPos));

                logger.Debug("Found {MemberCount} members", memberBoundaries.Count);

                // Now that we have a whole bunch of member boundaries we need to merge them into numThreads regions so
                // that we can give each map thread a larger region to reduce overhead of thread scheduling.
                var averageBytesPerRegion = fileLength / numThreads;
                var mergedRegions = new List<MapBoundary>();
                long curStart = 0;
                long curEnd = 0;
                var i = 0;
                while (curEnd < fileLength)
                {
                    if (curEnd - curStart > averageBytesPerRegion)
                    {
                        mergedRegions.Add(new MapBoundary(StartPos: curStart, EndPos: curEnd));
                        curStart = curEnd;
                    }

                    curEnd = memberBoundaries[i].EndPos;
                    i += 1;
                }

                mergedRegions.Add(new MapBoundary(StartPos: curStart, EndPos: curEnd));
                return mergedRegions;
            }
        }
    }
}
