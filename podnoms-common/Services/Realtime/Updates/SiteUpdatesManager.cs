
using System;
using System.Threading;

namespace PodNoms.Common.Services.Realtime.Updates {
    public class SiteUpdatesManager {
        private readonly Timer _timer;
        private readonly AutoResetEvent _autoResetEvent;
        private readonly Action _action;
        public DateTime TimerStarted { get; }

        public SiteUpdatesManager(Action action) {
            _action = action;
            _autoResetEvent = new AutoResetEvent(false);
            _timer = new Timer(Execute, _autoResetEvent, 1000, 2000);
            TimerStarted = DateTime.Now;
        }

        public void Execute(object stateInfo) {
            _action();
            if ((DateTime.Now - TimerStarted).Seconds > 60) {
                _timer.Dispose();
            }
        }
    }
}
