using System;
using System.Threading;
using Metrics.Reporters;

namespace Metrics
{
    class Worker {
        private readonly Random mRand = new Random();

        private Thread mWorkerThread;
        private CancellationTokenSource mTokenSource;

        private readonly int mId;
        private readonly IReporter mReporter;

        public Worker(int id, IReporter reporter)
        {
            mId = id;
            mReporter = reporter;
        }

        public void Start() {
            mTokenSource = new CancellationTokenSource();
            mWorkerThread = new Thread(() => DoWork(mTokenSource.Token));

            mWorkerThread.Start();
        }

        public void Stop() {
            mTokenSource.Cancel();
            mWorkerThread.Join();
        }

        private void DoWork(CancellationToken token) {
            while(!token.IsCancellationRequested) {
                var opLength = mRand.Next(100, 3000);
                var op = (OperationType)mRand.Next(0, 3);

                Thread.Sleep(opLength);

                mReporter.ReportValue("Worker.Operation", opLength, new { operation = op, workerId = mId });
            }
        }
    }

    enum OperationType {
        Type1,
        Type2,
        Type3
    }
}