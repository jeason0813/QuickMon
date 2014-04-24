﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuickMon
{
    public abstract class AgentBase : IAgent
    {
        #region IAgent Members
        public string Name { get;  set; }
        public IAgentConfig AgentConfig { get; set; }

        
        public virtual bool ShowEditEntry(ref ICollectorConfigEntry entry)
        {
            bool accepted = false;
            IEditConfigEntryWindow editConfig = GetEditConfigEntryWindow();
            editConfig.SelectedEntry = entry;
            //editConfig.SetTitle(title); //"Edit " + Name + " config");
            if (editConfig != null && editConfig.ShowEditEntry() == QuickMonDialogResult.Ok)
            {
                //entry = (ICollectorConfigEntry)editConfig.SelectedEntry;
                entry = editConfig.SelectedEntry;
                accepted = true;
            }
            return accepted;
        }
        public abstract IEditConfigEntryWindow GetEditConfigEntryWindow();
        
        public abstract string GetDefaultOrEmptyConfigString();
        public virtual void SetConfigurationFromXmlString(string configurationString)
        {
            AgentConfig.ReadConfiguration(configurationString);
        }
        public virtual void Close() { }
        #endregion
    }
}