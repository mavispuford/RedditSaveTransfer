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
    class LogInThread : WorkerThread
    {
        private string mUsername;           //Username of the account
        private string mPassword;           //Password of the account
        private string mUserAgent;          //User Agent string

        private CookieContainer mCookie;    //Cookie that will be used
        private string mCookieFileName;     //Filename of the cookie

        public LogInThread(string username, string password, ref CookieContainer cookie, string cookieFileName, string userAgent)
        {
            mUsername = username;
            mPassword = password;
            mCookie = cookie;
            mCookieFileName = cookieFileName;
            mUserAgent = userAgent;
        }

        public override void thread_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            base.thread_DoWork(sender, e);

            Thread.ReportProgress(0);

            JObject result = LogIn();
            
            Console.WriteLine("LogIn result: " + result);

            e.Result = result;
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
        /// Logs into the reddit servers
        /// </summary>
        /// <returns>Server response</returns>
        private JObject LogIn()
        {
            string url = "http://www.reddit.com/api/login/" + mUsername;
            Dictionary<string, string> body = new Dictionary<string, string>();
            body.Add("user", mUsername);
            body.Add("passwd", mPassword);

            string postData = string.Format("api_type=json&user={0}&passwd={1}", body["user"], body["passwd"]);

            HttpWebRequest request = null;

            Uri uri = new Uri(url);
            request = (HttpWebRequest)WebRequest.Create(uri);

            //Grab the default proxy from IE Internet Settings
            var proxy = WebRequest.GetSystemWebProxy();
            WebProxy wp = new WebProxy();
            wp.Credentials = proxy.Credentials;
            wp.Address = proxy.GetProxy(uri);
            request.Proxy = wp;
            
            request.CookieContainer = mCookie;
            request.UserAgent = mUserAgent;
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = postData.Length;

            string result = string.Empty;

            try
            {

                //Encode the POST data
                using (Stream writeStream = request.GetRequestStream())
                {
                    UTF8Encoding encoding = new UTF8Encoding();
                    byte[] bytes = encoding.GetBytes(postData);
                    writeStream.Write(bytes, 0, bytes.Length);
                }

                //Send the request, get the server response
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using (Stream responseStream = response.GetResponseStream())
                    {
                        using (StreamReader readStream = new StreamReader(responseStream, Encoding.UTF8))
                        {
                            result = readStream.ReadToEnd();
                            Console.WriteLine(mCookie.GetCookieHeader(new System.Uri("http://www.reddit.com/api/login/" + mUsername)));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("There was a problem connecting to the server. Exception: " + ex.Message);
                return null;
            }

            JObject jResult = JObject.Parse(result);
            
            //If there are no errors
            if (!jResult["json"].SelectToken("errors").HasValues)
            {
                //Save the cookie
                SaveCookie(mCookie, mCookieFileName);
            }

            return jResult;
        }

        /// <summary>
        /// Saves the cookie to a temporary file
        /// </summary>
        /// <param name="container">CookieContainer</param>
        /// <param name="filename">Filename of the cookie</param>
        private void SaveCookie(CookieContainer container, string filename)
        {
            try
            {
                Console.WriteLine("Writing cookie \"" + filename + "\"");
                Stream stream = File.Open(filename, FileMode.Create);
                BinaryFormatter bFormatter = new BinaryFormatter();
                bFormatter.Serialize(stream, container);
                stream.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Problem writing cookie \"" + filename + "\"" + ": " + e.Message);
            }
        }

    }
}
