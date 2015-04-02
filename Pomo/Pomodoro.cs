using System;
using System.Timers;

namespace Pomo
{
    public enum PomodoroMode
    {
        Work,
        Break
    }

    public class Pomodoro
    {
        #region Private Members

        readonly Timer _timer;
        readonly TimeSpan _workInterval;
        readonly TimeSpan _breakInterval;

        #endregion

        #region Public Properties

        public TimeSpan CurrentInterval { get; private set; }

        public PomodoroMode CurrentMode { get; private set; }

        public bool IsRunning { get { return _timer.Enabled; } }

        public int Count { get; private set; }

        #endregion

        #region Public Events

        public event ElapsedEventHandler Tick;

        public event EventHandler BreakTime;

        public event EventHandler WorkTime;

        #endregion

        #region Constructors

        public Pomodoro(bool autoStart = true, int workDuration = 25, int breakDuration = 5)
        {
            Count = 0;

            _workInterval = new TimeSpan(0, workDuration, 0);
            _breakInterval = new TimeSpan(0, breakDuration, 0);

            Reset();

            _timer = new Timer
            {
                Interval = 1000,
                Enabled = autoStart
            };

            _timer.Elapsed += Timer_Elapsed;
        }

        #endregion

        #region Public Methods

        public void Start()
        {
            _timer.Enabled = true;
        }

        public void Pause()
        {
            _timer.Enabled = false;
        }

        public void Reset()
        {
            CurrentInterval = _workInterval;
            CurrentMode = PomodoroMode.Work;
            Count = 0;
        }

        public void Next()
        {
            switch (CurrentMode)
            {
                case PomodoroMode.Work:
                    CurrentMode = PomodoroMode.Break;
                    CurrentInterval = _breakInterval;
                    break;

                case PomodoroMode.Break:
                    CurrentMode = PomodoroMode.Work;
                    CurrentInterval = _workInterval;
                    break;
            }

            if (Tick != null) { Tick(this, null); }
        }

        #endregion

        #region Private Methods

        void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            CurrentInterval = CurrentInterval.Add(new TimeSpan(0, 0, -1));

            if (Tick != null) { Tick(this, e); }

            if (CurrentInterval.TotalSeconds == 0)
            {
                switch (CurrentMode)
                {
                    case PomodoroMode.Work:
                        CurrentMode = PomodoroMode.Break;
                        CurrentInterval = _breakInterval;
                        Count++;

                        if (BreakTime != null) { BreakTime(this, EventArgs.Empty); }

                        break;

                    case PomodoroMode.Break:
                        CurrentMode = PomodoroMode.Work;
                        CurrentInterval = _workInterval;

                        if (WorkTime != null) { WorkTime(this, EventArgs.Empty); }

                        break;
                }
            }
        }

        #endregion
    }
}