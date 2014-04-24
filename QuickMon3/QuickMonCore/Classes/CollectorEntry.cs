﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;

namespace QuickMon
{
    public partial class CollectorEntry
    {
        public CollectorEntry()
        {
            ServiceWindows = new ServiceWindows();
            ServiceWindows.CreateFromConfig("<serviceWindows/>");
            Name = "";
            Enabled = true;
            UniqueId = Guid.NewGuid().ToString();
            RemoteAgentHostPort = 8181;
            //LastStateUpdate = new DateTime(2000, 1, 1);
            //FirstStateUpdate = new DateTime(2000, 1, 1);
            PollCount = 0;
            GoodStateCount = 0;
            WarningStateCount = 0;
            ErrorStateCount = 0;
            LastGoodState = null;
            LastWarningState = null;
            LastErrorState = null;
        }

        #region Private vars
        private bool waitAlertTimeErrWarnInMinFlagged = false;
        private DateTime delayErrWarnAlertTime = new DateTime(2000, 1, 1);
        private int numberOfPollingsInErrWarn = 0;
        #endregion

        #region Properties
        #region General properties
        public string Name { get; set; }
        /// <summary>
        /// Any unique identifier for this collector entry. Used in collector parent-child relatioships
        /// </summary>
        public string UniqueId { get; set; }
        /// <summary>
        /// The parent collector entry's unique identifier
        /// </summary>
        public string ParentCollectorId { get; set; }
        /// <summary>
        /// Any object you wish to link with this instance
        /// </summary>
        public object Tag { get; set; }
        #endregion

        #region Is this collector entry enabled now
        /// <summary>
        /// User/config based setting to disable this CollectorEntry
        /// </summary>
        public bool Enabled { get; set; }
        /// <summary>
        /// List if service windows when collector can operate
        /// </summary>
        public ServiceWindows ServiceWindows { get; set; }
        #endregion

        #region Collector agent related
        /// <summary>
        /// Is this collector entry just a folder/container
        /// </summary>
        public bool IsFolder { get; set; }
        public bool CollectOnParentWarning { get; set; } 
        public string CollectorRegistrationName { get; set; }
        public string CollectorRegistrationDisplayName { get; set; }
        public ICollector Collector { get; set; }
        private string initialConfiguration = "";
        public string InitialConfiguration
        {
            get { return initialConfiguration; }
            set
            {
                LastMonitorState = new MonitorState() { State = CollectorState.NotAvailable };
                CurrentState = new MonitorState() { State = CollectorState.NotAvailable };
                initialConfiguration = value;
            }
        }
        #endregion

        #region Corrective Script related
        public bool CorrectiveScriptDisabled { get; set; }
        public string CorrectiveScriptOnWarningPath { get; set; }
        public string CorrectiveScriptOnErrorPath { get; set; }
        public string RestorationScriptPath { get; set; }
        public bool CorrectiveScriptsOnlyOnStateChange { get; set; } 
        #endregion

        #region Last State update details
        /// <summary>
        /// 'Current' monitor state
        /// </summary>
        public MonitorState CurrentState { get; set; }
        /// <summary>
        /// Records the last monitor state
        /// </summary>
        public MonitorState LastMonitorState { get; set; }
        /// <summary>
        /// Records when the last state change was
        /// </summary>
        public DateTime LastStateChange { get; set; }

        #endregion

        #region Stats
        /// <summary>
        /// Record the last time an alert was raised. Used in conjunction with AlertOnceInXMin
        /// </summary>
        public DateTime LastAlertTime { get; set; }
        /// <summary>
        /// Records when the last good state was recorded
        /// </summary>
        public DateTime LastGoodStateTime { get; set; }
        public DateTime LastWarningStateTime { get; set; }
        public DateTime LastErrorStateTime { get; set; }
        public DateTime LastStateCheckAttemptBegin { get; set; }
        public DateTime LastStateUpdate { get; set; }
        public long LastStateCheckDurationMS { get; set; }
        public int PollCount { get; set; }
        public DateTime FirstStateUpdate { get; set; }
        public int GoodStateCount { get; set; }
        public int WarningStateCount { get; set; }
        public int ErrorStateCount { get; set; }
        public DateTime LastWarningAlertTime { get; set; }
        public DateTime LastErrorAlertTime { get; set; }

        public MonitorState LastGoodState { get; set; }
        public MonitorState LastWarningState { get; set; }
        public MonitorState LastErrorState { get; set; }
        #endregion

        #region Alerting
        /// <summary>
        /// Repeat raising alert after X minutes if state remains in error/warning
        /// </summary>
        public int RepeatAlertInXMin { get; set; }
        public int RepeatAlertInXPolls { get; set; }
        /// <summary>
        /// Only raise an alert once in X minutes. Used in conjunction with LastAlertTime
        /// </summary>
        public int AlertOnceInXMin { get; set; }
        public int AlertOnceInXPolls { get; set; }
        /// <summary>
        /// Only raise an alert if the LastMonitorState remains Error or Warning.
        /// After each alert is generated this time gets updated
        /// </summary>
        public int DelayErrWarnAlertForXSec { get; set; }
        public int DelayErrWarnAlertForXPolls { get; set; }
        #endregion

        #region Remote Execution
        public bool EnableRemoteExecute { get; set; }
        public string RemoteAgentHostAddress { get; set; }
        public int RemoteAgentHostPort { get; set; }
        public bool ForceRemoteExcuteOnChildCollectors { get; set; }
        public bool OverrideRemoteAgentHost { get; set; }
        public string OverrideRemoteAgentHostAddress  { get; set; }
        public int OverrideRemoteAgentHostPort { get; set; }
        #endregion
        #endregion

        #region Refreshing state and getting alerts
        /// <summary>
        /// Queries the agent to get the latest state.
        /// </summary>
        /// <returns>True or False but not both I hope</returns>
        public MonitorState GetCurrentState()
        {
            if (LastMonitorState == null)
                LastMonitorState = new MonitorState() { State = CollectorState.NotAvailable };
            if (CurrentState == null)
                CurrentState = new MonitorState() { State = CollectorState.NotAvailable };
            if (LastMonitorState.State != CollectorState.ConfigurationError)            
            {                
                if (!Enabled)
                {
                    CurrentState.State = CollectorState.Disabled;
                }
                else if (IsFolder)
                {
                    LastMonitorState = CurrentState.Clone();
                    if (ServiceWindows.IsInTimeWindow())
                        CurrentState.State = CollectorState.Folder;
                    else
                        CurrentState.State = CollectorState.Disabled;
                }
                else
                {
                    if (ServiceWindows.IsInTimeWindow())
                    {
                        //*********** Call actual collector GetState **********
                        LastStateCheckAttemptBegin = DateTime.Now;
                        LastMonitorState = CurrentState.Clone();
                        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                        sw.Start();
                        if (OverrideRemoteAgentHost)
                        {
                            try
                            {
                                CurrentState = CollectorEntryRelay.GetRemoteAgentState(this, OverrideRemoteAgentHostAddress, OverrideRemoteAgentHostPort);
                            }
                            catch (Exception ex)
                            {
                                CurrentState.State = CollectorState.Error;
                                CurrentState.RawDetails = ex.ToString();
                            }
                        }
                        else if (EnableRemoteExecute)
                        {
                            try
                            {
                                CurrentState = CollectorEntryRelay.GetRemoteAgentState(this);
                            }
                            catch(Exception ex)
                            {
                                CurrentState.State = CollectorState.Error;
                                CurrentState.RawDetails = ex.ToString();
                            }
                        }
                        else
                            CurrentState = Collector.GetState();
                        sw.Stop();

                        //Updating stats
                        LastStateCheckDurationMS = sw.ElapsedMilliseconds;
                        LastStateUpdate = DateTime.Now;
                        if (FirstStateUpdate <  (new DateTime(2000, 1, 1)))
                            FirstStateUpdate = DateTime.Now;
                        PollCount++;
                        if (CurrentState.State == CollectorState.Good)
                        {
                            LastGoodState = CurrentState.Clone();
                            LastGoodStateTime = DateTime.Now;
                            GoodStateCount++;
                        }
                        else if (CurrentState.State == CollectorState.Warning)
                        {
                            LastWarningState = CurrentState.Clone();
                            LastWarningStateTime = DateTime.Now;
                            WarningStateCount++;
                        }
                        else if (CurrentState.State == CollectorState.Error)
                        {
                            LastErrorState = CurrentState.Clone();
                            LastErrorStateTime = DateTime.Now;
                            ErrorStateCount++;
                        }
                    }
                    else
                        CurrentState.State = CollectorState.Disabled;
                }
            }
            else
            {
                CurrentState.State = CollectorState.ConfigurationError;
            }
            return CurrentState;
        }
        /// <summary>
        /// Check if the Collector state changed from previous poll
        /// </summary>
        /// <returns>True or False, you decide</returns>
        public bool StateChanged()
        {
            bool stateChanged = false;
            if (IsFolder || !Enabled) //don't bother raising events for folders
                stateChanged = false;
            else
            {
                stateChanged = (LastMonitorState.State != CurrentState.State);
                if (stateChanged)
                    LastStateChange = DateTime.Now; //reset time
            }
            return stateChanged;
        }
        #endregion

        #region Get/Set configuration
        /// <summary>
        /// Create a new instance of the collector agent
        /// </summary>
        /// <param name="ra">Agent registration type</param>
        /// <returns>Instance of ICollector</returns>
        public static ICollector CreateCollectorEntry(RegisteredAgent ra)
        {
            if (ra != null)
            {
                return CreateCollectorEntry(ra.AssemblyPath, ra.ClassName);
            }
            else
                return null;
        }
        /// <summary>
        /// Create a new instance of the collector agent
        /// </summary>
        /// <param name="assemblyPath">Path to Collector agent type</param>
        /// <param name="className">Class name of Collector agent type</param>
        /// <returns>Instance of ICollector</returns>
        private static ICollector CreateCollectorEntry(string assemblyPath, string className)
        {
            Assembly collectorEntryAssembly = Assembly.LoadFile(assemblyPath);
            ICollector newCollectorEntry = (ICollector)collectorEntryAssembly.CreateInstance(className);
            newCollectorEntry.Name = className.Replace("QuickMon.Collectors.","");
            return newCollectorEntry;
        }
        /// <summary>
        /// Create CollectorEntry instance based on configuration string
        /// </summary>
        /// <param name="stringCollectorEntry">configuration string</param>
        /// <returns>CollectorEntry instance</returns>
        public static CollectorEntry FromConfig(string stringCollectorEntry)
        {
            XmlDocument xmlCollectorEntry = new XmlDocument();
            xmlCollectorEntry.LoadXml(stringCollectorEntry);
            return FromConfig(xmlCollectorEntry.DocumentElement);
        }
        /// <summary>
        /// Create CollectorEntry instance based on configuration string
        /// </summary>
        /// <param name="xmlCollectorEntry">configuration XmlElement instance</param>
        /// <returns>CollectorEntry instance</returns>
        public static CollectorEntry FromConfig(XmlElement xmlCollectorEntry)
        {
            CollectorEntry collectorEntry = new CollectorEntry();
            collectorEntry.LastStateChange = DateTime.Now;
            collectorEntry.Name = xmlCollectorEntry.ReadXmlElementAttr("name", "").Trim();
            collectorEntry.UniqueId = xmlCollectorEntry.ReadXmlElementAttr("uniqueID", DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"));
            collectorEntry.Enabled = bool.Parse(xmlCollectorEntry.ReadXmlElementAttr("enabled", "True"));
            collectorEntry.IsFolder = bool.Parse(xmlCollectorEntry.ReadXmlElementAttr("isFolder", "False"));
            collectorEntry.ParentCollectorId = xmlCollectorEntry.ReadXmlElementAttr("dependOnParent");
            collectorEntry.CorrectiveScriptDisabled = bool.Parse(xmlCollectorEntry.ReadXmlElementAttr("correctiveScriptDisabled", "False"));
            collectorEntry.CorrectiveScriptOnWarningPath = xmlCollectorEntry.ReadXmlElementAttr("correctiveScriptOnWarningPath");
            collectorEntry.CorrectiveScriptOnErrorPath = xmlCollectorEntry.ReadXmlElementAttr("correctiveScriptOnErrorPath");
            collectorEntry.RestorationScriptPath = xmlCollectorEntry.ReadXmlElementAttr("restorationScriptPath");
            collectorEntry.CorrectiveScriptsOnlyOnStateChange = bool.Parse(xmlCollectorEntry.ReadXmlElementAttr("correctiveScriptsOnlyOnStateChange", "False"));
            collectorEntry.EnableRemoteExecute = bool.Parse(xmlCollectorEntry.ReadXmlElementAttr("enableRemoteExecute", "False"));
            collectorEntry.ForceRemoteExcuteOnChildCollectors = bool.Parse(xmlCollectorEntry.ReadXmlElementAttr("forceRemoteExcuteOnChildCollectors", "False"));
            collectorEntry.RemoteAgentHostAddress = xmlCollectorEntry.ReadXmlElementAttr("remoteAgentHostAddress");
            collectorEntry.RemoteAgentHostPort = xmlCollectorEntry.ReadXmlElementAttr("remoteAgentHostPort", 8181);

            //Service windows config
            collectorEntry.ServiceWindows = new ServiceWindows();
            XmlNode serviceWindowsNode = xmlCollectorEntry.SelectSingleNode("serviceWindows");
            if (serviceWindowsNode != null) //Load service windows info
                collectorEntry.ServiceWindows.CreateFromConfig(serviceWindowsNode.OuterXml);
            else
            {
                collectorEntry.ServiceWindows.CreateFromConfig("<serviceWindows />");
            }

            if (!collectorEntry.IsFolder)
            {
                collectorEntry.CollectorRegistrationName = xmlCollectorEntry.ReadXmlElementAttr("collector", "No collector");
                collectorEntry.CollectOnParentWarning = bool.Parse(xmlCollectorEntry.ReadXmlElementAttr("collectOnParentWarning", "False"));
                collectorEntry.RepeatAlertInXMin = int.Parse(xmlCollectorEntry.ReadXmlElementAttr("repeatAlertInXMin", "0"));
                collectorEntry.AlertOnceInXMin = int.Parse(xmlCollectorEntry.ReadXmlElementAttr("alertOnceInXMin", "0"));
                collectorEntry.DelayErrWarnAlertForXSec = int.Parse(xmlCollectorEntry.ReadXmlElementAttr("delayErrWarnAlertForXSec", "0"));

                collectorEntry.RepeatAlertInXPolls = int.Parse(xmlCollectorEntry.ReadXmlElementAttr("repeatAlertInXPolls", "0"));
                collectorEntry.AlertOnceInXPolls = int.Parse(xmlCollectorEntry.ReadXmlElementAttr("alertOnceInXPolls", "0"));
                collectorEntry.DelayErrWarnAlertForXPolls = int.Parse(xmlCollectorEntry.ReadXmlElementAttr("delayErrWarnAlertForXPolls", "0"));

                collectorEntry.LastAlertTime = new DateTime(2000, 1, 1); //long ago
                collectorEntry.LastGoodStateTime = new DateTime(2000, 1, 1); //long ago
                collectorEntry.LastMonitorState = new MonitorState() { State = CollectorState.NotAvailable, CurrentValue = null, RawDetails = "", HtmlDetails = "" };
                XmlNode configNode = xmlCollectorEntry.SelectSingleNode("config");
                if (configNode != null)
                {
                    collectorEntry.InitialConfiguration = configNode.OuterXml;
                }
                else
                {
                    collectorEntry.LastMonitorState.State = CollectorState.ConfigurationError;
                    collectorEntry.InitialConfiguration = "";
                }
            }
            else
            {
                collectorEntry.CollectorRegistrationName = "Folder";
                collectorEntry.CollectorRegistrationDisplayName = "Folder";
                collectorEntry.InitialConfiguration = "";
                collectorEntry.LastMonitorState.State = CollectorState.Folder;
            }
            return collectorEntry;
        }
        /// <summary>
        /// Export current CollectorEntry config as XML string
        /// </summary>
        /// <returns>XML config string</returns>
        public string ToConfig()
        {
            string collectorConfig = Collector == null ? InitialConfiguration : Collector.AgentConfig.ToConfig();
            string config = ToConfig(UniqueId,
                Name.EscapeXml(),
                Enabled,
                IsFolder,
                CollectorRegistrationName,
                ParentCollectorId,
                CollectOnParentWarning,
                RepeatAlertInXMin, AlertOnceInXMin, DelayErrWarnAlertForXSec,
                RepeatAlertInXPolls, AlertOnceInXPolls,  DelayErrWarnAlertForXPolls,
                CorrectiveScriptDisabled,
                CorrectiveScriptOnWarningPath,
                CorrectiveScriptOnErrorPath,
                RestorationScriptPath,
                CorrectiveScriptsOnlyOnStateChange,
                EnableRemoteExecute,
                ForceRemoteExcuteOnChildCollectors,
                RemoteAgentHostAddress,
                RemoteAgentHostPort,
                collectorConfig,
                ServiceWindows.ToConfig());
            return config;
        }
        /// <summary>
        /// Export current CollectorEntry config as XML string
        /// </summary>
        /// <param name="uniqueId">Unique id of Collector entry</param>
        /// <param name="name">Display name of Collector entry</param>
        /// <param name="enabled">Is Collector entry enables (True/False)</param>
        /// <param name="isFolder">Is this Collector entry a folder (True/False)</param>
        /// <param name="collectorRegistrationName">Agent registration name</param>
        /// <param name="parentCollectorId">Parent  Collector entry id</param>
        /// <param name="collectOnParentWarning">Show 'child' collector entries be checked if this collector entry returns a warning state</param>
        /// <param name="repeatAlertInXMin">Repeat warning/error alert if state remains warning/error</param>
        /// <param name="alertOnceInXMin">Only alert once in specified minutes regardless of state changes</param>
        /// <param name="delayErrWarnAlertForXSec">Only raise alert if state remains warning/error for specified number of seconds</param>
        /// <param name="correctiveScriptDisabled">Is corrective scripts disabled? (True/False)</param>
        /// <param name="correctiveScriptOnWarningPath">Path to script if warning state is reached</param>
        /// <param name="correctiveScriptOnErrorPath">Path to script if error state is reached</param>
        /// <param name="restorationScriptPath">Path to script if good state is reached after state was warning/error before</param>
        /// <param name="correctiveScriptsOnlyOnStateChange"></param>
        /// <param name="enableRemoteExecute">Is remote agent checking enabled (True/False)</param>
        /// <param name="remoteAgentHostAddress">Name of the remote agent host</param>
        /// <param name="remoteAgentHostPort">Port number of remote agent host (default is 8181)</param>
        /// <param name="collectorConfig">Full xml config string of collector agent</param>
        /// <param name="serviceWindows">Full xml config of service windows</param>
        /// <returns>XML config string</returns>
        public static string ToConfig(string uniqueId,
                string name,
                bool enabled,
                bool isFolder,
                string collectorRegistrationName,
                string parentCollectorId,
                bool collectOnParentWarning,
                int repeatAlertInXMin, int alertOnceInXMin, int delayErrWarnAlertForXSec,
                int repeatAlertInXPolls, int alertOnceInXPolls,  int delayErrWarnAlertForXPolls,
                bool correctiveScriptDisabled,
                string correctiveScriptOnWarningPath,
                string correctiveScriptOnErrorPath,
                string restorationScriptPath,
                bool correctiveScriptsOnlyOnStateChange,
                bool enableRemoteExecute,
                bool forceRemoteExcuteOnChildCollectors,
                string remoteAgentHostAddress,
                int remoteAgentHostPort,
                string collectorConfig,
                string serviceWindows)
        {
            string config = string.Format(Properties.Resources.CollectorEntryXml,
                uniqueId,
                name,
                enabled,
                isFolder,
                collectorRegistrationName,
                parentCollectorId,
                collectOnParentWarning,
                repeatAlertInXMin, alertOnceInXMin, delayErrWarnAlertForXSec,
                repeatAlertInXPolls, alertOnceInXPolls,  delayErrWarnAlertForXPolls,
                correctiveScriptDisabled,
                correctiveScriptOnWarningPath,
                correctiveScriptOnErrorPath,
                restorationScriptPath,
                correctiveScriptsOnlyOnStateChange,
                enableRemoteExecute,
                forceRemoteExcuteOnChildCollectors,
                remoteAgentHostAddress,
                remoteAgentHostPort,
                collectorConfig,
                serviceWindows);
            return config;
        }
        #endregion

        #region Viewing details
        private ICollectorDetailView collectorDetailView = null;
        public void ShowDetails()
        {
            if (Collector != null)
            {
                if (collectorDetailView == null || (!collectorDetailView.IsStillVisible()))
                {
                    collectorDetailView = Collector.GetCollectorDetailView();
                    if (collectorDetailView != null)
                    {
                        collectorDetailView.SetWindowTitle(Name + " (" + this.CollectorRegistrationDisplayName + ")");
                        collectorDetailView.ShowCollectorDetails(Collector);
                    }
                }
                else
                {
                    collectorDetailView.RefreshConfig(Collector);
                }
            }
        }
        public bool ShowEditConfigEntry(ref ICollectorConfigEntry entry)
        {
            bool changed = false;
            if (Collector != null)
            {
                changed = Collector.ShowEditEntry(ref entry);
            }
            return changed;
        }
        public void RefreshDetailsIfOpen()
        {
            if (Collector != null)
            {
                if (collectorDetailView != null && (collectorDetailView.IsStillVisible()))
                {
                    collectorDetailView.SetWindowTitle(Name + " (" + this.CollectorRegistrationName + ")");
                    collectorDetailView.RefreshConfig(Collector);
                }
            }
        }
        public void CloseDetails()
        {
            if (Collector != null)
            {
                if (collectorDetailView != null && (collectorDetailView.IsStillVisible()))
                {
                    try
                    {
                        collectorDetailView.CloseWindow();
                    }
                    catch { }
                }
            }
        }
        #endregion

        public override string ToString()
        {
            return string.Format("{0} ({1})", Name, CollectorRegistrationName);
        }
        /// <summary>
        /// Creates a new copy of the current Collector Entry
        /// </summary>
        /// <returns>CollectorEntry instance</returns>
        public CollectorEntry Clone(bool newId = false)
        {
            CollectorEntry clone = FromConfig(ToConfig());
            if (newId)
                clone.UniqueId = Guid.NewGuid().ToString();
            clone.CollectorRegistrationDisplayName = this.CollectorRegistrationDisplayName; //have to add it afterwards since config does not specify it
            return clone;
        }
    }
}