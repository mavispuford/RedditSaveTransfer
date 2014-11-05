using System;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json.Linq;

namespace RedditSaveTransfer.Threading
{
    /// <summary>
    /// Grabs the saved posts from the specified Reddit account
    /// </summary>
    class GetSavedThread : WorkerThread
    {
        private readonly CookieContainer _cookie;    //Cookie that will be used
        
        public GetSavedThread(CookieContainer cookie)
        {
            _cookie = cookie;
        }

        protected override void thread_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            base.thread_DoWork(sender, e);

            var savedPosts = GrabSaved();

            savedPosts.Reverse();

            e.Result = savedPosts;
        }

        /// <summary>
        /// Grabs the saved posts
        /// </summary>
        /// <returns>List of posts</returns>
        private List<SavedListing> GrabSaved()
        {
            var currentId = "current";
            var posts = new List<SavedListing>();
            var savedChunks = new List<JObject>();

            // Loop until we dont have an "after" id
            do
            {
                Thread.ReportProgress(savedChunks.Count * 10, "Working... (Saved Chunks: " + savedChunks.Count + ")");

                Console.WriteLine("Saved Chunks: " + savedChunks.Count);

                var result = "";
                if (currentId != "current")
                    result = HttpUtility.SendGet(Common.BaseUrl + "/saved.json?limit=100&after=" + currentId, _cookie);
                else
                    result = HttpUtility.SendGet(Common.BaseUrl + "/saved.json?limit=100", _cookie);

                savedChunks.Add(JObject.Parse(result));

                currentId = (string)savedChunks[savedChunks.Count - 1]["data"].SelectToken("after");

                Console.WriteLine("Current ID: " + currentId);

                System.Threading.Thread.Sleep(2000);

            } while (currentId != null);

            Console.WriteLine("Final Chunk Count: " + savedChunks.Count);

            // Add posts to the list
            foreach (var j in savedChunks)
            {
                foreach (var child in j["data"]["children"])
                {
                    var listing = new SavedListing();

                    foreach (var jToken in child["data"])
                    {
                        var jProp = (JProperty) jToken;
                        listing.Properties.Add(new KeyValuePair<string, string>(jProp.Name, jProp.Value.ToString()));
                    }

                    posts.Add(listing);
                }
            }

            return posts;
        }

    }
}
