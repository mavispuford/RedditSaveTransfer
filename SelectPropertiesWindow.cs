using System;
using System.Linq;
using System.Windows.Forms;
using RedditSaveTransfer.Properties;

namespace RedditSaveTransfer
{
    public partial class SelectPropertiesWindow : Form
    {

        public SelectPropertiesWindow()
        {
            InitializeComponent();

            AcceptButton = btnAccept;
            CancelButton = btnCancel;

            SetCheckedItems();
        }

        private void SetPropertiesToExport()
        {
            Common.PropertiesToExport.Clear();

            foreach (var p in chkListBoxProps.CheckedItems)
                Common.PropertiesToExport.Add(p.ToString());
        }

        private void SetCheckedItems()
        {
            foreach (var p in Common.PropertiesToExport)
            {
                var index = chkListBoxProps.Items.IndexOf(p);

                if (index <= 0 || index >= chkListBoxProps.Items.Count)
                    continue;

                chkListBoxProps.SetItemChecked(index, true);

                Console.WriteLine(p);
            }
        }

        private void btnAccept_Click(object sender, EventArgs e)
        {
            if (chkListBoxProps.CheckedItems.Count == 0)
            {
                MessageBox.Show("You must have at least one property selected.");
                return;
            }

            SetPropertiesToExport();

            if (Common.PropertiesToExport != null && Common.PropertiesToExport.Any())
            {
                Settings.Default.PropertiesToExport = String.Join(",", Common.PropertiesToExport.ToArray());
                Settings.Default.Save();
            }

            DialogResult = DialogResult.OK;
        }

        private void btnDefault_Click(object sender, EventArgs e)
        {
            CheckUncheckAll(false);

            foreach (var p in Common.DefaultPropertiesToExport)
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