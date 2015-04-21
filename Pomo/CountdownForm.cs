using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Pomo
{
    public partial class CountdownForm : Form
    {
        #region Private Members

        Pomodoro _pomodoro;
        int _hideCountdown = 0;
        Timer _hideTimer;
        Timer _updateTimer;
        bool _isFading = false;

        #endregion

        #region Constructors

        public CountdownForm()
        {
            InitializeComponent();

            Left = Screen.PrimaryScreen.WorkingArea.Width - Width - 10;
            Top = Screen.PrimaryScreen.WorkingArea.Height - Height - 10;
        }

        public CountdownForm(Pomodoro pomodoro)
            : this()
        {
            _pomodoro = pomodoro;

            _updateTimer = new Timer
            {
                Interval = 250,
                Enabled = true
            };

            _updateTimer.Tick += Pomodoro_Tick;

            _hideTimer = new Timer
            {
                Interval = 1000,
                Enabled = true
            };

            _hideTimer.Tick += _hideTimer_Tick;

            Visible = false;
        }

        #endregion

        #region Public Methods

        public void DoShow()
        {
            _hideCountdown = 3;

            Left = Screen.PrimaryScreen.WorkingArea.Width - Width - 10;
            Top = Screen.PrimaryScreen.WorkingArea.Height - Height - 10;

            if (Visible) { return; }

            Opacity = 0;
            Visible = true;

            FadeIn();
        }

        #endregion

        #region Private Methods

        void _hideTimer_Tick(object sender, EventArgs e)
        {
            if (_hideCountdown == 0)
            {
                FadeOut();
            }
            else
            {
                _hideCountdown--;
            }
        }

        void Pomodoro_Tick(object sender, EventArgs e)
        {
            displayLabel.Text = _pomodoro.CurrentInterval.ToString("mm':'ss");
            BackColor = _pomodoro.CurrentMode == PomodoroMode.Work ? Color.FromArgb(0xe7, 0x4c, 0x3c) : Color.FromArgb(0x2e, 0xcc, 0x71);
        }

        async void FadeIn()
        {
            if (_isFading) { return; }
            _isFading = true;
            while (Opacity < 1.0)
            {
                await Task.Delay(3);
                Opacity += 0.05;
            }
            Opacity = 1;
            _isFading = false;
        }

        async void FadeOut()
        {
            if (_isFading) { return; }
            _isFading = true;
            while (Opacity > 0.0)
            {
                await Task.Delay(3);
                Opacity -= 0.05;
            }
            Opacity = 0;
            Visible = false;
            _isFading = false;
        }

        #endregion
    }
}