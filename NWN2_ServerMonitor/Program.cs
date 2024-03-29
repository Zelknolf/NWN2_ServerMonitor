﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Configuration;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Xml.Serialization;
using System.Collections;
using System.Resources;

using NWN2_ServerMonitor.com.valhallalegends.mst.api1;

namespace NWN2_ServerMonitor
{
    class Program
    {
        public class SysTrayApp : Form
        {
            public static List<string> WatchedServers = new List<string>();
            public static ServerListForm serverLists = null;

            public static NotifyIcon trayIcon;
            public static ContextMenu trayMenu;

            [STAThread]
            public static void Main()
            {
                // First, make sure it's not already running. End early if it is.
                try
                {
                    System.Diagnostics.Process[] processes = System.Diagnostics.Process.GetProcessesByName("NWN2_ServerMonitor");
                    if (processes.Length > 1)
                    {
                        return;
                    }
                }
                catch{ }

                // Next, see if we've saved any server lists.
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

                // And then we open the actual main part of the application.
                try
                {
                    Application.Run(new SysTrayApp());
                }

                // When we're done, try to save.
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
            }

            private void ShowServers(object sender, EventArgs e)
            {
                if (serverLists == null)
                    serverLists = new ServerListForm();
                serverLists.ShowDialog();
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
                PopulateListBox(this, new EventArgs());
                this.Resize += OnResized;
                this.Shown += PopulateListBox;
            }

            public void PopulateListBox(object Sender, EventArgs e)
            {
                box.Clear();
                box.View = View.Details;
                box.Columns.Add("Server Name");
                box.Columns.Add("Module Name");
                box.Columns.Add("IP Address");
                box.Columns.Add("Plr /");
                box.Columns.Add("Max");
                box.FullRowSelect = true;
                try
                {
                    foreach (NWGameServer Server in new NWNMasterServerAPI().GetOnlineServerList("NWN2"))
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
                    NWGameServer Server = null;
                    try
                    {
                        Server = new NWNMasterServerAPI().LookupServerByAddress("NWN2", ServerAddress);
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
                if(playersOn > 21)          SysTrayApp.trayIcon.Icon = PopulationIcons.ICON_21;
                else if (playersOn == 20)   SysTrayApp.trayIcon.Icon = PopulationIcons.ICON_20;
                else if (playersOn == 19)   SysTrayApp.trayIcon.Icon = PopulationIcons.ICON_19;
                else if (playersOn == 18)   SysTrayApp.trayIcon.Icon = PopulationIcons.ICON_18;
                else if (playersOn == 17)   SysTrayApp.trayIcon.Icon = PopulationIcons.ICON_17;
                else if (playersOn == 16)   SysTrayApp.trayIcon.Icon = PopulationIcons.ICON_16;
                else if (playersOn == 15)   SysTrayApp.trayIcon.Icon = PopulationIcons.ICON_15;
                else if (playersOn == 14)   SysTrayApp.trayIcon.Icon = PopulationIcons.ICON_14;
                else if (playersOn == 13)   SysTrayApp.trayIcon.Icon = PopulationIcons.ICON_13;
                else if (playersOn == 12)   SysTrayApp.trayIcon.Icon = PopulationIcons.ICON_12;
                else if (playersOn == 11)   SysTrayApp.trayIcon.Icon = PopulationIcons.ICON_11;
                else if (playersOn == 10)   SysTrayApp.trayIcon.Icon = PopulationIcons.ICON_10;
                else if (playersOn == 09)   SysTrayApp.trayIcon.Icon = PopulationIcons.ICON_09;
                else if (playersOn == 08)   SysTrayApp.trayIcon.Icon = PopulationIcons.ICON_08;
                else if (playersOn == 07)   SysTrayApp.trayIcon.Icon = PopulationIcons.ICON_07;
                else if (playersOn == 06)   SysTrayApp.trayIcon.Icon = PopulationIcons.ICON_06;
                else if (playersOn == 05)   SysTrayApp.trayIcon.Icon = PopulationIcons.ICON_05;
                else if (playersOn == 04)   SysTrayApp.trayIcon.Icon = PopulationIcons.ICON_04;
                else if (playersOn == 03)   SysTrayApp.trayIcon.Icon = PopulationIcons.ICON_03;
                else if (playersOn == 02)   SysTrayApp.trayIcon.Icon = PopulationIcons.ICON_02;
                else if (playersOn == 01)   SysTrayApp.trayIcon.Icon = PopulationIcons.ICON_01;
                else                        SysTrayApp.trayIcon.Icon = PopulationIcons.ICON_00;

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