using System;
using System.Collections.Generic;

namespace Ovation.FasterQC.Net
{

    public class BaseGroup
    {
        private static readonly bool nogroup = false;
        private static readonly bool expgroup = false;
        private static readonly int minLength = 0;

        public int LowerCount { get; private set; }
        public int UpperCount { get; private set; }

        public static BaseGroup[] MakeBaseGroups(int maxLength)
        {
            if (minLength > maxLength)
            {
                maxLength = minLength;
            }

            if (nogroup)
            {
                return MakeUngroupedGroups(maxLength);
            }
            else if (expgroup)
            {
                return MakeExponentialBaseGroups(maxLength);
            }
            else
            {
                return MakeLinearBaseGroups(maxLength);
            }
        }

        public static BaseGroup[] MakeUngroupedGroups(int maxLength)
        {
            int startingBase = 1;
            int interval = 1;
            var groups = new List<BaseGroup>();

            while (startingBase <= maxLength)
            {
                int endBase = startingBase + (interval - 1);
                if (endBase > maxLength)
                {
                    endBase = maxLength;
                }

                var bg = new BaseGroup(startingBase, endBase);
                groups.Add(bg);
                startingBase += interval;
            }

            return groups.ToArray();
        }

        public static BaseGroup[] MakeExponentialBaseGroups(int maxLength)
        {
            int startingBase = 1;
            int interval = 1;
            var groups = new List<BaseGroup>();

            while (startingBase <= maxLength)
            {
                int endBase = startingBase + (interval - 1);
                if (endBase > maxLength)
                {
                    endBase = maxLength;
                }

                var bg = new BaseGroup(startingBase, endBase);
                groups.Add(bg);
                startingBase += interval;

                if (startingBase == 10 && maxLength > 75)
                {
                    interval = 5;
                }

                if (startingBase == 50 && maxLength > 200)
                {
                    interval = 10;
                }

                if (startingBase == 100 && maxLength > 300)
                {
                    interval = 50;
                }

                if (startingBase == 500 && maxLength > 1000)
                {
                    interval = 100;
                }

                if (startingBase == 1000 && maxLength > 2000)
                {
                    interval = 500;
                }
            }

            return groups.ToArray();
        }

        private static int GetLinearInterval(int Length)
        {
            int[] baseValues = new int[] { 2, 5, 10 };
            int multiplier = 1;

            while (true)
            {
                for (int b = 0; b < baseValues.Length; b++)
                {
                    int interval = baseValues[b] * multiplier;
                    int groupCount = 9 + ((Length - 9) / interval);

                    if ((Length - 9) % interval != 0)
                    {
                        groupCount += 1;
                    }

                    if (groupCount < 75)
                    {
                        return interval;
                    }
                }

                multiplier *= 10;
                if (multiplier == 10000000)
                {
                    throw new InvalidOperationException($"Couldn't find a sensible interval grouping for length {Length}");
                }
            }
        }

        public static BaseGroup[] MakeLinearBaseGroups(int maxLength)
        {
            if (maxLength <= 75)
            {
                return MakeUngroupedGroups(maxLength);
            }

            int interval = GetLinearInterval(maxLength);
            int startingBase = 1;
            var groups = new List<BaseGroup>();

            while (startingBase <= maxLength)
            {
                int endBase = startingBase + (interval - 1);

                if (startingBase < 10)
                {
                    endBase = startingBase;
                }

                if (startingBase == 10 && interval > 10)
                {
                    endBase = interval - 1;
                }

                if (endBase > maxLength)
                {
                    endBase = maxLength;
                }

                var bg = new BaseGroup(startingBase, endBase);
                groups.Add(bg);

                if (startingBase < 10)
                {
                    startingBase += 1;
                }
                else if (startingBase == 10 && interval > 10)
                {
                    startingBase = interval;
                }
                else
                {
                    startingBase += interval;
                }
            }

            return groups.ToArray();
        }

        private BaseGroup(int lowerCount, int upperCount)
        {
            LowerCount = lowerCount;
            UpperCount = upperCount;
        }

        public bool ContainsValue(int value)
        {
            return value >= LowerCount && value <= UpperCount;
        }

        public override string ToString()
        {
            if (LowerCount == UpperCount)
            {
                return "" + LowerCount;
            }
            else
            {
                return "" + LowerCount + "-" + UpperCount;
            }
        }
    }
}
