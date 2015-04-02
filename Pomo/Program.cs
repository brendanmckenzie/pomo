using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Media;
using System.Windows.Forms;

namespace Pomo
{
    static class Program
    {
        #region Private Members

        static NotifyIcon _notifyIcon;
        static IDictionary<string, Icon> _iconCache = new Dictionary<string, Icon>();

        static Pomodoro _pomodoro;
        static MenuItem _toggleItem;

        #endregion

        #region Entry Point

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            SetupPomodoro();
            AddTrayIcon();

            Application.Run();

            RemoveTrayIcon();
        }

        #endregion

        #region Private Methods

        static void SetupPomodoro()
        {
            _pomodoro = new Pomodoro();

            _pomodoro.Tick += Pomodoro_Tick;
            _pomodoro.BreakTime += Pomodoro_BreakTime;
            _pomodoro.WorkTime += Pomodoro_WorkTime;
        }

        static void AddTrayIcon()
        {
            var menu = new ContextMenu();

            var exitItem = new MenuItem { Text = "E&xit" };
            exitItem.Click += (sender, e) => { Application.Exit(); };

            _toggleItem = new MenuItem { Text = "&Start" };
            _toggleItem.Click += (sender, e) =>
            {
                Toggle();
            };

            var skipItem = new MenuItem { Text = "S&kip" };
            skipItem.Click += (sender, e) => { _pomodoro.Next(); };

            var resetItem = new MenuItem { Text = "&Reset" };
            resetItem.Click += (sender, e) =>
            {
                _pomodoro.Reset();

                UpdateIcon();
            };

            menu.MenuItems.Add(resetItem);
            menu.MenuItems.Add(new MenuItem("-"));
            menu.MenuItems.Add(_toggleItem);
            menu.MenuItems.Add(skipItem);
            menu.MenuItems.Add(new MenuItem("-"));
            menu.MenuItems.Add(exitItem);

            menu.Popup += Menu_Popup;

            _notifyIcon = new NotifyIcon
            {
                Visible = true,
                ContextMenu = menu,
            };

            _notifyIcon.MouseClick += NotifyIcon_Click;

            UpdateIcon();
        }

        static void RemoveTrayIcon()
        {
            _notifyIcon.Visible = false;
        }

        static Icon GetIcon(string text = "0")
        {
            var key = string.Format("{0}:{1}:{2}", text, _pomodoro.CurrentMode, _pomodoro.IsRunning);

            var ret = _iconCache.ContainsKey(key) ? _iconCache[key] : null;
            if (ret == null)
            {
                var backgroundColor = _pomodoro.CurrentMode == PomodoroMode.Work ? Color.FromArgb(0xe7, 0x4c, 0x3c) : Color.FromArgb(0x2e, 0xcc, 0x71);
                if (!_pomodoro.IsRunning)
                {
                    backgroundColor = Color.Black;
                }

                using (var bitmap = new Bitmap(16, 16))
                {
                    using (var graphics = Graphics.FromImage(bitmap))
                    {
                        var rect = new Rectangle(0, 0, 16, 16);

                        using (var backgroundBrush = new SolidBrush(backgroundColor))
                        using (var borderPen = new Pen(Color.White))
                        {
                            graphics.FillRectangle(backgroundBrush, rect);
                            graphics.DrawRectangle(borderPen, new Rectangle(0, 0, 15, 15));
                        }

                        using (var textBrush = new SolidBrush(Color.White))
                        using (var font = new Font("Segoe UI", 8))
                        using (var stringFormat = new StringFormat())
                        {
                            stringFormat.Alignment = StringAlignment.Center;
                            stringFormat.LineAlignment = StringAlignment.Center;

                            var textRect = new Rectangle(rect.X, rect.Y + 1, rect.Width, rect.Height);

                            graphics.DrawString(text, font, textBrush, textRect, stringFormat);
                        }
                    }

                    ret = Icon.FromHandle(bitmap.GetHicon());

                    _iconCache.Add(key, ret);
                }
            }

            return ret;
        }

        static void UpdateIcon()
        {
            var text = _pomodoro.CurrentInterval.Seconds.ToString();
            if (_pomodoro.CurrentInterval.TotalSeconds > 60)
            {
                text = _pomodoro.CurrentInterval.Minutes.ToString();
            }

            _notifyIcon.Icon = GetIcon(text);
            _notifyIcon.Text = string.Format("{0} pomodoro{1} down", _pomodoro.Count, _pomodoro.Count == 1 ? null : "s");
        }

        static void PlaySound(Stream data)
        {
            using (var player = new SoundPlayer())
            {
                player.Stream = data;
                player.Play();
            }
        }

        static void Toggle()
        {
            if (_pomodoro.IsRunning)
            {
                _pomodoro.Pause();
            }
            else
            {
                _pomodoro.Start();
            }

            UpdateIcon();
        }

        #region Event Handlers

        static void Pomodoro_WorkTime(object sender, EventArgs e)
        {
            _notifyIcon.ShowBalloonTip(750, "Pomodoro", "Work time!", ToolTipIcon.Info);

            PlaySound(Resources.WorkTime);
        }

        static void Pomodoro_BreakTime(object sender, EventArgs e)
        {
            var message = _pomodoro.Count % 4 == 0 ? "Long! Break time." : "Break time!";

            _notifyIcon.ShowBalloonTip(750, "Pomodoro", message, ToolTipIcon.Info);


            PlaySound(Resources.BreakTime);
        }

        static void Pomodoro_Tick(object sender, System.Timers.ElapsedEventArgs e)
        {
            UpdateIcon();
        }

        static void Menu_Popup(object sender, EventArgs e)
        {
            _toggleItem.Text = _pomodoro.IsRunning ? "&Pause" : "&Start";
        }

        static void NotifyIcon_Click(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Toggle();
            }
        }

        #endregion

        #endregion
    }
}