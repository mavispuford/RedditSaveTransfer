using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace RedditSaveTransfer
{
    /// <summary>
    /// Basic class used for threading
    /// </summary>
    public class WorkerThread
    {
        BackgroundWorker thread;

        public BackgroundWorker Thread
        {
            get { return thread; }
        }

        public WorkerThread()
        {
            thread = new BackgroundWorker { WorkerReportsProgress = true, WorkerSupportsCancellation = true };

            thread.DoWork += new DoWorkEventHandler(thread_DoWork);
            thread.ProgressChanged += new ProgressChangedEventHandler(thread_ProgressChanged);
            thread.RunWorkerCompleted += new RunWorkerCompletedEventHandler(thread_RunWorkerCompleted);
        }

        public void Start()
        {
            thread.RunWorkerAsync();
        }

        public void Stop()
        {
            thread.CancelAsync();
            thread.Dispose();
        }

        public virtual void thread_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {

        }

        public virtual void thread_DoWork(object sender, DoWorkEventArgs e)
        {
            if (thread.CancellationPending)
                return;
        }

        public virtual void thread_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

        }

    }
}
