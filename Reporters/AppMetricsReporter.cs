using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using App.Metrics;
using App.Metrics.Counter;
using App.Metrics.Extensions.Hosting;
using App.Metrics.Gauge;
using App.Metrics.Timer;

namespace Metrics.Reporters
{
    class AppMetricsReporter : IReporter
    {
        private MetricsReporterBackgroundService mReportingService;

        private IMetricsRoot mMetrics;

        public AppMetricsReporter()
        {
            mMetrics = new MetricsBuilder()
                //.Report.ToConsole(TimeSpan.FromSeconds(1))
                .Report.ToInfluxDb("http://127.0.0.1:8086", "metrics-test", TimeSpan.FromSeconds(1))
                .Build();

            mReportingService = new MetricsReporterBackgroundService(mMetrics, new MetricsOptions(), mMetrics.Reporters);
            mReportingService.StartAsync(CancellationToken.None).Wait();
        }
        
        public void Flush()
        {
            // Task.WaitAll(mMetrics.ReportRunner.RunAllAsync(CancellationToken.None).ToArray());
            mReportingService.StopAsync(CancellationToken.None).Wait();
        }

        public void ReportValue(string name, int value, dynamic meta)
        {
            MetricTags tags = ExtractTags(meta);
            
            var options = new GaugeOptions { Name = name, Tags = tags, MeasurementUnit = Unit.Custom("ms") };
            mMetrics.Measure.Gauge.SetValue(options, value);
        }

        public void ReportTime(string name, Action action, dynamic meta)
        {
            MetricTags tags = ExtractTags(meta);

            var timeOpt = new TimerOptions {
                Name = name,
                Tags = tags
            };

            mMetrics.Measure.Timer.Time(timeOpt, action);
        }

        private MetricTags ExtractTags(dynamic meta) {
            var tags = MetricTags.Empty;
            
            foreach (PropertyInfo propertyInfo in meta.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                tags = MetricTags.Concat(tags, new MetricTags(propertyInfo.Name, $"{propertyInfo.GetValue(meta, null)}"));
            }

            return tags;
        }
    }
}