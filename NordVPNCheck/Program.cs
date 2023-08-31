using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Http;
using System.Drawing;
using System.Configuration;
using Microsoft.Win32;

namespace NordVPNCheck
{
    static class Program
    {
        static void Main()
        {   
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());

            if (bool.TryParse(ConfigurationManager.AppSettings["Startup"], out bool runAtStartup) && runAtStartup)
            {
                RegistryKey rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                rkApp.SetValue("NordVPNConnectionChecker", Application.ExecutablePath);
            }
        }
    }
    public partial class MainForm : Form
    {
        private const string UrlToCheck = "https://nordvpn.com/wp-admin/admin-ajax.php?action=get_user_info_data";  // Replace with your URL
        private const string TextToCheck = "\"status\":true";      // Replace with your selected text

        private Timer timer;
        private HttpClient httpClient;
        private NotifyIcon trayIcon;

        public MainForm()
        {            
            //AllocConsole();
            InitializeApp();
        }

        //[System.Runtime.InteropServices.DllImport("kernel32.dll")]
        //private static extern bool AllocConsole();

        private void InitializeApp()
        {

            Console.WriteLine("App started");

            this.WindowState = FormWindowState.Minimized;         
            this.Icon = new Icon("app.ico");
            this.Visible = false;
            this.ShowInTaskbar = false;

            // Initialize HttpClient
            httpClient = new HttpClient();
            
            // Initialize timer to run every 30 seconds
            timer = new Timer();
            timer.Interval = 30 * 1000; // this is set in miliseconds
            timer.Tick += async (sender, e) => await CheckUrlAndShowIcon();
            timer.Start();
           
            // Initialize system tray icon
            trayIcon = new NotifyIcon();
            trayIcon.Icon = new Icon("warning.ico");  // Replace with your icon file           
            trayIcon.Visible = true;
            trayIcon.Text = "Status Uknown!";

            trayIcon.ContextMenuStrip = new ContextMenuStrip();

            trayIcon.ContextMenuStrip.Items.AddRange(new ToolStripItem[]
            {
                new ToolStripMenuItem("EXIT", null, new EventHandler(Exit), "Close")
            });

            CheckUrlAndShowIcon();
        }

        private void Exit(object sender, EventArgs e)
        {
            this.Close();
        }

        private async Task CheckUrlAndShowIcon()
        {
            try
            {
                HttpResponseMessage response = await httpClient.GetAsync(UrlToCheck);
                string responseBody = await response.Content.ReadAsStringAsync();

                if (responseBody.Contains(TextToCheck))
                {
                    Console.WriteLine("Found!");
                    // Selected text is found in the response
                    trayIcon.Icon = new Icon("ok.ico");  // Replace with success icon
                    trayIcon.Text = "You are protected!";
                }
                else
                {
                    Console.WriteLine("NOT Found!");
                    // Selected text is not found in the response
                    trayIcon.Icon = new Icon("nook.ico");  // Replace with failure icon
                    trayIcon.Text = "You are NOT protected!";

                }
            }
            catch (Exception ex)            {
                trayIcon.Icon = new Icon("warning.ico");  // Replace with error icon
                trayIcon.Text = "Error checking status";
            }
        }
    }   

}