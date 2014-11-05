using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using RedditSaveTransfer.Properties;

namespace RedditSaveTransfer
{
    public partial class SelectPropertiesWindow : Form
    {
        public static readonly string[] DefaultPropertiesToExport = {"subreddit", "url", "title", "created_utc"};
        public static List<string> PropertiesToExport = Settings.Default.PropertiesToExport.Split(',').ToList();

        public SelectPropertiesWindow()
        {
            InitializeComponent();

            AcceptButton = btnAccept;
            CancelButton = btnCancel;

            SetCheckedItems();
        }

        private void SetPropertiesToExport()
        {
            PropertiesToExport.Clear();

            if (chkListBoxProps.CheckedItems.Count > 0)
            {
                foreach (var p in chkListBoxProps.CheckedItems)
                    PropertiesToExport.Add(p.ToString());
            }
            else
            {
                foreach (var p in DefaultPropertiesToExport)
                    PropertiesToExport.Add(p);
            }
        }

        private void SetCheckedItems()
        {
            foreach (var p in PropertiesToExport)
            {
                chkListBoxProps.SetItemChecked(chkListBoxProps.Items.IndexOf(p), true);

                Console.WriteLine(p);
            }
        }

        private void btnAccept_Click(object sender, EventArgs e)
        {
            SetPropertiesToExport();

            if (PropertiesToExport != null && PropertiesToExport.Any())
            {
                Settings.Default.PropertiesToExport = String.Join(",", PropertiesToExport.ToArray());
                Settings.Default.Save();
            }

            DialogResult = DialogResult.OK;
        }

        private void btnDefault_Click(object sender, EventArgs e)
        {
            CheckUncheckAll(false);

            foreach (var p in DefaultPropertiesToExport)
                chkListBoxProps.SetItemChecked(chkListBoxProps.Items.IndexOf(p), true);
        }

        private void btnSelectAll_Click(object sender, EventArgs e)
        {
            CheckUncheckAll(true);
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            CheckUncheckAll(false);
        }

        private void CheckUncheckAll(bool itemChecked)
        {
            for (var i = 0; i < chkListBoxProps.Items.Count; i++)
                chkListBoxProps.SetItemChecked(i, itemChecked);
        }
    }
}