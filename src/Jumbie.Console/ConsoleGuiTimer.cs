using System;
using System.Threading;

namespace Jumbie.Console
{
    public static class ConsoleGuiTimer
    {
        private static Timer? _timer;
        private static int _interval = 100;
        private static readonly object _lock = new object();
        private static bool _isRunning;

        public static event EventHandler? Tick;

        public static void Start(int intervalMs = 100)
        {
            lock (_lock)
            {
                if (_isRunning) return;
                _interval = intervalMs;
                _isRunning = true;
                _timer = new Timer(OnTick, null, _interval, _interval);
            }
        }

        public static void Stop()
        {
            lock (_lock)
            {
                _isRunning = false;
                _timer?.Dispose();
                _timer = null;
            }
        }

        private static void OnTick(object? state)
        {
            Tick?.Invoke(null, EventArgs.Empty);
        }
    }
}
