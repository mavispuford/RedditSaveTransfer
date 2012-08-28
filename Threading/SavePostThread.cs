using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;

namespace RedditSaveTransfer
{
    /// <summary>
    /// Saves/Unsaves the given list of posts to the specified Reddit account
    /// </summary>
    class SavePostThread : WorkerThread
    {
        List<SavedListing> postsToSave = new List<SavedListing>();

        private CookieContainer mCookie;    //Cookie that will be used
        private string mUserAgent;          //User Agent string
        private string mModHash;            //Modhash of the user
        private bool mSave;                 //TRUE = SAVE, FALSE = UNSAVE

        public SavePostThread(CookieContainer cookie, string userAgent, List<SavedListing> posts, bool save)
        {
            mCookie = cookie;
            mUserAgent = userAgent;
            postsToSave = posts;
            mSave = save;
        }

        public override void thread_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            base.thread_DoWork(sender, e);

            Thread.ReportProgress(0);

            if (GetModHash())
                SavePosts();

            Thread.ReportProgress(100);
        }

        public override void thread_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            base.thread_ProgressChanged(sender, e);            
        }

        public override void thread_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            base.thread_RunWorkerCompleted(sender, e);
        }

        /// <summary>
        /// Gets the user's modhash so we can use it to send a POST
        /// </summary>
        /// <returns>True if successful (the modhash is not null or empty)</returns>
        private bool GetModHash()
        {
            string response = SendGET("http://www.reddit.com/api/me.json");

            JObject jObject = JObject.Parse(response);

            JValue property = (JValue) jObject["data"].SelectToken("modhash", false);

            if (property != null)
            {
                mModHash = property.Value.ToString();
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Saves the posts to the Reddit account
        /// </summary>
        private void SavePosts()
        {
            int count = 1;
            int progress = 0;

            foreach (SavedListing listing in postsToSave)
            {
                progress = (int)((( (double)count - 1 ) / postsToSave.Count) * 100);

                foreach (KeyValuePair<string, string> pair in listing.Properties)
                {
                    if (pair.Key == "name")
                    {
                        //If we are SAVING
                        if (mSave)
                        {
                            Console.WriteLine("Saving \"" + pair.Value + "\"");
                            Thread.ReportProgress(progress, "Saving Post " + count + " of " + postsToSave.Count);

                            Console.WriteLine("Progress: " + (int)((( (double)count - 1 ) / postsToSave.Count) * 100));
                            
                            //SEND POST
                            Console.WriteLine("POST: " + "http://www.reddit.com/api/save?id=" + pair.Value + "&uh=" + mModHash);
                            Console.WriteLine("RESPONSE: " + SendPOST("http://www.reddit.com/api/save", "id=" + pair.Value + "&uh=" + mModHash));
                        }
                        else //If we are UNSAVING
                        {
                            Console.WriteLine("Unsaving \"" + pair.Value + "\"");
                            Thread.ReportProgress(progress, "Unsaving Post " + count + " of " + postsToSave.Count);

                            //SEND POST
                            Console.WriteLine("POST: " + "http://www.reddit.com/api/unsave?id=" + pair.Value + "&uh=" + mModHash);
                            Console.WriteLine("RESPONSE: " + SendPOST("http://www.reddit.com/api/unsave", "id=" + pair.Value + "&uh=" + mModHash));
                        }

                        //SLEEP 2000 MS
                        System.Threading.Thread.Sleep(2000);

                        count++;
                        break;
                    }
                }
            }
        }


        /// <summary>
        /// Sends a POST to the specified server
        /// </summary>
        /// <param name="uri">URI to send</param>
        /// <param name="data">Additional data</param>
        /// <returns>Server response</returns>
        private string SendPOST(string uri, string data)
        {
            HttpWebRequest request = null;

            request = (HttpWebRequest)WebRequest.Create(uri);
            request.CookieContainer = mCookie;
            request.UserAgent = mUserAgent;
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;

            //Encode the data
            using (Stream writeStream = request.GetRequestStream())
            {
                UTF8Encoding encoding = new UTF8Encoding();
                byte[] bytes = encoding.GetBytes(data);
                writeStream.Write(bytes, 0, bytes.Length);
            }

            //Send the request and get the server's response
            string result = string.Empty;
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                using (Stream responseStream = response.GetResponseStream())
                {
                    using (StreamReader readStream = new StreamReader(responseStream, Encoding.UTF8))
                    {
                        result = readStream.ReadToEnd();
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Sends a GET to the specified server
        /// </summary>
        /// <param name="uri">URI to send</param>
        /// <returns>Server response</returns>
        private string SendGET(string uri)
        {
            HttpWebRequest request = null;

            request = (HttpWebRequest)WebRequest.Create(uri);
            request.CookieContainer = mCookie;
            request.UserAgent = mUserAgent;
            request.Method = "GET";
            request.ContentType = "application/x-www-form-urlencoded";
            request.KeepAlive = false;
            request.ProtocolVersion = HttpVersion.Version10;

            string result = string.Empty;

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using (Stream responseStream = response.GetResponseStream())
                    {
                        using (StreamReader readStream = new StreamReader(responseStream, Encoding.UTF8))
                        {
                            result = readStream.ReadToEnd();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error sending GET message: " + e.Message);
            }

            return result;
        }

    }
}
