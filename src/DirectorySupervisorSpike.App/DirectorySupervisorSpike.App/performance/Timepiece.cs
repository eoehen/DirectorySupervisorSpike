using System.Diagnostics;

namespace DirectorySupervisorSpike.App.performance
{
    internal readonly struct Timepiece
    {
        private static readonly double s_timestampToTicks = TimeSpan.TicksPerSecond / (double)Stopwatch.Frequency;

        private readonly long _startTimestamp;

        private Timepiece(long startTimestamp) => _startTimestamp = startTimestamp;

        public static Timepiece StartNew() => new Timepiece(GetTimestamp());

        public static long GetTimestamp() => Stopwatch.GetTimestamp();

        public static TimeSpan GetElapsedTime(long startTimestamp, long endTimestamp)
        {
            var timestampDelta = endTimestamp - startTimestamp;
            var ticks = (long)(s_timestampToTicks * timestampDelta);
            return new TimeSpan(ticks);
        }

        public TimeSpan GetElapsedTime() => GetElapsedTime(_startTimestamp, GetTimestamp());
    }
}
