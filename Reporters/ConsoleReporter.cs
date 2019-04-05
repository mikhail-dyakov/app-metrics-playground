using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace Metrics.Reporters
{
    class ConsoleReporter : IReporter {
        public void Flush()
        {            
        }

        public void ReportValue(string name, int value, dynamic meta) {
            var metaString = GetMetaString(meta);
            Console.WriteLine($"\t{name}:\t{value};\t[{metaString}]");
        }

        public void ReportTime(string name, Action action, dynamic meta)
        {
            var sw = Stopwatch.StartNew();

            action();

            sw.Stop();

            var metaString = GetMetaString(meta);
            Console.WriteLine($"\t{name}:\t{sw.ElapsedMilliseconds} ms;\t[{metaString}]");                        
        }

        private string GetMetaString(dynamic meta) {
            IEnumerable<(string key, object value)> metaData = ExtractMetaData(meta);
            
            string metaString = "";
            foreach(var pair in metaData) {
                metaString += $"{pair.key}:{pair.value};";
            }

            return metaString;
        }

        private IEnumerable<(string key, object value)> ExtractMetaData(dynamic meta) {
            if(meta == null)
                yield break;

            foreach (PropertyInfo propertyInfo in meta.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                yield return (key: propertyInfo.Name, value: propertyInfo.GetValue(meta, null));
            }
        }
    }
}