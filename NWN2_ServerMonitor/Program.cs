using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Configuration;
using System.Web.Script.Serialization;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Xml.Serialization;
using System.Collections;

namespace ConsoleApplication1
{
    class Program
    {
        public class SysTrayApp : Form
        {
            public static List<string> WatchedServers = new List<string>();

            public static NWNMasterServerAPIClient client;
            public static NotifyIcon trayIcon;
            public static ContextMenu trayMenu;

            [STAThread]
            public static void Main()
            {
                try
                {
                    string savedServersFile = System.IO.Path.GetDirectoryName(Application.ExecutablePath) + "\\SavedServers.sav";
                    if (File.Exists(savedServersFile))
                    {
                        using (FileStream stream = new FileStream(savedServersFile, FileMode.Open))
                        {
                            XmlSerializer serialize = new XmlSerializer(typeof(List<string>));
                            SysTrayApp.WatchedServers = serialize.Deserialize(stream) as List<string>;
                        }
                    }
                }
                catch { }
                try
                {
                    client = new NWNMasterServerAPIClient("WSHttpBinding_INWNMasterServerAPI");
                    Application.Run(new SysTrayApp());
                }
                finally
                {
                    if (WatchedServers.Count > 0)
                    {
                        string savedServersFile = System.IO.Path.GetDirectoryName(Application.ExecutablePath) + "\\SavedServers.sav";
                        if(File.Exists(savedServersFile))
                            File.Delete(savedServersFile);
                        using (FileStream stream = new FileStream(savedServersFile, FileMode.OpenOrCreate))
                        {
                            XmlSerializer serialize = new XmlSerializer(typeof(List<string>));
                            serialize.Serialize(stream, WatchedServers);
                        }
                    }
                }
            }

            public SysTrayApp()
            {
                // Create a simple tray menu with only one item.
                trayMenu = new ContextMenu();
                trayMenu.MenuItems.Add("Exit", OnExit);
                trayMenu.MenuItems.Add("Show Servers", ShowServers);

                // Create a tray icon. In this example we use a
                // standard system icon for simplicity, but you
                // can of course use your own custom icon too.
                trayIcon = new NotifyIcon();
                trayIcon.Text = "Servers not yet queried.";
                trayIcon.Icon = new Icon(SystemIcons.Information, 40, 40);

                // Add menu to tray icon and show it.
                trayIcon.ContextMenu = trayMenu;
                trayIcon.MouseDoubleClick += ShowServers;
                trayIcon.Visible = true;

                if (WatchedServers.Count > 0)
                {
                    ServerWatcher.CheckServers(this, new EventArgs());
                    if (ServerWatcher.watcherLoop.Enabled == false)
                    {
                        ServerWatcher.watcherLoop.Tick += ServerWatcher.CheckServers;
                        ServerWatcher.watcherLoop.Start();
                        ServerWatcher.watcherLoop.Enabled = true;
                    }
                }
            }

            protected override void OnLoad(EventArgs e)
            {
                Visible = false; // Hide form window.
                ShowInTaskbar = false; // Remove from taskbar.

                base.OnLoad(e);
            }

            private void OnExit(object sender, EventArgs e)
            {
                Application.Exit();
                client.Close();
            }

            private void ShowServers(object sender, EventArgs e)
            {
                new ServerListForm().ShowDialog();
            }

            protected override void Dispose(bool isDisposing)
            {
                if (isDisposing)
                {
                    // Release the icon resource.
                    trayIcon.Dispose();
                }

                base.Dispose(isDisposing);
            }
        }

        public class ServerListForm : Form
        {
            private ListView box = new ListView();
            private Button watchServers = new Button() { Text = "Watch Selected Servers" };
            private int playersOn;

            public ServerListForm()
            {
                watchServers.Size = watchServers.PreferredSize;
                watchServers.Click += ButtonClicked;

                box.View = View.Details;
                box.Columns.Add("Server Name");
                box.Columns.Add("Module Name");
                box.Columns.Add("IP Address");
                box.Columns.Add("Plr /");
                box.Columns.Add("Max");
                box.FullRowSelect = true;
                try
                {
                    foreach (NWN.NWGameServer Server in SysTrayApp.client.GetOnlineServerList("NWN2"))
                    {
                        bool isSelected = SysTrayApp.WatchedServers.Contains(Server.ServerAddress);
                        box.Items.Add(new ListViewItem(new string[] { Server.ServerName, Server.ModuleName, Server.ServerAddress, Server.ActivePlayerCount.ToString(), Server.MaximumPlayerCount.ToString() }) { Selected = isSelected });
                    }
                }
                catch
                {
                    MessageBox.Show("Error:\nCould not contact the server lists. Please try again later.");
                }

                this.Controls.Add(box);
                this.Controls.Add(watchServers);
                box.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                box.ColumnClick += ColumnSort;
                int sizeX = box.Columns[0].Width + box.Columns[1].Width + box.Columns[2].Width + box.Columns[3].Width + box.Columns[4].Width + 10;
                int sizeY = box.Items[0].Bounds.Height * box.Items.Count - this.ClientRectangle.Height + this.Height + 10;
                if (sizeY == 700) sizeY = 700;
                box.Size = new Size(sizeX, sizeY);
                watchServers.Location = new Point(box.Width - watchServers.Width, box.Height);
                this.ClientSize = new Size(sizeX, sizeY + watchServers.Height);
                this.Resize += OnResized;
            }

            private void ButtonClicked(object Sender, EventArgs e)
            {
                SysTrayApp.WatchedServers.Clear();
                foreach (ListViewItem Item in box.SelectedItems)
                {
                    SysTrayApp.WatchedServers.Add(Item.SubItems[2].Text);
                }
                if (SysTrayApp.WatchedServers.Count > 0)
                {
                    ServerWatcher.CheckServers(this, new EventArgs());
                    if (ServerWatcher.watcherLoop.Enabled == false)
                    {
                        ServerWatcher.watcherLoop.Tick += ServerWatcher.CheckServers;
                        ServerWatcher.watcherLoop.Start();
                        ServerWatcher.watcherLoop.Enabled = true;
                    }
                }
                this.Close();
            }

            private void ColumnSort(object Sender, ColumnClickEventArgs e)
            {
                box.ListViewItemSorter = new ListViewItemComparer(e.Column);
            }

            private void OnResized(object Sender, EventArgs e)
            {
                int boxWidth = this.Size.Width - 10;
                int boxHeight = this.ClientRectangle.Height - this.watchServers.Height;

                box.Size = new Size(boxWidth, boxHeight);
                watchServers.Location = new Point(box.Width - watchServers.Width, box.Height);
            }
        }

        public static class ServerWatcher
        {
            public static Timer watcherLoop = new Timer() { Interval = 300 * 1000 }; // Timer ticks every five minutes.

            public static void CheckServers(object Sender, EventArgs e)
            {
                uint playersOn = 0;
                List<string> removedServers = new List<string>();
                foreach (string ServerAddress in SysTrayApp.WatchedServers)
                {
                    NWN.NWGameServer Server = null;
                    try
                    {
                        Server = SysTrayApp.client.LookupServerByAddress("NWN2", ServerAddress);
                    }
                    catch {  }
                    if (Server == null)
                    {
                        removedServers.Add(ServerAddress);
                    }
                    if (Server.Online)
                    {
                        playersOn += Server.ActivePlayerCount;
                    }
                }
                if(removedServers.Count > 0)
                {
                    foreach(string remove in removedServers)
                    {
                        SysTrayApp.WatchedServers.Remove(remove);
                    }
                }
                string number;
                if (playersOn < 10)
                    number = "0" + playersOn.ToString();
                else if (playersOn < 22)
                    number = playersOn.ToString();
                else
                    number = "21";
                string icon = System.IO.Path.GetDirectoryName(Application.ExecutablePath) + "\\ICON_" + number + ".ico";
                SysTrayApp.trayIcon.Icon = new Icon(icon);
                SysTrayApp.trayIcon.Text = String.Format("A total of {0} players are logged onto watched servers.", playersOn);
            }
        }
    }

    class ListViewItemComparer : IComparer
    {
        private int col;
        public ListViewItemComparer()
        {
            col = 0;
        }
        public ListViewItemComparer(int column)
        {
            col = column;
        }
        public int Compare(object x, object y)
        {
            return String.Compare(((ListViewItem)x).SubItems[col].Text, ((ListViewItem)y).SubItems[col].Text);
        }
    }
}