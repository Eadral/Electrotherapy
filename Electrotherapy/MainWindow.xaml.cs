using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.ComponentModel;
using System.Net;
using System.Runtime.InteropServices;



namespace Electrotherapy {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window {

        private System.ComponentModel.Container components;
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private System.Windows.Forms.ContextMenu contextMenu;

        

        public MainWindow() {
            InitializeComponent();

            components = new System.ComponentModel.Container();
            contextMenu = new System.Windows.Forms.ContextMenu();

            notifyIcon = new System.Windows.Forms.NotifyIcon(components) {
                Icon = Properties.Resources.lightning,
                ContextMenu = contextMenu,
                Text = "Electrotherapy",
                Visible = true,                
            };
            notifyIcon.DoubleClick += new System.EventHandler(this.NotifyIcon_DoubleClick);

            progressBar.DataContext = progressbarValue;

            MainLoop();
        }

        private void NotifyIcon_DoubleClick(object Sender, EventArgs e) {
            switch (this.Visibility) {
                case Visibility.Visible:
                    this.Hide();
                    break;
                default:
                    this.Show();
                    break;
            }
        }

        private void HTTPGet(String url) {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            request.Timeout = 5000;
            try {
                request.GetResponse();
            } catch {
                Console.WriteLine("Error: request.GetResponse();");
            }
        }

        private void Vibrate(float strength = 0.5f) {
            String url = String.Format(
                @"https://pavlok-unlocked.herokuapp.com/public/do/361d088c3/vibrate/{0}", (int)(strength*255));

            HTTPGet(url);
        }

        private void Zap(float strength = 0.5f) {
            String url = String.Format(
                @"https://pavlok-unlocked.herokuapp.com/public/do/361d088c3/zap/{0}", (int)(strength * 255));

            HTTPGet(url);
        }

        private void Button_Zap(object sender, RoutedEventArgs e) {
            Zap();
        }

        private void Button_Vibrate(object sender, RoutedEventArgs e) {
            Vibrate();
        }

        private POINT lastCursor;
        private POINT nowCursor;

        private int countDownCursor;
        private int countDownCursorInit = 60;
        private BindingInt progressbarValue = new BindingInt();

        private async void MainLoop() {
            Start();
            while (true) {
                Update();
                await Task.Delay(1000);
            }
        }
           
        private void Start() {
            GetCursorPos(out lastCursor);
            UpdateCountDown(countDownCursorInit);
        }

        private void Update() {

            GetCursorPos(out nowCursor);
            if (nowCursor.X == lastCursor.X && nowCursor.Y == lastCursor.Y) {
                if (countDownCursor > 0)
                    UpdateCountDown(countDownCursor - 1);
                else
                    Remind();
            } else {
                UpdateCountDown(countDownCursorInit);
                remindTimes = 0;
            }
            lastCursor.X = nowCursor.X;
            lastCursor.Y = nowCursor.Y;

#if DEBUG
            //Console.WriteLine("Update.");
            //Console.WriteLine(String.Format("Cursor X: {0}, Y: {1}.", nowCursor.X, nowCursor.Y));
            //Console.WriteLine(String.Format("CountDown: {0}", countDownCursor));
#endif
        }

        private int remindTimes;
        private int zapStartFrom = 3;
        private void Remind() {
            if (remindTimes < zapStartFrom)
                Vibrate();
            else {
                float strength = remindTimes / 10f;
                if (strength > 255)
                    strength = 255;
                Zap(strength);
            }
            remindTimes++;
        }
         
        private void UpdateCountDown(int value) {
            countDownCursor = value;
            progressbarValue.IntValue = (int)(100*((float)value / (float)countDownCursorInit));
        }

        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out POINT lpPoint);

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT {
            public int X;
            public int Y;
            public POINT(int x, int y) {
                this.X = x;
                this.Y = y;
            }
        }

        class BindingInt : INotifyPropertyChanged {
            private int intvalue;
            public event PropertyChangedEventHandler PropertyChanged;
            public int IntValue {
                get { return intvalue; }
                set {
                    intvalue = value;
                    OnPropertyChanged("IntValue");
                }
            }

            protected void OnPropertyChanged(string name) {
                PropertyChangedEventHandler handler = PropertyChanged;
                if (handler != null) {
                    handler(this, new PropertyChangedEventArgs(name));
                }
            }
        }
    }
}
