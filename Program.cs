using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;



namespace TestMetrics
{
    class Program
    {
        static void Main(string[] args)
        {
            List<Task> workers = new List<Task>();

            CancellationTokenSource source = new CancellationTokenSource();

            for(var i = 0; i < 10; i++)
            {
                var worker = StartWorker(i, source.Token);
                workers.Add(worker);
            }

            var producer = StartProducer(source.Token);

            Console.WriteLine("Press button to stop...");
            Console.ReadKey();
            Console.WriteLine("Stopping...");

            source.Cancel();

            producer.Wait();
            Console.WriteLine("Producer stopped...");
            Task.WaitAll(workers.ToArray());
            Console.WriteLine("Consumers stopped...");
        }

        private static SemaphoreSlim sWorkSemaphore = new SemaphoreSlim(0);

        private static Task StartWorker(int id, CancellationToken token)
        {
            var rand = new Random();

            return Task.Factory.StartNew(() =>
            {
                Console.WriteLine($"\t\tWorker {id} started");

                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        sWorkSemaphore.Wait(token);

                        Console.WriteLine($"\t\tWorker {id} doing work");

                        var workMs = rand.Next(100, 1000);

                        if (workMs > 950)
                        {
                            throw new InvalidOperationException($"Error! Rand: {workMs}");
                        }

                        Thread.Sleep(workMs);

                        Console.WriteLine($"\t\tWorker {id} done");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"\t\tException in Worker {id}: {ex.Message}");
                    }
                }
            }, token);
        }

        private static Task StartProducer(CancellationToken token)
        {
            var producingRand = new Random();

            return Task.Factory.StartNew(() =>
            {
                Console.WriteLine("\tProducer started");

                while (!token.IsCancellationRequested)
                {
                    sWorkSemaphore.Release(1);
                    //Console.WriteLine("\tWork signalled");

                    var produceDelay = producingRand.Next(0, 100);
                    try
                    {
                        Task.Delay(produceDelay, token).Wait();
                    }
                    catch
                    {

                    }
                    
                }

            }, token);
        }
    }
}
