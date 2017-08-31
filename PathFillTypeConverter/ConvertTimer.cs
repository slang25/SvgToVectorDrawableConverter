using System;
using System.Diagnostics;
using PathFillTypeConverter.Exceptions;

namespace PathFillTypeConverter
{
    internal static class ConvertTimer
    {
        private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(10);

        private static readonly Stopwatch Stopwatch = new Stopwatch();

        public static void Restart()
        {
            Stopwatch.Restart();
        }

        public static void ThrowIfOvertime()
        {
            if (Stopwatch.Elapsed > Timeout)
            {
                throw new ConvertOvertimeException();
            }
        }
    }
}
