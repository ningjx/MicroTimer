using NingSoft.CSharpTools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using static System.Net.Mime.MediaTypeNames;
using CosTimer = NingSoft.CSharpTools.MicroTimer;

namespace MicroTimer
{
    public class Handler : INotifyPropertyChanged
    {
        private static CosTimer? _timer = null;
        private static int _period = 1; // 1毫秒周期，充分利用144Hz屏幕
        private static bool _pause = true; // 初始为暂停
        private static bool _stop = true;
        public static bool Stop => _stop;
        private static bool _swapText = false;

        public event PropertyChangedEventHandler? PropertyChanged;

        // 改为实例属性而不是静态属性
        public TimeOnly TimeNow { get; private set; } = new(0, 0, 0, 0);
        public string TimeNowString => _swapText ? TimeNow.ToString("fff") : TimeNow.ToString("HH:mm:ss");
        public string MsNowString => _swapText ? TimeNow.ToString("HH:mm:ss") : TimeNow.ToString("fff");

        public Handler()
        {
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
        }

        public void ResetAction()
        {
            // 重置并暂停
            _pause = true;
            _stop = true;
            TimeNow = new(0, 0, 0, 0);
            OnPropertyChanged(nameof(TimeNow));
            OnPropertyChanged(nameof(TimeNowString));
            OnPropertyChanged(nameof(MsNowString));
        }
    }
}
