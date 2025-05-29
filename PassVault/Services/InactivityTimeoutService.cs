using Timer = System.Timers.Timer;


namespace PassVault.Services
{
    public class InactivityTimeoutService
    {
        private readonly Timer _timer;
        private readonly TimeSpan _timeout = TimeSpan.FromMinutes(1);
        public event Action TimeoutElapsed = delegate { };

        public InactivityTimeoutService()
        {
            _timer = new Timer(_timeout.TotalMilliseconds);
            _timer.Elapsed += (s, e) => TimeoutElapsed?.Invoke();
            _timer.AutoReset = false;
        }

        public void Start() => _timer.Start();
        public void Stop() => _timer.Stop();
        public void Reset()
        {
            _timer.Stop();
            _timer.Start();
        }
    }
}
