using System;
using System.Diagnostics;
using Ovation.FasterQC.Net;
using ShellProgressBar;

namespace fasterqc.net.Utils
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

        public void Update()
        {
            var read = sequenceReader.SequencesRead;
            var percent = sequenceReader.ApproximateCompletion;

            if (read % 100_000 == 0)
            {
                if (percent > 0)
                {
                    var remainingTime = elapsedTime.ElapsedMilliseconds / percent * 100.0;
                    EstimatedDuration = TimeSpan.FromMilliseconds(remainingTime);
                }

                AsProgress<double>().Report(percent / 100.0);
                Message = $"{read.WithSsiUnits()} sequences completed";
            }
        }
    }
}
