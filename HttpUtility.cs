using System;
using System.IO;
using System.Net;
using System.Text;

namespace RedditSaveTransfer
{
    public static class HttpUtility
    {
        /// <summary>
        /// Sends a POST to the specified server
        /// </summary>
        /// <param name="uri">URI to send</param>
        /// <param name="data">Additional data</param>
        /// <param name="cookie">The cookie to associate with the request</param>
        /// <returns>Server response</returns>
        public static string SendPost(string uri, string data, CookieContainer cookie)
        {
            var request = (HttpWebRequest)WebRequest.Create(uri);
            request.CookieContainer = cookie;
            request.UserAgent = Common.UserAgent;
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;

            //Grab the default proxy from IE Internet Settings
            var proxy = WebRequest.GetSystemWebProxy();
            var wp = new WebProxy
            {
                Credentials = proxy.Credentials,
                Address = proxy.GetProxy(request.RequestUri)
            };
            request.Proxy = wp;

            //Encode the data
            using (var writeStream = request.GetRequestStream())
            {
                var encoding = new UTF8Encoding();
                var bytes = encoding.GetBytes(data);
                writeStream.Write(bytes, 0, bytes.Length);
            }

            //Send the request and get the server's response
            var result = string.Empty;
            using (var response = (HttpWebResponse)request.GetResponse())
            {
                using (var responseStream = response.GetResponseStream())
                {
                    using (var readStream = new StreamReader(responseStream, Encoding.UTF8))
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
        /// <param name="cookie">The cookie to associate with the request</param>
        /// <returns>Server response</returns>
        public static string SendGet(string uri, CookieContainer cookie)
        {
            HttpWebRequest request = null;

            request = (HttpWebRequest)WebRequest.Create(uri);
            request.CookieContainer = cookie;
            request.UserAgent = Common.UserAgent;
            request.Method = "GET";
            request.ContentType = "application/x-www-form-urlencoded";
            request.KeepAlive = false;
            request.ProtocolVersion = HttpVersion.Version10;

            //Grab the default proxy from IE Internet Settings
            var proxy = WebRequest.GetSystemWebProxy();
            var wp = new WebProxy
            {
                Credentials = proxy.Credentials,
                Address = proxy.GetProxy(request.RequestUri)
            };
            request.Proxy = wp;

            var result = string.Empty;

            try
            {
                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    using (var responseStream = response.GetResponseStream())
                    {
                        using (var readStream = new StreamReader(responseStream, Encoding.UTF8))
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
