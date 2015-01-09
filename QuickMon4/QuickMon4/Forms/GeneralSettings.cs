﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace QuickMon
{
    public partial class GeneralSettings : Form
    {
        internal class RemoteAgentInfo
        {
            public RemoteAgentInfo()
            {
                PortNumber = 8181;
            }
            public string Computer { get; set; }
            public int PortNumber { get; set; }
            public static RemoteAgentInfo FromString(string remoteAgentStr)
            {
                RemoteAgentInfo newRemoteAgentInfo = new RemoteAgentInfo();
                if (remoteAgentStr.Contains(":"))
                {
                    newRemoteAgentInfo.Computer = remoteAgentStr.Substring(0, remoteAgentStr.IndexOf(":"));
                    if (remoteAgentStr.Substring(newRemoteAgentInfo.Computer.Length + 1).IsNumber())
                        newRemoteAgentInfo.PortNumber = int.Parse(remoteAgentStr.Substring(newRemoteAgentInfo.Computer.Length + 1));
                    else
                        newRemoteAgentInfo.PortNumber = 8181;
                }
                else
                {
                    newRemoteAgentInfo.Computer = remoteAgentStr;
                    newRemoteAgentInfo.PortNumber = 8181;
                }
                return newRemoteAgentInfo;
            }
            public override string ToString()
            {
                return Computer + ":" + PortNumber.ToString();
            }
        }

        public GeneralSettings()
        {
            InitializeComponent();
        }

        private bool loading = false;
        private bool freChanging = false;
        private bool remoteListLoaded = false;

        public int PollingFrequencySec { get; set; }
        public bool PollingEnabled { get; set; }

        #region Form events
        private void GeneralSettings_Load(object sender, EventArgs e)
        {
            loading = true;
            concurrencyLevelNnumericUpDown.Value = Properties.Settings.Default.ConcurrencyLevel;
            chkSnapToDesktop.Checked = Properties.Settings.Default.MainFormSnap;
            chkAutosaveChanges.Checked = Properties.Settings.Default.AutosaveChanges;
            chkOverridesMonitorPackFrequency.Checked = Properties.Settings.Default.OverridesMonitorPackFrequency;
            if (Properties.Settings.Default.RecentQMConfigFileFilters == null || Properties.Settings.Default.RecentQMConfigFileFilters.Trim() == "")
                txtRecentMonitorPackFilter.Text = "*";
            else
                txtRecentMonitorPackFilter.Text = Properties.Settings.Default.RecentQMConfigFileFilters;
            chkDisplayFullPathForQuickRecentEntries.Checked = Properties.Settings.Default.ShowFullPathForQuickRecentist;
            chkPausePollingDuringEditConfig.Checked = Properties.Settings.Default.PausePollingDuringEditConfig;
            SetFrequency(PollingFrequencySec);

            chkPollingEnabled.Checked = PollingEnabled;

            try
            {
                //ServiceController[] allServices = ServiceController.GetServices();
                //ServiceController qm3srvc = (from s in allServices
                //                             where s.DisplayName == "QuickMon 4 Service"
                //                             select s).FirstOrDefault();
                //if (qm3srvc == null)
                //{
                //    llblLocalServiceRegistered.Visible = true;
                //}
                //else
                //{
                //    llblLocalServiceRegistered.Visible = false;
                //}

                try
                {
                    llblLocalServiceRegistered.Visible = true;
                    Microsoft.Win32.RegistryKey svcsInstalled = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\services\QuickMon 4 Service");
                    if (svcsInstalled.GetValue("DisplayName").ToString() == "QuickMon 4 Service")
                        llblLocalServiceRegistered.Visible = false;
                }
                catch { }

                try
                {
                    llblFirewallRule.Visible = true;
                    Microsoft.Win32.RegistryKey fwrules = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\services\SharedAccess\Parameters\FirewallPolicy\FirewallRules");
                    string quickMonRule = fwrules.GetValue("{F811AB2E-286C-4DB6-8512-4C991A8A54EA}").ToString();
                    if (quickMonRule.Length > 0)
                        llblFirewallRule.Visible = false;
                }
                catch { }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            LoadKnownRemoteHosts();
            loading = false;
        }   
        private void GeneralSettings_Shown(object sender, EventArgs e)
        {
            lvwRemoteHosts.AutoResizeColumnEnabled = true;
        }
        #endregion

        private void cmdOK_Click(object sender, EventArgs e)
        {
            PollingFrequencySec = (int)freqSecNumericUpDown.Value;
            PollingEnabled = chkPollingEnabled.Checked;
            Properties.Settings.Default.ConcurrencyLevel = (int)concurrencyLevelNnumericUpDown.Value;
            Properties.Settings.Default.MainFormSnap = chkSnapToDesktop.Checked;
            Properties.Settings.Default.AutosaveChanges = chkAutosaveChanges.Checked;
            Properties.Settings.Default.OverridesMonitorPackFrequency = chkOverridesMonitorPackFrequency.Checked;
            Properties.Settings.Default.ShowFullPathForQuickRecentist = chkDisplayFullPathForQuickRecentEntries.Checked;
            Properties.Settings.Default.PausePollingDuringEditConfig = chkPausePollingDuringEditConfig.Checked;
            if (txtRecentMonitorPackFilter.Text.Trim().Length == 0)
                Properties.Settings.Default.RecentQMConfigFileFilters = "*";
            else 
                Properties.Settings.Default.RecentQMConfigFileFilters = txtRecentMonitorPackFilter.Text;

            if (remoteListLoaded)
            {
                Properties.Settings.Default.KnownRemoteHosts = new System.Collections.Specialized.StringCollection();
                foreach (ListViewItem lvi in lvwRemoteHosts.Items)
                {
                    if (lvi.Tag is RemoteAgentInfo)
                    {

                        Properties.Settings.Default.KnownRemoteHosts.Add(((RemoteAgentInfo)lvi.Tag).ToString());
                    }
                }
            }
            Properties.Settings.Default.Save();
            DialogResult = System.Windows.Forms.DialogResult.OK;
            Close();
        }

        private void freqSecNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            SetFrequency((int)freqSecNumericUpDown.Value);
        }

        private void SetFrequency(int frequency)
        {
            if (!freChanging)
            {
                freChanging = true;
                if (frequency == 0)
                    frequency = 1;
                if (freqSecNumericUpDown.Maximum >= frequency)
                    freqSecNumericUpDown.Value = frequency;
                else
                    freqSecNumericUpDown.Value = 30;
                freChanging = false;
            }
        }

        private void cmdEditQuickSelectTypeFilters_Click(object sender, EventArgs e)
        {
            Forms.CSVEditor csvEdit = new Forms.CSVEditor();
            csvEdit.CSVData = txtRecentMonitorPackFilter.Text;
            if (csvEdit.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txtRecentMonitorPackFilter.Text = csvEdit.CSVData;
            }
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab == tabPageRemoteHosts)
            {
                if (!remoteListLoaded)
                {
                    RefreshServiceStates();
                    remoteListLoaded = true;
                }
            }
        }

        private void LoadKnownRemoteHosts()
        {
            ListViewItem lvi;
            if (Properties.Settings.Default.KnownRemoteHosts == null)
                Properties.Settings.Default.KnownRemoteHosts = new System.Collections.Specialized.StringCollection();
            else
            {

                foreach (string rh in (from string s in Properties.Settings.Default.KnownRemoteHosts
                                       orderby s
                                       select s))
                {
                    try
                    {
                        RemoteAgentInfo ri = RemoteAgentInfo.FromString(rh);
                        lvi = new ListViewItem(ri.Computer);
                        lvi.SubItems.Add(ri.PortNumber.ToString());
                        string computerNameOnly = rh;

                        lvi.SubItems.Add(""); //Version info to be added afterwards
                        lvi.Tag = ri;
                        lvi.ImageIndex = 0;
                        lvwRemoteHosts.Items.Add(lvi);
                    }
                    catch { }
                }
            }
        }
        private void RefreshServiceStates()
        {
            refreshTimer.Enabled = false;
            refreshTimer.Enabled = true;
        }
        private void UpdateListViewItem(ListViewItem lvi, int imageIndex, string statusText)
        {
            try
            {
                lvwRemoteHosts.Invoke((MethodInvoker)delegate
                {
                    lvi.ImageIndex = imageIndex;
                    lvi.SubItems[2].Text = statusText;
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.ToString());
            }
        }


        #region Refresh statusses
        private void refreshTimer_Tick(object sender, EventArgs e)
        {
            refreshTimer.Enabled = false;
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                List<ListViewItem> items = new List<ListViewItem>();
                items.AddRange((from ListViewItem lvi in lvwRemoteHosts.Items
                                select lvi).ToArray());
                foreach (ListViewItem lvi in lvwRemoteHosts.Items)
                {
                    SetListViewItemIcon(lvi, 0);
                    lvi.SubItems[2].Text = "Loading...";
                    System.Threading.ThreadPool.QueueUserWorkItem(RefreshItem, lvi);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            Cursor.Current = Cursors.Default;
        }
        private void RefreshItem(object o)
        {
            ListViewItem lvi = (ListViewItem)o;
            try
            {
                try
                {
                    bool hostExists = false;
                    RemoteAgentInfo ri = (RemoteAgentInfo)lvi.Tag;
                    string configXml = "<collectorHost uniqueId=\"Ping" + ri.Computer.EscapeXml() + "\" name=\"Ping " + ri.Computer.EscapeXml() + " tests\" enabled=\"True\" expandOnStart=\"True\" dependOnParentId=\"\" " +
                        "agentCheckSequence=\"All\" childCheckBehaviour=\"OnlyRunOnSuccess\" " +
                           "enableRemoteExecute=\"True\" " +
                           "forceRemoteExcuteOnChildCollectors=\"False\" remoteAgentHostAddress=\"" + ri.Computer.EscapeXml() + "\" remoteAgentHostPort=\"48181\" " +
                           "blockParentRemoteAgentHostSettings=\"False\" runLocalOnRemoteHostConnectionFailure=\"True\" >\r\n" +
                           "<collectorAgents>\r\n" +
                                "<collectorAgent type=\"PingCollector\">\r\n" +
                                    "<config>\r\n" +
                                        "<entries>\r\n" +
                                            "<entry pingMethod=\"Ping\" address=\"" + ri.Computer.EscapeXml() + "\" />\r\n" +
                                            "</entries>\r\n" +
                                    "</config>\r\n" +
                                "</collectorAgent>\r\n" +
                            "</collectorAgents>\r\n" +
                        "</collectorHost>\r\n";
                    CollectorHost ce = CollectorHost.FromXml(configXml);
                    
                    ce.EnableRemoteExecute = true;
                    ce.RemoteAgentHostAddress = ri.Computer;
                    ce.RemoteAgentHostPort = ri.PortNumber;
                    try
                    {
                        hostExists = System.Net.Dns.GetHostAddresses(ri.Computer).Count() != 0;
                    }
                    catch { }
                    if (!hostExists)
                    {
                        UpdateListViewItem(lvi, 4, "Host not found");
                    }
                    else if (!CanPingHost(ri.Computer))
                    {
                        UpdateListViewItem(lvi, 4, "Host not pingable");
                    }
                    else
                    {
                        MonitorState testState = RemoteCollectorHostService.GetCollectorHostState(ce);
                        if (testState.State == CollectorState.Good)
                        {
                            try
                            {
                                string versionInfo = RemoteCollectorHostService.GetRemoteAgentHostVersion(ri.Computer, ri.PortNumber);
                                UpdateListViewItem(lvi, 1, versionInfo);
                            }
                            catch (Exception ex)
                            {
                                if (ex.Message.Contains("ContractFilter"))
                                {
                                    UpdateListViewItem(lvi, 2, "Remote host does not support version info query! Check that QuickMon 4.x or later is installed.");
                                }
                                else
                                    UpdateListViewItem(lvi, 2, ex.Message);
                            }
                        }
                        else
                        {
                            UpdateListViewItem(lvi, 3, "N/A");
                        }
                    }
                }
                catch (Exception delegateEx)
                {
                    if (delegateEx.Message.Contains("The formatter threw an exception while trying to deserialize the message"))
                        UpdateListViewItem(lvi, 3, "Old version of Remote agent host does not support query or format does not match! Please update remote agent host version.");
                    else
                        UpdateListViewItem(lvi, 3, delegateEx.Message);
                }
            }
            catch (Exception riEx)
            {
                UpdateListViewItem(lvi, 1, riEx.ToString());
            }
        }
        private bool CanPingHost(string hostName)
        {
            bool isPingable = false;
            try
            {
                using (System.Net.NetworkInformation.Ping ping = new System.Net.NetworkInformation.Ping())
                {
                    System.Net.NetworkInformation.PingReply reply = ping.Send(hostName, 2000);
                    if (reply.Status == System.Net.NetworkInformation.IPStatus.Success)
                        isPingable = true;
                }
            }
            catch { }
            return isPingable;
        }
        private void SetListViewItemIcon(ListViewItem lvi, int imageIndex)
        {
            this.Invoke((MethodInvoker)delegate
            {
                lvi.ImageIndex = imageIndex;
            });
        }
        #endregion

        #region ListView events
        private void lvwRemoteHosts_SelectedIndexChanged(object sender, EventArgs e)
        {
            removeToolStripMenuItem.Enabled = lvwRemoteHosts.SelectedItems.Count > 0;
        }
        private void lvwRemoteHosts_DoubleClick(object sender, EventArgs e)
        {
            if (lvwRemoteHosts.SelectedItems.Count == 1)
            {
                try
                {
                    ListViewItem lvi = lvwRemoteHosts.SelectedItems[0];
                    RemoteAgentInfo ri = (RemoteAgentInfo)lvi.Tag;
                    string remoteHostURL = string.Format("http://{0}:{1}/QuickMonRemoteHost", ri.Computer, ri.PortNumber);
                    System.Diagnostics.Process p = new System.Diagnostics.Process();
                    p.StartInfo = new System.Diagnostics.ProcessStartInfo();
                    p.StartInfo.FileName = remoteHostURL;
                    try
                    {
                        p.Start();
                    }
                    catch (System.ComponentModel.Win32Exception ex)
                    {
                        System.Diagnostics.Trace.WriteLine(ex.ToString());
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        #endregion

        #region Registering local service and firewall rule
        private void llblLocalServiceRegistered_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string localPath = Environment.CurrentDirectory;
            string serviceEXE = System.IO.Path.Combine(localPath, "QuickMonService.exe");
            quickMonServiceOpenFileDialog.FileName = serviceEXE;
            quickMonServiceOpenFileDialog.InitialDirectory = localPath;
            if (quickMonServiceOpenFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                System.Diagnostics.Process p = new System.Diagnostics.Process();
                p.StartInfo = new System.Diagnostics.ProcessStartInfo();
                p.StartInfo.FileName = quickMonServiceOpenFileDialog.FileName;
                p.StartInfo.Arguments = "-install";
                p.StartInfo.Verb = "runas";
                try
                {
                    p.Start();
                }
                catch (System.ComponentModel.Win32Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine(ex.ToString());
                }
            }
        }
        private void llblFirewallRule_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string regfile = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "QuickMon4FirewallRule.reg");
            try
            {
                if (System.IO.File.Exists(regfile))
                    System.IO.File.Delete(regfile);
                System.IO.File.WriteAllText(regfile, Properties.Resources.FireWallRule);
                System.Diagnostics.Process p = new System.Diagnostics.Process();
                p.StartInfo = new System.Diagnostics.ProcessStartInfo();
                p.StartInfo.FileName = "REGEDIT.EXE";
                p.StartInfo.Arguments = "/S " + regfile;
                p.StartInfo.Verb = "runas";
                try
                {
                    p.Start();
                }
                catch (System.ComponentModel.Win32Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine(ex.ToString());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region Other control events
        private void lblComputer_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                if (txtComputer.Text.Length > 0)
                {
                    string versionInfo = RemoteCollectorHostService.GetRemoteAgentHostVersion(txtComputer.Text, (int)remoteportNumericUpDown.Value);
                    MessageBox.Show("Version Info: " + versionInfo, "Version", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Version", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void txtComputer_TextChanged(object sender, EventArgs e)
        {
            cmdAdd.Enabled = txtComputer.Text.Length > 0;
        }
        private void txtComputer_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                cmdAdd_Click(null, null);
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }
        #endregion

        private void cmdAdd_Click(object sender, EventArgs e)
        {
            bool accepted = false;
            if (txtComputer.Text.Length > 0)
            {
                try
                {                    
                    if ((from ListViewItem lvi in lvwRemoteHosts.Items
                         where lvi.Text.ToLower() == txtComputer.Text.ToLower() &&
                         lvi.SubItems[1].Text == remoteportNumericUpDown.Value.ToString()
                         select lvi).Count() > 0)
                    {
                        MessageBox.Show("Remote host is already in the list!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        accepted = false;
                    }
                    else
                    {
                        System.Net.IPAddress[] aa = System.Net.Dns.GetHostAddresses(txtComputer.Text);
                        if (aa.Length == 0)
                        {
                            if (MessageBox.Show("Computer not found or not available!\r\nAccept anyway?", "Error", MessageBoxButtons.YesNo, MessageBoxIcon.Error) == System.Windows.Forms.DialogResult.Yes)
                                accepted = true;
                        }
                        else
                        {
                            accepted = true;                            
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("The requested name is valid, but no data of the requested type was found"))
                    {
                        if (MessageBox.Show("Computer inaccessible or name invalid!\r\nAccept anyway?", "Error", MessageBoxButtons.YesNo, MessageBoxIcon.Error) == System.Windows.Forms.DialogResult.Yes)
                        {
                            accepted = true;
                        }                    }
                    else
                        MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                if (accepted)
                {
                    RemoteAgentInfo ri = new RemoteAgentInfo();
                    ri.Computer = txtComputer.Text;
                    ri.PortNumber = (int)remoteportNumericUpDown.Value;
                    ListViewItem lvi = new ListViewItem(txtComputer.Text);
                    lvi.SubItems.Add(remoteportNumericUpDown.Value.ToString());
                    lvi.SubItems.Add(""); //Version info to be added afterwards
                    lvi.Tag = ri;
                    lvi.ImageIndex = 3;
                    lvwRemoteHosts.Items.Add(lvi);
                    RefreshServiceStates();
                    txtComputer.Text = "";
                    txtComputer.Focus();
                }
            }
        }

        #region Context menu events
        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RefreshServiceStates();
        }
        private void removeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to delete the selected entry(s)", "Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
            {
                foreach (int index in (from int i in lvwRemoteHosts.SelectedIndices
                                       orderby i descending
                                       select i))
                {
                    lvwRemoteHosts.Items.RemoveAt(index);
                }
            }
        }        
        #endregion

    }
}