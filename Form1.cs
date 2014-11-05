using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RedditSaveTransfer.Exporting;
using RedditSaveTransfer.Properties;
using RedditSaveTransfer.Threading;

namespace RedditSaveTransfer
{
    public partial class Form1 : Form
    {
        CookieContainer redditCookie1;                              //Cookie of the LEFT user account
        string cookieFileName1 = "cookie1";

        CookieContainer redditCookie2;                              //Cookie of the RIGHT user account
        string cookieFileName2 = "cookie2";

        List<string> cookieJar = new List<string>();                //For keeping track of the different cookie filenames used during the session

        List<SavedListing> savedPosts = new List<SavedListing>();   //Saved posts that were grabbed from the LEFT account
        List<SavedListing> toSave = new List<SavedListing>();       //Saved posts that will be saved to the RIGHT account
        int _currentPost;                                           //Currently selected post (for the DataGridView)

        public Form1()
        {
            InitializeComponent();

            redditCookie1 = new CookieContainer();
            redditCookie2 = new CookieContainer();

            if (Settings.Default.SaveUsername1)
                txtUsername1.Text = Settings.Default.Username1;

            if (Settings.Default.SaveUsername2)
                txtUsername2.Text = Settings.Default.Username2;

            if (!string.IsNullOrEmpty(Settings.Default.PropertiesToExport))
            {
                Common.PropertiesToExport = new HashSet<string>(Settings.Default.PropertiesToExport.Split(','));
            }
            else
            {
                Common.PropertiesToExport = new HashSet<string>(Common.DefaultPropertiesToExport);
                Settings.Default.PropertiesToExport = String.Join(",", Common.PropertiesToExport.ToArray());
                Settings.Default.Save();
            }
        }
        
        protected override void OnLoad(EventArgs e)
        {
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            //For testing different dpi settings

            //this.Font = new Font(this.Font.FontFamily, this.Font.Size * 120 / 96);
            //this.Font = new Font(this.Font.FontFamily, this.Font.Size * 144 / 96);
            //this.Font = new Font(this.Font.FontFamily, this.Font.Size * 192 / 96);
            base.OnLoad(e);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);

            //Delete the cookies that were used
            try
            {
                foreach (var s in cookieJar)
                    File.Delete(s);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error deleting cookie: " + ex.Message);
            }

        }

        private void btnLoadSaved_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(txtUsername1.Text) && !String.IsNullOrEmpty(txtPassword1.Text))
            {
                if (Settings.Default.SaveUsername1)
                {
                    Settings.Default.Username1 = txtUsername1.Text;
                    Settings.Default.Save();
                }

                btnCopyPosts.Enabled = false;
                btnExport.Enabled = false;
                btnLoadSaved.Enabled = false;
                btnPrevious.Enabled = false;
                btnNext.Enabled = false;
                btnUnsave.Enabled = false;

                cookieFileName1 = txtUsername1.Text;
                redditCookie1 = Loadcookie(cookieFileName1);

                //Add the cookie filename to the cookies list for deletion later
                AddToCookieJar(txtUsername1.Text);

                if (redditCookie1 == null)
                {
                    redditCookie1 = new CookieContainer();
                    LogIn(true);
                }
                else
                {
                    GrabPosts();
                }
            }
            else
            {
                MessageBox.Show("Make sure the username and password fields are not blank.");
            }
        }

        private CookieContainer Loadcookie(string filename)
        {
            CookieContainer container = null;

            try
            {
                var stream = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.None);
                var bFormatter = new BinaryFormatter();

                container = (CookieContainer)bFormatter.Deserialize(stream);

                stream.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Problem loading cookie: " + e.Message);
            }

            return container;
        }

        private void LogIn(bool leftAccount)
        {
            string username, password, filename;
            CookieContainer cookie;

            if (leftAccount)
            {
                username = txtUsername1.Text;
                password = txtPassword1.Text;
                cookie = redditCookie1;
                filename = cookieFileName1;
            }
            else
            {
                username = txtUsername2.Text;
                password = txtPassword2.Text;
                cookie = redditCookie2;
                filename = cookieFileName2;
            }

            statusLabel.Text = "Logging In...";

            //Start the LogInthread
            var logInThread = new LogInThread(username, password, ref cookie, filename);

            logInThread.Thread.ProgressChanged += Login_ProgressChanged;

            if (leftAccount)
                logInThread.Thread.RunWorkerCompleted += Login_Completed_1;
            else
                logInThread.Thread.RunWorkerCompleted += Login_Completed_2;

            logInThread.Start();
        }

        /// <summary>
        /// Adds a cookie filename to the cookies list for deletion later
        /// </summary>
        /// <param name="cookie">filename to add</param>
        void AddToCookieJar(string cookie)
        {
            if (!cookieJar.Contains(cookie))
                cookieJar.Add(cookie);
        }

        void Login_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            toolStripProgressBar1.Value = e.ProgressPercentage;
        }

        void Login_Completed_1(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                var result = (JObject)e.Result;

                //Check for server errors
                if (result["json"].SelectToken("errors").HasValues)
                {
                    IList<string> errors = result["json"]["errors"][0].Select(t => (string)t).ToList();
                    MessageBox.Show("Error logging in. Server message:\n\"" + errors[1] + "\"");

                    statusLabel.Text = "Login Error";

                    btnLoadSaved.Enabled = true;
                }
                else
                {
                    toolStripProgressBar1.Value = 100;
                    GrabPosts();
                }
            }
            else
            {
                statusLabel.Text = "Login Error";

                btnLoadSaved.Enabled = true;
            }
        }

        void Login_Completed_2(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                var result = (JObject)e.Result;

                //Check for server errors
                if (result["json"].SelectToken("errors").HasValues)
                {
                    IList<string> errors = result["json"]["errors"][0].Select(t => (string)t).ToList();
                    MessageBox.Show("Error logging in. Server message:\n\"" + errors[1] + "\"");

                    statusLabel.Text = "Login Error";

                    btnLoadSaved.Enabled = true;
                    btnCopyPosts.Enabled = true;
                }
                else
                {
                    toolStripProgressBar1.Value = 100;
                    SavePosts();
                }
            }
            else
            {
                statusLabel.Text = "Login Error";

                btnLoadSaved.Enabled = true;
            }
        }

        void SavePosts()
        {
            //Filter the list of posts to be saved
            FilterPosts(chkMatchRows.Checked);

            //Start the save thread
            var davePostThread = new SavePostThread(redditCookie2, toSave, true);

            davePostThread.Thread.ProgressChanged += SavePosts_ProgressChanged;
            davePostThread.Thread.RunWorkerCompleted += SavePosts_Completed;

            davePostThread.Start();  
        }

        /// <summary>
        /// Filters the list of saved posts according to which rows were selected in the DataGridView
        /// </summary>
        /// <param name="matchRows">If TRUE, match the selected rows</param>
        void FilterPosts(bool matchRows)
        {
            toSave.Clear();

            if (!matchRows)
                toSave = new List<SavedListing>(savedPosts);
            else
            {
                var pulledFromBigList = false;
                var toRemove = new List<SavedListing>();

                //Loop through selected rows
                foreach (DataGridViewRow c in dataGridView1.SelectedRows)
                {
                    //First run looks through the big list (savedPosts)
                    if (!pulledFromBigList)
                    {
                        pulledFromBigList = true;
                        foreach (var listing in savedPosts)
                        {
                            var add = true;
                            foreach (var pair in listing.Properties)
                            {
                                if (pair.Key != c.Cells[0].Value.ToString()) continue;

                                if (pair.Value == c.Cells[1].Value.ToString())
                                    break;
                                    
                                add = false;
                            }

                            if (add)
                                toSave.Add(listing);
                        }
                    }
                    else //Second run and on - look through the filtered list
                    {
                        foreach (var listing in toSave)
                        {
                            var remove = false;
                            foreach (var pair in listing.Properties)
                            {
                                if (pair.Key != c.Cells[0].Value.ToString()) continue;

                                if (pair.Value == c.Cells[1].Value.ToString())
                                    break;

                                remove = true;
                            }

                            if (remove)
                                toRemove.Add(listing);
                        }
                    }
                }

                //Remove listings that don't match
                foreach (var listing in toRemove)
                    toSave.Remove(listing);

                Console.WriteLine("Found " + toSave.Count + " posts.");
            }
        }

        void SavePosts_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            statusLabel.Text = (string)e.UserState;

            toolStripProgressBar1.Value = e.ProgressPercentage;
        }

        void SavePosts_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            statusLabel.Text = "DONE SAVING";

            //If we need to unsave the posts in the LEFT account
            if (chkUnsaveAfter.Checked)
            {
                var savePostThread = new SavePostThread(redditCookie1, toSave, false);

                savePostThread.Thread.ProgressChanged += UnSavePosts_ProgressChanged;
                savePostThread.Thread.RunWorkerCompleted += UnSavePosts_Completed;

                savePostThread.Start();
            }
            else
            {
                Console.WriteLine("DONE");

                MessageBox.Show("Finished saving " + toSave.Count + " posts.");

                btnCopyPosts.Enabled = true;
                btnLoadSaved.Enabled = true;
                btnUnsave.Enabled = true;

                toolStripProgressBar1.Value = 100;
            }
        }

        void UnSavePosts_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            statusLabel.Text = (string)e.UserState;
            toolStripProgressBar1.Value = e.ProgressPercentage;
        }

        void UnSavePosts_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            Console.WriteLine("DONE");

            foreach (var listing in toSave)
                savedPosts.Remove(listing);

            _currentPost = 0;
            dataGridView1.DataSource = savedPosts[_currentPost].Properties;

            UpdateSelectionText();
            
            MessageBox.Show("Finished unsaving " + toSave.Count + " posts.");

            btnCopyPosts.Enabled = true;
            btnLoadSaved.Enabled = true;
            btnUnsave.Enabled = true;

            toolStripProgressBar1.Value = 100;
        }

        void GrabPosts()
        {
            statusLabel.Text = "Grabbing Saved Posts...";

            var getSavedThread = new GetSavedThread(redditCookie1);

            getSavedThread.Thread.RunWorkerCompleted += GrabPosts_Completed;
            getSavedThread.Thread.ProgressChanged += GrabPosts_ProgressChanged;

            getSavedThread.Start();
        }

        void GrabPosts_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            statusLabel.Text = (string)e.UserState;

            toolStripProgressBar1.Value = e.ProgressPercentage;
        }

        void GrabPosts_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            savedPosts = (List<SavedListing>)e.Result;

            if (savedPosts != null)
            {
                if (savedPosts.Count > 0)
                {
                    btnCopyPosts.Enabled = true;
                    btnExport.Enabled = true;
                    btnPrevious.Enabled = true;
                    btnNext.Enabled = true;
                    btnUnsave.Enabled = true;

                    Console.WriteLine("Total Posts: " + savedPosts.Count);

                    _currentPost = 0;

                    dataGridView1.DataSource = null;
                    dataGridView1.DataSource = savedPosts[_currentPost].Properties;

                    dataGridView1.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    dataGridView1.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

                    UpdateSelectionText();
                }
                else
                {
                    statusLabel.Text = "[No Saved Post Entries]";

                    MessageBox.Show("The saved post list is empty.");
                }
            }

            btnLoadSaved.Enabled = true;

            toolStripProgressBar1.Value = 100;
        }

        //Outputs the given JSON data - for testing
        void PrintJson(string json)
        {
            var reader = new JsonTextReader(new StringReader(json));

            while (reader.Read())
            {
              if (reader.Value != null)
                Console.WriteLine("Token: {0}, Value: {1}", reader.TokenType, reader.Value);
              else
                Console.WriteLine("Token: {0}", reader.TokenType);
            }
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            if (_currentPost + 1 < savedPosts.Count)
                _currentPost++;
            else
                _currentPost = 0;

            if (savedPosts.Count > 0)
            {
                dataGridView1.DataSource = savedPosts[_currentPost].Properties;

                dataGridView1.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                dataGridView1.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            }

            UpdateSelectionText();
        }

        private void btnPrevious_Click(object sender, EventArgs e)
        {
            if (_currentPost - 1 >= 0)
                _currentPost--;
            else
                _currentPost = savedPosts.Count - 1;

            if (savedPosts.Count > 0)
            {
                dataGridView1.DataSource = savedPosts[_currentPost].Properties;

                dataGridView1.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                dataGridView1.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            }

            UpdateSelectionText();
        }

        private void UpdateSelectionText()
        {
            if (savedPosts.Count > 0)
                statusLabel.Text = "[" + (_currentPost + 1).ToString() + "/" + savedPosts.Count + "]";
            else
                statusLabel.Text = "[List Empty]";
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            FilterPosts(chkMatchRows.Checked);

            var saveFileDialog = new SaveFileDialog
            {
                InitialDirectory = Directory.GetCurrentDirectory(),
                Filter =
                    "HTML Files (*.html) |*.html; *.HTML; *.Html |CSV Files (*.csv) |*.csv; *.CSV; *.Csv |All files (*.*)|*.*",
                FilterIndex = 0,
                RestoreDirectory = true,
                Title = "Export " + toSave.Count + " Posts..."
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    Console.WriteLine("File Extension: " + Path.GetExtension(saveFileDialog.FileName.ToLower()));

                    switch (Path.GetExtension(saveFileDialog.FileName.ToLower()))
                    {
                        case ".csv":
                            var csv = new CsvExport(toSave);
                            csv.ExportToFile(saveFileDialog.FileName);
                            break;
                        case ".html":
                            var html = new HtmlExport(toSave);
                            html.ExportToFile(saveFileDialog.FileName);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not save file to disk. Error: " + ex.Message);
                }
            }         

        }

        private void btnCopyPosts_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(txtUsername2.Text) && !String.IsNullOrEmpty(txtPassword2.Text))
            {
                if (Settings.Default.SaveUsername2)
                {
                    Settings.Default.Username2 = txtUsername2.Text;
                    Settings.Default.Save();
                }

                var message = "";

                FilterPosts(chkMatchRows.Checked);

                if (chkUnsaveAfter.Checked && chkMatchRows.Checked)
                    message = "Are you sure you want to move " +  toSave.Count + " post(s) from the left account to the right account?\n\n         (This WILL unsave the posts from the LEFT account)";
                else if (chkUnsaveAfter.Checked)
                    message = "Are you sure you want to move ALL the posts from the left account to the right account?\n\n         (This WILL unsave the posts from the LEFT account)";
                else if (chkMatchRows.Checked)
                    message = "Are you sure you want to COPY " + toSave.Count + " post(s) from the left account to the right account?\n\n         (This will NOT unsave the posts from the LEFT account)";
                else
                    message = "Are you sure you want to COPY all the posts from the left account to the right account?\n\n      (This will NOT unsave the posts from the LEFT account)";

                DialogResult result = MessageBox.Show(message, "Are you sure?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    btnCopyPosts.Enabled = false;
                    btnLoadSaved.Enabled = false;
                    btnUnsave.Enabled = false;

                    cookieFileName2 = txtUsername2.Text;
                    redditCookie2 = Loadcookie(cookieFileName2);

                    //Add the cookie filename to the cookies list for deletion later
                    AddToCookieJar(txtUsername2.Text);

                    if (redditCookie2 == null)
                    {
                        redditCookie2 = new CookieContainer();
                        LogIn(false);
                    }
                    else
                    {
                        SavePosts();
                    }
                }
                else if (result == DialogResult.No)
                {
                    btnCopyPosts.Enabled = true;
                    btnLoadSaved.Enabled = true;
                    btnUnsave.Enabled = true;

                    Console.WriteLine("Unsave Canceled.");
                }
            }
            else
            {
                MessageBox.Show("Make sure the username and password fields are not blank.");
            }
            
        }

        private void btnUnsave_Click(object sender, EventArgs e)
        {
            btnCopyPosts.Enabled = false;
            btnLoadSaved.Enabled = false;
            btnUnsave.Enabled = false;
            //btnExport.Enabled = false;

            FilterPosts(chkMatchRows.Checked);

            DialogResult result = MessageBox.Show("This will UNSAVE " + toSave.Count + " posts!  Are you sure you want to do this?", "Sure about that?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                FilterPosts(chkMatchRows.Checked);

                var savePostThread = new SavePostThread(redditCookie1, toSave, false);

                savePostThread.Thread.ProgressChanged += UnSavePosts_ProgressChanged;
                savePostThread.Thread.RunWorkerCompleted += UnSavePosts_Completed;

                savePostThread.Start();
            }
            else if (result == DialogResult.No)
            {
                btnCopyPosts.Enabled = true;
                btnLoadSaved.Enabled = true;
                btnUnsave.Enabled = true;

                Console.WriteLine("Unsave Canceled.");
            }

        }

        private void btnClearSelection_Click(object sender, EventArgs e)
        {
            dataGridView1.ClearSelection();
        }

        private void btnHelp_Click(object sender, EventArgs e)
        {
            ShowHelp();
        }

        private void ShowHelp()
        {
            MessageBox.Show(
                "First, put in your credentials and click \"Load Saved Posts\".\n\n" +
                "After that, you can export the saved posts to a file (HTML and CSV),\n" +
                "unsave them, or copy the posts to another Reddit account\n" +
                "by putting in the credentials on the right and clicking \"Copy!\"\n\n" +
                "Checking \"Unsave from left account after saving\" unsaves the\n" +
                "posts after they have been copied to the new account.\n\n" +
                "Checking \"Match selected rows\" transfers/exports only the \n" +
                "posts that match the selected rows in the table.\n\n" +
                "Want to unsave just one post?  Just check \"Match selected rows,\"\n" +
                "go to the post and select its id, then click \"Unsave.\"\n\n" +
                "**ALSO, IT TAKES A WHILE TO SAVE/UNSAVE POSTS.**\n" +
                "They have to be done one at a time, and Reddit's servers have\n" +
                "limits on how often you can make requests.\n" +
                "(about one request every 2 seconds)"
                , "Usage", MessageBoxButtons.OK, MessageBoxIcon.Question);
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            if (!Settings.Default.FirstRun) return;

            ShowHelp();

            Settings.Default.FirstRun = false;
            Settings.Default.Save();
        }

        private void btnExportOptions_Click(object sender, EventArgs e)
        {
            var properties = new SelectPropertiesWindow {StartPosition = FormStartPosition.CenterParent};

            properties.ShowDialog();
        }

    }
}
