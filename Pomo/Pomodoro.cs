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

        TimeSpan _currentInterval;
        PomodoroMode _currentMode;

        #endregion

        #region Public Properties

        public TimeSpan CurrentInterval { get { return _currentInterval; } }

        public PomodoroMode CurrentMode { get { return _currentMode; } }

        public bool IsRunning { get { return _timer.Enabled; } }

        #endregion

        #region Public Events

        public event ElapsedEventHandler Tick;

        public event EventHandler BreakTime;

        public event EventHandler WorkTime;

        #endregion

        #region Constructors

        public Pomodoro(bool autoStart = true, int workDuration = 25, int breakDuration = 5)
        {
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
            _currentInterval = _workInterval;
            _currentMode = PomodoroMode.Work;
        }

        public void Next()
        {
            switch (_currentMode)
            {
                case PomodoroMode.Work:
                    _currentMode = PomodoroMode.Break;
                    _currentInterval = _breakInterval;
                    break;

                case PomodoroMode.Break:
                    _currentMode = PomodoroMode.Work;
                    _currentInterval = _workInterval;
                    break;
            }

            if (Tick != null) { Tick(this, null); }
        }

        #endregion

        #region Private Methods

        void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _currentInterval = _currentInterval.Add(new TimeSpan(0, 0, -1));

            if (Tick != null) { Tick(this, e); }

            if (_currentInterval.TotalSeconds == 0)
            {
                switch (_currentMode)
                {
                    case PomodoroMode.Work:
                        _currentMode = PomodoroMode.Break;
                        _currentInterval = _breakInterval;

                        if (BreakTime != null) { BreakTime(this, EventArgs.Empty); }

                        break;

                    case PomodoroMode.Break:
                        _currentMode = PomodoroMode.Work;
                        _currentInterval = _workInterval;

                        if (WorkTime != null) { WorkTime(this, EventArgs.Empty); }

                        break;
                }
            }
        }

        #endregion
    }
}