using System;

namespace Metrics.Reporters
{
    interface IReporter {
        void ReportValue(string name, int value, dynamic meta);

        void ReportTime(string name, Action action, dynamic meta);

        void Flush();
    }
}