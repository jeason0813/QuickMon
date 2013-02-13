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
    public partial class EditSoapWebServicePingConfigEntry : Form
    {
        public EditSoapWebServicePingConfigEntry()
        {
            InitializeComponent();
        }

        public SoapWebServicePingConfigEntry SelectedSoapWebServicePingConfigEntry { get; set; }

        #region Form events
        private void EditSoapWebServicePingConfigEntry_Shown(object sender, EventArgs e)
        {
            if (SelectedSoapWebServicePingConfigEntry != null)
            {
                txtServiceURL.Text = SelectedSoapWebServicePingConfigEntry.ServiceBaseURL;
                txtServiceName.Text = SelectedSoapWebServicePingConfigEntry.ServiceName;
                txtMethodName.Text = SelectedSoapWebServicePingConfigEntry.MethodName;
                txtParameters.Text = SelectedSoapWebServicePingConfigEntry.ToStringFromParameters();
                cboCheckType.SelectedIndex = SelectedSoapWebServicePingConfigEntry.CheckType == SoapWebServicePingCheckType.Success ? 0 : 1;
                cboResultType.SelectedIndex = (int)SelectedSoapWebServicePingConfigEntry.ResultType;
                cboErrorCustomValue1.Text = SelectedSoapWebServicePingConfigEntry.CustomValue1;
                txtErrorCustomValue2.Text = SelectedSoapWebServicePingConfigEntry.CustomValue2;
            }
            else
            {
                txtServiceURL.Text = "http://";
                cboCheckType.SelectedIndex = 0;
                cboResultType.SelectedIndex = 0;
            }
        }
        #endregion

        #region Button events
        private void cmdTestAddress_Click(object sender, EventArgs e)
        {
            SoapWebServicePingConfigEntry tmpSoapWebServicePingConfigEntry = new SoapWebServicePingConfigEntry();
            if (!(txtServiceURL.Text.StartsWith("http://", StringComparison.CurrentCultureIgnoreCase) || txtServiceURL.Text.StartsWith("https://", StringComparison.CurrentCultureIgnoreCase)))
                txtServiceURL.Text = "http://" + txtServiceURL.Text;
            tmpSoapWebServicePingConfigEntry.ServiceBaseURL = txtServiceURL.Text;
            tmpSoapWebServicePingConfigEntry.ServiceName = txtServiceName.Text;
            tmpSoapWebServicePingConfigEntry.MethodName = txtMethodName.Text;
            tmpSoapWebServicePingConfigEntry.Parameters = new List<string>();
            foreach (string par in txtParameters.Text.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (par.Trim().Length > 0)
                    tmpSoapWebServicePingConfigEntry.Parameters.Add(par);
            }
            tmpSoapWebServicePingConfigEntry.CheckType = (SoapWebServicePingCheckType)cboCheckType.SelectedIndex;
            tmpSoapWebServicePingConfigEntry.ResultType = (SoapWebServicePingResultEnum)cboResultType.SelectedIndex;
            tmpSoapWebServicePingConfigEntry.CustomValue1 = cboErrorCustomValue1.Text;
            tmpSoapWebServicePingConfigEntry.CustomValue2 = txtErrorCustomValue2.Text;
            try
            {
                object pingResult = tmpSoapWebServicePingConfigEntry.ExecuteMethod();
                if (tmpSoapWebServicePingConfigEntry.ResultType == SoapWebServicePingResultEnum.CheckAvailabilityOnly)
                {
                    MessageBox.Show("Web service is available!", "Test", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else if (pingResult != null && pingResult.ToString() != "")
                {
                    string formattedVal = "";
                    bool result = tmpSoapWebServicePingConfigEntry.CheckResultMatch(pingResult, tmpSoapWebServicePingConfigEntry.ResultType,
                        tmpSoapWebServicePingConfigEntry.CustomValue1, tmpSoapWebServicePingConfigEntry.CustomValue2, out formattedVal);
                    MessageBox.Show(string.Format("Method was executed successfully\r\nResult: {0}\r\nValue: {1}",
                            result ? "Success" : "Fail", formattedVal), "Test", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Method was executed successfully but return nothing!", "Test", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void cmdOK_Click(object sender, EventArgs e)
        {
            if (CheckOKButtonEnable())
            {
                SelectedSoapWebServicePingConfigEntry = new SoapWebServicePingConfigEntry();
                SelectedSoapWebServicePingConfigEntry.ServiceBaseURL = txtServiceURL.Text;
                SelectedSoapWebServicePingConfigEntry.ServiceName = txtServiceName.Text;
                SelectedSoapWebServicePingConfigEntry.MethodName = txtMethodName.Text;
                SelectedSoapWebServicePingConfigEntry.Parameters = new List<string>();
                foreach (string par in txtParameters.Text.Split(new string[] {","}, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (par.Trim().Length > 0)
                        SelectedSoapWebServicePingConfigEntry.Parameters.Add(par);
                }
                SelectedSoapWebServicePingConfigEntry.CheckType = (SoapWebServicePingCheckType)cboCheckType.SelectedIndex;
                SelectedSoapWebServicePingConfigEntry.ResultType = (SoapWebServicePingResultEnum)cboResultType.SelectedIndex;
                SelectedSoapWebServicePingConfigEntry.CustomValue1 = cboErrorCustomValue1.Text;
                SelectedSoapWebServicePingConfigEntry.CustomValue2 = txtErrorCustomValue2.Text;
                DialogResult = System.Windows.Forms.DialogResult.OK;
                Close();
            }
        }
        #endregion

        #region Input control events
        private void txtAddress_TextChanged(object sender, EventArgs e)
        {
            CheckOKButtonEnable();
        }
        #endregion

        #region Private methods
        private bool CheckOKButtonEnable()
        {
            cmdOK.Enabled = txtServiceURL.Text.Length > 0 
                && (!txtServiceURL.Text.Contains("\\"))
                && txtServiceName.Text.Trim().Length > 0
                && txtMethodName.Text.Trim().Length > 0
                && cboCheckType.SelectedIndex > -1
                && cboResultType.SelectedIndex > -1
                && (cboErrorCustomValue1.Text.Length > 0 || cboResultType.SelectedIndex < 2);
            cmdTestAddress.Enabled = txtServiceURL.Text.Length > 0
                && txtServiceURL.Text.ToLower() != "http://" 
                && (!txtServiceURL.Text.Contains("\\"))
                && txtServiceName.Text.Length > 0
                && txtMethodName.Text.Length > 0
                && cboCheckType.SelectedIndex > -1
                && cboResultType.SelectedIndex > -1
                && (cboErrorCustomValue1.Text.Length > 0 || cboResultType.SelectedIndex < 2);
            return cmdOK.Enabled;
        }
        #endregion    

        private void cboCheckType_SelectedIndexChanged(object sender, EventArgs e)
        {
            CheckOKButtonEnable();
        }

        private void cboResultType_SelectedIndexChanged(object sender, EventArgs e)
        {
            CheckOKButtonEnable();
        }

    }
}
