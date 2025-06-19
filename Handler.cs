using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MicroTimer
{
    public class Handler : INotifyPropertyChanged
    {
        private static Handler? _handler;
        private static CosTimer? _timer = null;
        private static int _period = 1; // 计时周期
        private static bool _pause = true;
        private static bool _stop = true;
        private static bool _swapText = false;
        private static int _freshRate { get; set; } = 200;
        private static int _refreshTicks = 0;
        private static int _refreshInterval = 0;

        public event PropertyChangedEventHandler? PropertyChanged;
        public static bool Stop => _stop;

        public static TimeOnly TimeNow { get; private set; } = new(0, 0, 0, 0);
        public string TimeNowString => _swapText ? TimeNow.ToString("fff") : TimeNow.ToString("HH:mm:ss");
        public string MsNowString => _swapText ? TimeNow.ToString("HH:mm:ss") : TimeNow.ToString("fff");

        public static Handler Instance
        {
            get
            {
                if (_handler == null)
                {
                    _handler = new Handler();
                }
                return _handler;
            }
        }

        private Handler()
        {
            SetRefreshRate(_freshRate);
            InitializeTimer();
        }

        private void InitializeTimer()
        {
            _timer = new CosTimer(_period, TimerMode.PERIODIC);
            _timer.OnRunning += _timer_OnRunning;
            _timer.Start();
        }

        private void _timer_OnRunning(ulong ticks)
        {
            if (!_pause && !_stop)
            {
                UpdateTime();
            }
        }

        private void UpdateTime()
        {
            TimeNow = TimeNow.Add(new TimeSpan(0, 0, 0, 0, _period));
            if (_refreshTicks >= _refreshInterval)
            {
                _refreshTicks = 0;
                ChangeUI();
            }
            else
            {
                _refreshTicks++;
            }
        }

        private void ChangeUI()
        {
            OnPropertyChanged(nameof(TimeNow));
            OnPropertyChanged(nameof(TimeNowString));
            OnPropertyChanged(nameof(MsNowString));
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Dispose()
        {
            if (_timer != null)
            {
                _timer.Dispose();
            }
            _handler = null;
        }

        public void SetRefreshRate(int rate)
        {
            if (rate <= 0) return;
            _freshRate = rate;
            _refreshInterval = (int)(1000.0 / _freshRate / _period - 1);
        }

        public void SwapText()
        {
            _swapText = !_swapText;
            OnPropertyChanged(nameof(TimeNowString));
            OnPropertyChanged(nameof(MsNowString));
        }
        public void SingleAction()
        {
            if (_pause)
            {
                _pause = false;
                _stop = false;
            }
            else
            {
                _pause = true;
            }
            UpdateTime();
            ChangeUI();
        }

        public void ResetAction()
        {
            // 重置并暂停
            _pause = true;
            _stop = true;
            TimeNow = new(0, 0, 0, 0);
            ChangeUI();
        }
    }
}
