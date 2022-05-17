using System.Diagnostics.CodeAnalysis;

namespace Ovation.FasterQC.Net
{
    [SuppressMessage("Code style", "IDE1006", Justification = "Names correspond to BAM structure field names")]
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

        public byte[] read_name { get; set; } = null!;

        public uint[] cigar { get; set; } = null!;

        public byte[] seq { get; set; } = null!;

        public byte[] qual { get; set; } = null!;
    }
}
