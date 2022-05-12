namespace Ovation.FasterQC.Net
{
#pragma warning disable IDE1006
    public class BamAlignment
    {
        public uint block_size { get; set; }

        public int refID { get; set; }

        public int pos { get; set; }

        public byte l_read_name { get; set; }

        public byte mapq { get; set; }

        public ushort bin { get; set; }

        public ushort n_cigar_op { get; set; }

        public ushort flag { get; set; }

        public uint l_seq { get; set; }

        public int next_refID { get; set; }

        public int next_pos { get; set; }

        public int tlen { get; set; }

        public byte[] read_name { get; set; }

        public uint[] cigar { get; set; }

        public byte[] seq { get; set; }

        public byte[] qual { get; set; }
    }
#pragma warning restore IDE1006
}