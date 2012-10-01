using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace RedditSaveTransfer
{
    public partial class SelectPropertiesWindow : Form
    {
        public static List<string> PropertiesToExport = new List<string>();

        //string[] defaultChecked = { "id", "subreddit", "author", "domain", "over_18", "url", "title", "score", "num_comments", "created_utc" };
        string[] defaultChecked = { "subreddit", "url", "title", "created_utc" };

        public SelectPropertiesWindow()
        {
            InitializeComponent();

            if (PropertiesToExport.Count > 0)
                foreach (string p in PropertiesToExport)
                    chkListBoxProps.SetItemChecked(chkListBoxProps.Items.IndexOf(p), true);
            else
                foreach (string p in defaultChecked)
                    chkListBoxProps.SetItemChecked(chkListBoxProps.Items.IndexOf(p), true);

            this.AcceptButton = btnAccept;
            this.CancelButton = btnCancel;
        }

        private void btnAccept_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void btnDefault_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < chkListBoxProps.Items.Count; i++)
                chkListBoxProps.SetItemChecked(i, false);

            foreach (string p in defaultChecked)
                chkListBoxProps.SetItemChecked(chkListBoxProps.Items.IndexOf(p), true);
        }

        private void btnSelectAll_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < chkListBoxProps.Items.Count; i++)
                chkListBoxProps.SetItemChecked(i, true);
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < chkListBoxProps.Items.Count; i++)
                chkListBoxProps.SetItemChecked(i, false);
        }

    }
}
