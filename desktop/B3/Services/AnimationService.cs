using System;
using System.Threading.Tasks;
using Avalonia.Threading;

namespace B3.Services
{
    /// <summary>
    /// Simple GPU-friendly animation helper using Dispatcher timer.
    /// Provides small, reusable async animation helpers for transforms and numeric values.
    /// </summary>
    public static class AnimationService
    {
        public static Task AnimateDoubleAsync(Action<double> setter, double from, double to, TimeSpan duration, Func<double, double>? easing = null)
        {
            var tcs = new TaskCompletionSource<bool>();
            var sw = System.Diagnostics.Stopwatch.StartNew();
            easing ??= EaseOutCubic;
            var timer = new DispatcherTimer(TimeSpan.FromMilliseconds(16), DispatcherPriority.Normal, (s, e) =>
            {
                var elapsed = sw.Elapsed.TotalMilliseconds;
                var progress = Math.Min(1.0, elapsed / Math.Max(1, duration.TotalMilliseconds));
                var eased = easing(progress);
                var val = from + (to - from) * eased;
                try
                {
                    setter(val);
                }
                catch
                {
                }

                if (progress >= 1.0)
                {
                    ((DispatcherTimer)s!).Stop();
                    sw.Stop();
                    tcs.TrySetResult(true);
                }
            });

            timer.Start();
            return tcs.Task;
        }

        private static double EaseOutCubic(double t)
        {
            t = Math.Max(0, Math.Min(1, t));
            return 1 - Math.Pow(1 - t, 3);
        }
    }
}
