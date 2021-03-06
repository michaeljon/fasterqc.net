namespace Ovation.FasterQC.Net.Utils
{
    public static class PrettyPrintExtension
    {
        public static string WithSsiUnits(this ulong n)
        {
            return n switch
            {
                >= 1_000_000_000 => $"{(n / 1_000_000_000.0):0.0}G",
                >= 1_000_000 => $"{(n / 1_000_000.0):0.0}M",
                >= 1_000 => $"{(n / 1_000.0):0.0}K",
                _ => n.ToString()
            };
        }
    }
}
