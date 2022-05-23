using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PhoneLink
{
    /// <summary>
    /// ico.xaml 的交互逻辑
    /// </summary>

    public partial class ico : Window
    {
        [DllImport("user32.dll")]
        public static extern bool AddClipboardFormatListener(IntPtr hwnd);

        [DllImport("user32.dll")]
        public static extern bool RemoveClipboardFormatListener(IntPtr hwnd);

        [DllImport("user32.dll", EntryPoint = "ShowWindow")]
        public static extern bool ShowWindow(int hwnd, int nCmdShow);
        public const int SW_HIDE = 0;

        const int WM_CLIPBOARDUPDATE = 0x031D;

        IntPtr windowsHeader;

        serverLink? link = null;


        /// <summary>
        /// 隐藏控件获得焦点时的虚线框
        /// </summary>
        /// <param name="root"></param>
        public static void HideBoundingBox(object root)
        {
            Control control = root as Control;
            if (control != null)
            {
                control.FocusVisualStyle = null;
            }

            if (root is DependencyObject)
            {
                foreach (object child in LogicalTreeHelper.GetChildren((DependencyObject)root))
                {
                    HideBoundingBox(child);
                }
            }
        }
        public ico()
        {
            InitializeComponent();
            HideBoundingBox(this);
            DataContext = this;
        }
        public void skinSwitch(object sender, RoutedEventArgs e)
        {
            if (contex.Style == FindResource("ContextMenuStyle") as Style)
            {
                contex.Style = FindResource("ContextMenuStyleBlue") as Style;
            }
            else
            {
                contex.Style = FindResource("ContextMenuStyle") as Style;
            }
        }
        private void ServerLinkFun(object sender, RoutedEventArgs e)
        {
            string ?temporaryText;
            try
            {
                link = new("106.75.62.81", 2564, 1, "PC");

                Thread.Sleep(20);

                ThreadStart childref = new ThreadStart(SyncServer);
                Thread childThread = new Thread(childref);
                childThread.TrySetApartmentState(ApartmentState.STA);
                childThread.Start();

                temporaryText = link.GetTemporaryData();
            }
            catch (Exception ex)
            {
                message("连接服务器异常", ex.Message);
                ServerText.Header = "状态:未连接";
                return;
            }


            message("成功连接服务器", "暂存区:" + temporaryText);
            ServerText.Header = "状态:已连接";
        }
        void SyncServer()
        {
            if (link == null)
                return;
            while (true)
            {
                string str;
                try
                {
                    str = link.Out.ReadString();
                }
                catch (Exception e)
                {
                    link = null;
                    message(e.Message, "");
                    return;
                }
                switch (str)
                {
                    case "textSync":
                        link.Out.SendString("OK");
                        string? data = link.Out.ReadString();
                        if (data == null)
                        {
                            break;
                        }
                        SyncText = data;
                        Clipboard.SetData(DataFormats.UnicodeText, data);
                        message("来自服务器的剪切板", data);
                        break;
                    case "SyncImage":
                        link.Out.SendString("OK");
                        string imageName = link.Out.ReadString();
                        link.Out.SendString("OK");
                        int dataLen = 0;
                        byte[] imageData = link.Out.Read(ref dataLen);
                        MemoryStream mem = new MemoryStream(imageData);

                        BitmapImage bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.StreamSource = mem;
                        bitmapImage.EndInit();

                        Clipboard.SetImage(bitmapImage);
                        break;
                    default:
                        break;
                }
            }
        }
        void GetTemporaryData()
        {
            if (link == null)
            {
                return;
            }
            link.In.SendString("GetTemporaryData");
            string? data = link.In.ReadString();
            if (data != "Ok")
            {
                return;
            }
        }
        string SyncText = new("");
        bool SyncThreadSign = false;
        public static BitmapSource ChangeBitmapToBitmapSource(Bitmap bmp)
        {
            BitmapSource returnSource;
            try
            {
                returnSource = Imaging.CreateBitmapSourceFromHBitmap(bmp.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            catch
            {
                returnSource = null;
            }
            return returnSource;
        }
        private void Sync(IDataObject iData)
        {
            if (link == null)
            {
                return;
            }
            if (SyncThreadSign == true)
            {
                return;
            }

            SyncThreadSign = true;
            Thread.Sleep(500);
            if (iData.GetDataPresent(DataFormats.UnicodeText))
            {
                string? text;
                try
                {
                    text = iData.GetData(DataFormats.UnicodeText) as string;
                }
                catch (Exception e)
                {
                    message(e.Message);
                    SyncThreadSign = false;
                    return;
                }
                if (text == null)
                {
                    SyncThreadSign = false;
                    return;
                }
                if (text == SyncText)
                {
                    SyncThreadSign = false;
                    return;
                }
                SyncText = text;
                if (SyncText == null)
                    return;
                message("同步剪切板", link.textSync(SyncText));
                SyncThreadSign = false;
                return;
            }
            if (iData.GetDataPresent(DataFormats.Bitmap))
            {
                //var image = iData.GetData(DataFormats.Bitmap) as Bitmap;

                //MemoryStream ms = new MemoryStream();

                var image = Clipboard.GetImage();

                if (image == null)
                {
                    SyncThreadSign = false;
                    return;
                }

                message("同步图片", link.imageSync(image));
                SyncThreadSign = false;
                return;
            }
            SyncThreadSign = false;
        }
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case WM_CLIPBOARDUPDATE:
                    IDataObject iData = Clipboard.GetDataObject();
                    ThreadStart childref = new ThreadStart(() => Sync(iData));
                    Thread childThread = new Thread(childref);
                    childThread.TrySetApartmentState(ApartmentState.STA);
                    childThread.Start();
                    break;
                default:
                    break;
            }

            return IntPtr.Zero;
        }
        void message(string data)
        {
            new ToastContentBuilder()
                .AddArgument("action", "viewConversation")
                .AddArgument("conversationId", 9813)
                .AddText(data)
                .Show();
        }
        void message(string title, string data)
        {
            new ToastContentBuilder()
                .AddText(title)
                .AddText(data)
                .Show();
        }
        public void ExitThis(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }
        public void MonitorInit(object sender, EventArgs e)
        {
            WindowInteropHelper wndHelper = new WindowInteropHelper(this);
            windowsHeader = wndHelper.Handle;

            AddClipboardFormatListener((IntPtr)windowsHeader);

            ShowWindow(windowsHeader.ToInt32(), 0);

            base.OnSourceInitialized(e);

            HwndSource source = PresentationSource.FromVisual(this) as HwndSource;

            source.AddHook(WndProc);
            
            contexIcon.Visibility = Visibility.Visible;
            try
            {
                ServerLinkFun(null, null);
            }
            catch (Exception ex)
            {
                return;
            }
            ServerText.Header = "状态:已连接";
        }
        public void Monitor(object sender, EventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            if (menuItem != null)
            {
                if (menuItem.Header.ToString() == "监听剪切板")
                {
                    AddClipboardFormatListener((IntPtr)new WindowInteropHelper(this).Handle);
                    menuItem.Header = "取消监听";
                }
                else
                {
                    RemoveClipboardFormatListener((IntPtr)new WindowInteropHelper(this).Handle);
                    menuItem.Header = "监听剪切板";
                }
            }
        }
        public void SetImage(BitmapSource image)
        {
            ImageBox.Source = image;
        }
    }

}
