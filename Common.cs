
using System.Collections.Generic;

namespace RedditSaveTransfer
{
    public static class Common
    {
        public const string UserAgent = "Reddit Saved Post Transfer Tool by MavisPuford"; //User Agent string
        public const string BaseUrl = "https://www.reddit.com";

        public static HashSet<string> PropertiesToExport;
        public static readonly string[] DefaultPropertiesToExport = { "subreddit", "url", "title", "created_utc" };
    }
}
