using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace keypress
{
    partial class Form1
    {

        private const int autoKey = (int)Keys.NumPad1;
        
        //private Keys[] autoKeys = { Keys.F, Keys.F, Keys.R}; // 按键
        //private int[] waitTime = { 0, 0, 0}; // 按下之前等待时间
        //private int[] pressTime = { 285, 10, 1}; // 松开之前等待时间

        //private Keys[] autoKeys = { Keys.D3, Keys.F, Keys.T, Keys.R, Keys.D2, Keys.T }; // 按键
        //private int[] waitTime = { 0, 0, 0, 0, 0, 0 }; // 按下之前等待时间
        //private int[] pressTime = { 1, 5, 1, 1, 1, 1 }; // 松开之前等待时间

        //private Keys[] autoKeys = { Keys.D2, Keys.D3, Keys.D9, Keys.F, Keys.T, Keys.R }; // 按键
        //private int[] waitTime = { 0, 0, 0, 0, 0, 0 }; // 按下之前等待时间
        //private int[] pressTime = { 10, 10, 10, 10, 10, 10 }; // 松开之前等待时间

        private Keys[] autoKeys = { Keys.D3, Keys.D1, Keys.F, Keys.X, Keys.T }; // 按键
        private int[] waitTime = { 0, 0, 0, 0, 0 }; // 按下之前等待时间
        private int[] pressTime = { 10, 10, 10, 10, 10 }; // 松开之前等待时间

        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private Icon mNetTrayIcon = new Icon("Resources/D.ico");
        private NotifyIcon TrayIcon;
        private ContextMenu notifyiconMnu;

        private MenuItem PauseMenuItem;

        private static volatile bool isRun = false;

        private static Thread keyLoop;

        private void Initializenotifyicon()
        {
            //设定托盘程序的各个属性 
            TrayIcon = new NotifyIcon();
            TrayIcon.Icon = mNetTrayIcon;
            TrayIcon.Text = "自动按键" + "\n" + "By：DDQ";
            TrayIcon.Visible = true;
            TrayIcon.Click += new System.EventHandler(this.click);

            //定义一个MenuItem数组，并把此数组同时赋值给ContextMenu对象 
            MenuItem[] mnuItms = new MenuItem[3];
            mnuItms[0] = new MenuItem();
            mnuItms[0].Text = "暂停";
            mnuItms[0].Click += new System.EventHandler(this.PauseSelect);
            PauseMenuItem = mnuItms[0];

            mnuItms[1] = new MenuItem("-");

            mnuItms[2] = new MenuItem();
            mnuItms[2].Text = "退出";
            mnuItms[2].Click += new System.EventHandler(this.ExitSelect);
            mnuItms[2].DefaultItem = true;

            notifyiconMnu = new ContextMenu(mnuItms);
            TrayIcon.ContextMenu = notifyiconMnu;

        }

        private void InitKeyLoopThread()
        {
            keyLoop = new Thread(delegate () {

                int i = 0;

                while (true) {

                    if (isRun)
                    {
                        i = i % autoKeys.Length;

                        KeyPause(waitTime[i]);
                        keybd_event((Keys)autoKeys[i], 0, 0, 0); // 按下 0：表示按下 1：扩展键

                        KeyPause(pressTime[i]);
                        keybd_event((Keys)autoKeys[i], 0, 2, 0); // 松开 2：表示松开

                        ++i;
                    }
                    else {
                       i = 0;
                       KeyPause(Timeout.Infinite);
                    }
                }
            });
            keyLoop.IsBackground = true;
            keyLoop.Start();
        }

        private void KeyPause(int ms) {
            try
            {
                Thread.Sleep(ms);
            }
            catch (ThreadInterruptedException e) { }
        }

        public void click(object sender, System.EventArgs e)
        {
            //MessageBox.Show("托盘程序左键事件响应");
        }

        public void PauseSelect(object sender, System.EventArgs e)
        {
            // 暂停
            if (hHook != 0)
            {
                HookStop();
                PauseMenuItem.Checked = true;
            }
            else {
                HookStart();
                PauseMenuItem.Checked = false;
            }
        }

        public void ExitSelect(object sender, System.EventArgs e)
        {
            HookStop();

            //隐藏托盘程序中的图标 
            TrayIcon.Visible = false;
            //关闭系统 
            this.Close();
        }

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        // 初始化UI
        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(320, 56);
            this.ControlBox = false;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.WindowState = System.Windows.Forms.FormWindowState.Minimized;

            this.Name = "auto key press";
            this.ShowInTaskbar = false;
            this.Text = "自动按键";
            this.ResumeLayout(false);

            Initializenotifyicon();
            InitKeyLoopThread();
            HookStart();
        }

        #endregion

        public delegate int HookProc(int nCode, Int32 wParam, IntPtr lParam);

        // 拦截后返回的结构体
        [StructLayout(LayoutKind.Sequential)]
        public class HookStruct
        {
            public int vkCode; // key code
            public int scanCode; // A hardware scan code for the key. 
            public int flags;
            public int time;
            public int dwExtraInfo;
        }

        public HookProc hookProc;

        // Windows预定义了拦截关键数字
        public const int WH_KEYBOARD_LL = 13; // 13标识拦截键盘
        public const int WH_MOUSE_LL = 14; // 14标识拦截鼠标

        private static int hHook;

        // 添加Windows的hook
        [DllImport("user32.dll")]
        private extern static Int32 SetWindowsHookEx(Int32 hookType, HookProc hookProc, IntPtr hModel, Int32 threadID);

        // 移除Windows的hook
        [DllImport("user32.dll")]
        private extern static bool UnhookWindowsHookEx(Int32 hWnd);

        // 执行下一个hook
        [DllImport("user32.dll")]
        private extern static Int32 CallNextHookEx(Int32 hHook, Int32 nCode, Int32 wParam, IntPtr lParam);

        // 获取当前执行线程id 暂时不用
        [DllImport("kernel32.dll")]
        private static extern int GetCurrentThreadId();

        // 获取模块句柄
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetModuleHandle(string name);

        // 模拟按键
        [DllImport("user32.dll", EntryPoint = "keybd_event", SetLastError = true)]
        public static extern void keybd_event(Keys bVk, byte bScan, uint dwFlags, uint dwExtraInfo);

        public void HookStart()
        {

            if (hHook != 0)
                HookStop();

            hookProc = new HookProc(OnHookProc);
            IntPtr ptr = GetModuleHandle(Process.GetCurrentProcess().MainModule.ModuleName);

            hHook = SetWindowsHookEx(WH_KEYBOARD_LL, hookProc, ptr, 0);

            Console.WriteLine("HookStart" + hHook);

            if (hHook == 0) // 返回0是没有添加成功
                HookStop();
        }


        public void HookStop()
        {
            if (hHook != 0)
            {
                bool ret = UnhookWindowsHookEx(hHook);
                if (ret) hHook = 0;

                Console.WriteLine("HookStop");
            }
        }

        // windows hook回调
        private static int OnHookProc(int nCode, int wParam, IntPtr lParam)
        {

            if (nCode >= 0)

            {
                HookStruct hookStruct = (HookStruct)Marshal.PtrToStructure(lParam, typeof(HookStruct));

                if (hookStruct.vkCode == autoKey)
                {
                    if ((hookStruct.flags >> 7) == 0)
                    {
                        if (isRun)
                            return 1;// 不继续执行其他hook

                        isRun = true;
                        keyLoop.Interrupt();
                        Console.WriteLine("OnHookProc key loop start");
                    }
                    else if ((hookStruct.flags >> 7) == 1 && isRun)
                    {
                        isRun = false;
                        Console.WriteLine("OnHookProc key loop end");
                    }
                }
                else {
                    //Console.WriteLine("other " + hookStruct.vkCode + " " + (hookStruct.flags));
                }
            }

            return CallNextHookEx(hHook, nCode, wParam, lParam);
        }
    }
}

