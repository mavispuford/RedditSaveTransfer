using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json.Linq;
using System.Windows.Forms;

namespace RedditSaveTransfer
{
    /// <summary>
    /// Grabs the saved posts from the specified Reddit account
    /// </summary>
    class GetSavedThread : WorkerThread
    {
        private CookieContainer mCookie;    //Cookie that will be used
        private string mUserAgent;          //User Agent string
        
        public GetSavedThread(CookieContainer cookie, string userAgent)
        {
            mCookie = cookie;
            mUserAgent = userAgent;
        }

        public override void thread_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            base.thread_DoWork(sender, e);

            List<SavedListing> savedPosts = GrabSaved();

            savedPosts.Reverse();

            e.Result = savedPosts;
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
        /// Grabs the saved posts
        /// </summary>
        /// <returns>List of posts</returns>
        private List<SavedListing> GrabSaved()
        {
            string currentId = "current";
            string result = "";
            List<SavedListing> posts = new List<SavedListing>();
            List<JObject> savedChunks = new List<JObject>();

            //Loop until we dont have an "after" id
            do
            {
                Thread.ReportProgress(savedChunks.Count * 10, "Working... (Saved Chunks: " + savedChunks.Count + ")");

                Console.WriteLine("Saved Chunks: " + savedChunks.Count);

                if (currentId != "current")
                    result = SendGET("http://www.reddit.com/saved.json?limit=100&after=" + currentId);
                else
                    result = SendGET("http://www.reddit.com/saved.json?limit=100");

                savedChunks.Add(JObject.Parse(result));

                currentId = (string)savedChunks[savedChunks.Count - 1]["data"].SelectToken("after");

                Console.WriteLine("Current ID: " + currentId);

                System.Threading.Thread.Sleep(2000);

            } while (currentId != null);

            Console.WriteLine("Final Chunk Count: " + savedChunks.Count);

            //Add posts to the list
            foreach (JObject j in savedChunks)
            {
                foreach (JObject child in j["data"]["children"])
                {
                    SavedListing listing = new SavedListing();

                    foreach (JProperty jProp in child["data"])
                        listing.Properties.Add(new KeyValuePair<string, string>(jProp.Name.ToString(), jProp.Value.ToString()));

                    posts.Add(listing);
                }
            }

            return posts;
        }


        /// <summary>
        /// Send a GET to the specified server
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

            //Grab the default proxy from IE Internet Settings
            var proxy = WebRequest.GetSystemWebProxy();
            WebProxy wp = new WebProxy();
            wp.Credentials = proxy.Credentials;
            wp.Address = proxy.GetProxy(request.RequestUri);
            request.Proxy = wp;

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
