using System;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json.Linq;

namespace RedditSaveTransfer.Threading
{
    /// <summary>
    /// Saves/Unsaves the given list of posts to the specified Reddit account
    /// </summary>
    class SavePostThread : WorkerThread
    {
        readonly List<SavedListing> _postsToSave = new List<SavedListing>();

        private readonly CookieContainer _cookie;    //Cookie that will be used
        private string _modHash;                     //Modhash of the user
        private readonly bool _save;                 //TRUE = SAVE, FALSE = UNSAVE

        public SavePostThread(CookieContainer cookie, List<SavedListing> posts, bool save)
        {
            _cookie = cookie;
            _postsToSave = posts;
            _save = save;
        }

        protected override void thread_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            base.thread_DoWork(sender, e);

            Thread.ReportProgress(0);

            if (GetModHash())
                SavePosts();

            Thread.ReportProgress(100);
        }

        /// <summary>
        /// Gets the user's modhash so we can use it to send a POST
        /// </summary>
        /// <returns>True if successful (the modhash is not null or empty)</returns>
        private bool GetModHash()
        {
            var response = HttpUtility.SendGet(Common.BaseUrl + "/api/me.json", _cookie);

            var jObject = JObject.Parse(response);

            var property = (JValue) jObject["data"].SelectToken("modhash", false);

            if (property != null)
            {
                _modHash = property.Value.ToString();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Saves the posts to the Reddit account
        /// </summary>
        private void SavePosts()
        {
            var count = 1;

            foreach (var listing in _postsToSave)
            {
                var progress = (int)((( (double)count - 1 ) / _postsToSave.Count) * 100);

                foreach (var pair in listing.Properties)
                {
                    if (pair.Key != "name") continue;

                    //If we are SAVING
                    if (_save)
                    {
                        Console.WriteLine("Saving \"" + pair.Value + "\"");
                        Thread.ReportProgress(progress, "Saving Post " + count + " of " + _postsToSave.Count);

                        Console.WriteLine("Progress: " + (int)((( (double)count - 1 ) / _postsToSave.Count) * 100));
                            
                        //SEND POST
                        Console.WriteLine("POST: " + Common.BaseUrl + "/api/save?id=" + pair.Value + "&uh=" + _modHash);
                        Console.WriteLine("RESPONSE: " + HttpUtility.SendPost(Common.BaseUrl + "/api/save", "id=" + pair.Value + "&uh=" + _modHash, _cookie));
                    }
                    else //If we are UNSAVING
                    {
                        Console.WriteLine("Unsaving \"" + pair.Value + "\"");
                        Thread.ReportProgress(progress, "Unsaving Post " + count + " of " + _postsToSave.Count);

                        //SEND POST
                        Console.WriteLine("POST: " + Common.BaseUrl + "/api/unsave?id=" + pair.Value + "&uh=" + _modHash);
                        Console.WriteLine("RESPONSE: " + HttpUtility.SendPost(Common.BaseUrl + "/api/unsave", "id=" + pair.Value + "&uh=" + _modHash, _cookie));
                    }

                    //SLEEP 2000 MS
                    System.Threading.Thread.Sleep(2000);

                    count++;
                    break;
                }
            }
        }
    }
}
