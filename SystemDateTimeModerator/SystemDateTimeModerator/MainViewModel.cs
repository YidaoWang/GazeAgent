using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;

namespace SystemDateTimeModerator
{
    [StructLayout(LayoutKind.Sequential)]
    public struct SystemTime
    {
        public ushort wYear;
        public ushort wMonth;
        public ushort wDayOfWeek;
        public ushort wDay;
        public ushort wHour;
        public ushort wMinute;
        public ushort wSecond;
        public ushort wMiliseconds;
    }

    public class MainViewModel : INotifyPropertyChanged
    {
        public MainViewModel()
        {
            var timer = new Timer(100);
            timer.Elapsed += (sender, e) =>
            {
                RaisePropertyChanged(nameof(SystemTime));
            };
            timer.Start();

            SetTime(DateTime.Now);

            SettingCommand = new DelegateCommand(() =>
            {
                try
                {
                    var settingTime = new DateTime(int.Parse(SettingYear), int.Parse(SettingMonth),
                        int.Parse(SettingDay), int.Parse(SettingHour), int.Parse(SettingMinute),
                        int.Parse(SettingSecond), 0);
                    SetNowDateTime(settingTime);
                }
                catch(Exception e)
                {
                    MessageBox.Show(e.Message);
                }
            });
        }

        private void SetTime(DateTime dateTime)
        {
            SettingYear = dateTime.Year.ToString();
            SettingMonth = dateTime.Month.ToString();
            SettingDay = dateTime.Day.ToString();
            SettingHour = dateTime.Hour.ToString();
            SettingMinute = dateTime.Minute.ToString();
            SettingSecond = "00";            
        } 

        public string SettingYear { get; set; }
        public string SettingMonth { get; set; }
        public string SettingDay { get; set; }
        public string SettingHour { get; set; }
        public string SettingMinute { get; set; }
        public string SettingSecond { get; set; }

        public DateTime SystemTime
        {
            get
            {
                return DateTime.Now;
            }
        }
        public ICommand SettingCommand { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        [DllImport("kernel32.dll")]
        public static extern bool SetLocalTime(
            ref SystemTime sysTime);

        /// <summary>
        /// 現在のシステム日時を設定する
        /// </summary>
        /// <param name="dt">設定する日時</param>
        private static void SetNowDateTime(DateTime dt)
        {
            //システム日時に設定する日時を指定する
            SystemTime sysTime = new SystemTime();
            sysTime.wYear = (ushort)dt.Year;
            sysTime.wMonth = (ushort)dt.Month;
            sysTime.wDay = (ushort)dt.Day;
            sysTime.wHour = (ushort)dt.Hour;
            sysTime.wMinute = (ushort)dt.Minute;
            sysTime.wSecond = (ushort)dt.Second;
            sysTime.wMiliseconds = (ushort)dt.Millisecond;
            //システム日時を設定する
            SetLocalTime(ref sysTime);
        }

    }
}
