using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Management;
using System.Management.Instrumentation;
using System.IO;
using System.Xml.Serialization;
using System.Threading;
using Microsoft.Win32;
using System.Net;
using ScrapySharp.Network;

namespace GCI_Data_Usage
{
    public partial class Form1 : Form
    {
        Icon[] iconArray = new Icon[101];
        NotifyIcon activeIcon;
        private int loginErrorCounter = 0;
        private int internetPercentage = Int32.Parse(Settings1.Default["usage"].ToString());
        private IEnumerable<Service> services;
        private FormDetail detailForm;
        //private System.Windows.Forms.Timer workTimer;
		private Thread gciWorker;

        public Form1()
        {
            #region Initialization Data (username and password)
            InitializeComponent();
            textBox_Username.Text = Settings1.Default.username;
            textBox_Password.Text = Settings1.Default.password;
            numericUpDown1.Value = Settings1.Default.resetDay;
            comboBox1.Text = Settings1.Default.frequency.ToString();
            comboBox2.Text = Settings1.Default.theme;


            //set icons objects
            buildIconArray(Settings1.Default["theme"].ToString());

            //create notify icon and assign idle icon and show it
            activeIcon = new NotifyIcon();
            activeIcon.Icon = iconArray[Int32.Parse(Settings1.Default["usage"].ToString())];
            activeIcon.Visible = true;

            //create menu items
            MenuItem programNameMenuItem = new MenuItem("GCI internet usage BETA v1.1.2");
            MenuItem programNameMenuItem2 = new MenuItem("Check for updates");
            MenuItem detailMenuItem = new MenuItem("Detailed usage");
            MenuItem quitMenuItem = new MenuItem("Quit");
            ContextMenu contextMenu = new ContextMenu();  //create the contect menu
            contextMenu.MenuItems.Add(programNameMenuItem);  //add items to the context menu
            contextMenu.MenuItems.Add(programNameMenuItem2);  //add items to the context menu
            contextMenu.MenuItems.Add(detailMenuItem);
            contextMenu.MenuItems.Add(quitMenuItem);
            activeIcon.ContextMenu = contextMenu;  //link context menu to the tray icon
            //set initial icon hover text.
            if(System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                activeIcon.Text = "Connected! Loading";
            } else {
                activeIcon.Text = "Waiting for internet";
            }


            //Make the first menu item unclickable
            contextMenu.MenuItems[0].Enabled = false;

            //click event for icon to open options
            activeIcon.Click += activeIcon_Click;

            detailMenuItem.Click += DetailMenuItem_Click;

            //wire up quit button to close application
            quitMenuItem.Click += QuitMenuItem_Click;

            //wire up quit button to close application
            programNameMenuItem2.Click += programNameMenuItem2_Click;

            //Hide the form because we don't need it yet
            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;

            //start worker thread that polls GCI
            gciWorker = new Thread(new ThreadStart(GCIActivityThread));
            gciWorker.Start();

            //adds an additional "on complete" task for the webbrowser
            //webBrowser1.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(webBrowser1_DocumentCompleted);
            //webBrowser1.Navigate("https://login.gci.com");

            //workTimer = new System.Windows.Forms.Timer();
            //workTimer.Interval = 1000 * 60 * 60 * Int32.Parse(Settings1.Default["frequency"].ToString());
            //workTimer.Tick += Timer_Tick;
            //workTimer.Start();
            #endregion
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            UpdateUsage();
        }

        private void DetailMenuItem_Click(object sender, EventArgs e)
        {
            if (detailForm == null) detailForm = new FormDetail();
            detailForm.Detail = services;
            detailForm.Show();
        }

        public void buildIconArray(string name)
        {
            for (int i = 0; i <= 100; i++)
            {
                System.IO.Stream st;
                var a = System.Reflection.Assembly.GetExecutingAssembly();
                st = a.GetManifestResourceStream("DataUsage.Resources.icons." + name + "." + i.ToString() + ".ico");
                iconArray[i] = new Icon(st);
            }
        }

        private void activeIcon_Click(object sender, EventArgs e)
        {
            this.Activate();
            this.WindowState = FormWindowState.Normal;
            this.ShowInTaskbar = true;
        }

        private void QuitMenuItem_Click(object sender, EventArgs e)
        {
            //workTimer.Stop();
			gciWorker.Abort();
            activeIcon.Dispose();
            this.Close();
        }

        private void programNameMenuItem2_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.allthedurbin.com/gci-usage-app/");
        }

        public void GCIActivityThread()
        {
            //wbemtest command to add u/d telemetrics later
            //main loop
            while (true)
            {
                int internetError = 0;
                while(!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
                {
                    if(internetError == 0)
                    {
                        activeIcon.Text = "Waiting for internet";
                        internetError = 1;
                    }
                    
                    Thread.Sleep(60000);
                }

                UpdateUsage();

                //webBrowser1.Navigate("https://apps.gci.com/um/overview");
                Thread.Sleep(1000 * 60 * 60 * Int32.Parse(Settings1.Default["frequency"].ToString()));
            }
        }

        private void UpdateUsage()
        {
            try
            {
                services = GCIUsageChecker.Check(Settings1.Default.username, Settings1.Default.password);

                var internet = services.FirstOrDefault(s => s.Category == "Internet");
                var wireless = services.FirstOrDefault(s => s.Category == "Wireless");

                int internetPercentage = Convert.ToInt32(Math.Round(internet.Total) * 100 / internet.Cap);
                activeIcon.Icon = iconArray[internetPercentage];
                Settings1.Default["usage"] = Int32.Parse(internetPercentage.ToString());
                Settings1.Default.Save();
                activeIcon.Text = string.Format("Internet: {0}GB/{1}GB", internet.Total, internet.Cap);
                lblHome.Text = String.Format("Home: {0}GB / {1}GB", internet.Total, internet.Cap);
                progressBar2.Maximum = (int)internet.Cap;
                progressBar2.Value = (int)internet.Total;
                if (wireless != null)
                {
                    lblWireless.Text = String.Format("Wireless: {0}GB / {1}GB", wireless.Total, wireless.Cap);
                    activeIcon.Text += "\n";
                    activeIcon.Text += string.Format("Wireless: {0}GB /{1} GB", wireless.Total, wireless.Cap);
                }
                else
                    lblWireless.Text = "Wireless: N/A";

                var startDt = new DateTime(DateTime.Now.Year, DateTime.Now.Month, Settings1.Default.resetDay);
                var endDt = startDt.AddMonths(1);
                if (startDt > DateTime.Now)
                {
                    startDt = startDt.AddMonths(-1);
                    endDt = endDt.AddMonths(-1);
                }
                var expected = 1.0 * (DateTime.Now - startDt).TotalDays / (endDt - startDt).TotalDays * internet.Cap;
                activeIcon.Text += String.Format("\nExpected: {0:#.00} for {1:#.#} days", expected, (DateTime.Now - startDt).TotalDays);
                lblLeft.Text = String.Format("Expected: {0:#.00} for {1:#.#} days", expected, (DateTime.Now - startDt).TotalDays);
            }
            catch (UsageException)
            {
                if (loginErrorCounter < 2)
                {
                    loginErrorCounter += 1;
                }
                else
                {
                    //This will display the login page if it cannot log in, no matter what.
                    errorStop("Unable To Login, Check your username and password then click save");
                }
            }
        }

        private void errorStop(string message)
        {
            //bring window to front, unminimize, focus
            MessageBox.Show(message);
            progressBar1.Value = 0;
            this.WindowState = FormWindowState.Normal;
            this.ShowInTaskbar = true;
        }

        //override the close button to ask if you want to close. most people want to minimze
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            if (e.CloseReason == CloseReason.WindowsShutDown) return;

            // Confirm user wants to close
            switch (MessageBox.Show(this, "Are you sure you want to close?", "Minimizing", MessageBoxButtons.YesNo))
            {
                case DialogResult.No:
                    e.Cancel = true;
                    break;
                default:
                    activeIcon.Dispose();
                    //workTimer.Stop();
					gciWorker.Abort();
                    break;
            }
        }

        //override default minimze action to hide the app again
        private void Form1_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                this.WindowState = FormWindowState.Minimized;
                this.ShowInTaskbar = false;
            }
        }

        //save button clicked
        private void button1_Click_1(object sender, EventArgs e)
        {
            Settings1.Default.username = textBox_Username.Text;
            Settings1.Default.password = textBox_Password.Text;
            Settings1.Default.Save();
            Application.Restart();
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(activeIcon != null)
            {
               Settings1.Default.theme = comboBox2.Text;
               buildIconArray(comboBox2.Text);
               activeIcon.Icon = iconArray[internetPercentage];

            } 
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Settings1.Default.frequency = Int32.Parse(comboBox1.Text.ToString());
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
