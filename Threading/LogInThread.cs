using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json.Linq;

namespace RedditSaveTransfer.Threading
{
    class LogInThread : WorkerThread
    {
        private readonly string _username;           //Username of the account
        private readonly string _password;           //Password of the account

        private readonly CookieContainer _cookie;    //Cookie that will be used
        private readonly string _cookieFileName;     //Filename of the cookie

        public LogInThread(string username, string password, ref CookieContainer cookie, string cookieFileName)
        {
            _username = username;
            _password = password;
            _cookie = cookie;
            _cookieFileName = cookieFileName;
        }

        protected override void thread_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            base.thread_DoWork(sender, e);

            Thread.ReportProgress(0);

            var result = LogIn();
            
            Console.WriteLine("LogIn result: " + result);

            e.Result = result;
        }

        /// <summary>
        /// Logs into the reddit servers
        /// </summary>
        /// <returns>Server response</returns>
        private JObject LogIn()
        {
            var url = Common.BaseUrl + "/api/login/" + _username;
            var body = new Dictionary<string, string>
            {
                {"user", _username}, 
                {"passwd", _password}
            };

            var postData = string.Format("api_type=json&user={0}&passwd={1}", body["user"], body["passwd"]);

            var result = HttpUtility.SendPost(url, postData, _cookie);

            var jResult = JObject.Parse(result);
            
            //If there are no errors
            if (!jResult["json"].SelectToken("errors").HasValues)
            {
                //Save the cookie
                SaveCookie(_cookie, _cookieFileName);
            }

            return jResult;
        }

        /// <summary>
        /// Saves the cookie to a temporary file
        /// </summary>
        /// <param name="container">CookieContainer</param>
        /// <param name="filename">Filename of the cookie</param>
        private static void SaveCookie(CookieContainer container, string filename)
        {
            try
            {
                Console.WriteLine("Writing cookie \"" + filename + "\"");
                Stream stream = File.Open(filename, FileMode.Create);
                var bFormatter = new BinaryFormatter();
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
