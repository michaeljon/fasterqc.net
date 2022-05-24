using System;
using System.Diagnostics;
using ShellProgressBar;

namespace Ovation.FasterQC.Net.Utils
{
    public class TimedSequenceProgressBar : ProgressBar
    {
        private readonly Stopwatch elapsedTime = new();

        private readonly ISequenceReader sequenceReader;

        private static readonly ProgressBarOptions progressBarOptions = new()
        {
            ProgressCharacter = '=',
            DisplayTimeInRealTime = false,
            ShowEstimatedDuration = true
        };

        public TimedSequenceProgressBar(ISequenceReader sequenceReader) : base(100, "Processing...", progressBarOptions)
        {
            this.sequenceReader = sequenceReader;
            elapsedTime.Start();
        }

        public void Update(bool force = false)
        {
            var sequencesRead = sequenceReader.SequencesRead;
            var approximateCompletion = sequenceReader.ApproximateCompletion;

            if (force || sequencesRead % CliOptions.UpdatePeriod == 0)
            {
                // if we're limiting the number of reads then the reader's
                // approximation will be incorrect (it's based on file positions),
                // so we'll do the math ourselves
                if (CliOptions.Settings.ReadLimit < ulong.MaxValue)
                {
                    approximateCompletion = 100.0 * sequencesRead / CliOptions.Settings.ReadLimit;
                }

                if (approximateCompletion > 0)
                {
                    var remainingTime = elapsedTime.ElapsedMilliseconds / approximateCompletion * 100.0;
                    EstimatedDuration = TimeSpan.FromMilliseconds(remainingTime);
                }

                AsProgress<double>().Report(approximateCompletion / 100.0);
                Message = $"{sequencesRead.WithSsiUnits()} sequences completed";
            }
        }
    }
}
