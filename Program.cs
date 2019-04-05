using System;
using System.Collections.Generic;
using System.Reflection;
using App.Metrics;
using Metrics.Reporters;

namespace Metrics
{
    class Program
    {
        static void Main(string[] args)
        {
            var reporter = new AppMetricsReporter();
            
            var workers = new List<Worker>();            
            for(var i = 0; i < 5; i++)
            {
                var worker = new Worker(i, reporter);
                worker.Start();
                workers.Add(worker);
            }

            foreach(var w in workers)
                w.Start();

            Console.Read();

            Console.WriteLine("Stopping");

            foreach(var w in workers)
                w.Stop();

            reporter.Flush();

            Console.WriteLine("Stopped");
        }
    }
}