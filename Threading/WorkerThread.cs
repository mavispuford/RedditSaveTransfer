using System.ComponentModel;

namespace RedditSaveTransfer.Threading
{
    /// <summary>
    /// Basic class used for threading
    /// </summary>
    public class WorkerThread
    {
        public BackgroundWorker Thread { get; private set; }

        public WorkerThread()
        {
            Thread = new BackgroundWorker { WorkerReportsProgress = true, WorkerSupportsCancellation = true };

            Thread.DoWork += thread_DoWork;
            Thread.ProgressChanged += thread_ProgressChanged;
            Thread.RunWorkerCompleted += thread_RunWorkerCompleted;
        }

        public void Start()
        {
            Thread.RunWorkerAsync();
        }

        public void Stop()
        {
            Thread.CancelAsync();
            Thread.Dispose();
        }

        protected virtual void thread_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {

        }

        protected virtual void thread_DoWork(object sender, DoWorkEventArgs e)
        {
            if (Thread.CancellationPending)
                return;
        }

        protected virtual void thread_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

        }

    }
}
