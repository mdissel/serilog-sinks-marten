using Serilog.Events;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Serilog.Formatting.Json;

namespace Serilog.Sinks.Marten
{
    public class LogMessage
    {

        public LogMessage(LogEvent logEvent)
        {
            if (logEvent == null)
                return;
            Exception = logEvent.Exception;
            Level = logEvent.Level;
            Timestamp = logEvent.Timestamp;
            if (logEvent.MessageTemplate != null)
                MessageTemplate = logEvent.MessageTemplate.Text;
            Exception = logEvent.Exception;
            if (logEvent.Properties != null)
            {
                var formatter = new JsonValueFormatter();
                Properties = new Dictionary<string, object>();
                foreach (KeyValuePair<string, LogEventPropertyValue> item in logEvent.Properties)
                {
                    Properties[item.Key] = SimplifyPropertyFormatter.Simplify(item.Value);
                }
            }
        }

        public Guid ID { get; set; }

        //
        // Summary:
        //     An exception associated with the event, or null.
        public Exception Exception { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string ExceptionMessage { get; set; }

        //
        // Summary:
        //     The level of the event.
        public LogEventLevel Level { get; set; }
        //
        // Summary:
        //     The rendered message.
        public string Message { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string MessageTemplate { get; set; }
        //
        // Summary:
        //     Properties associated with the event, including those presented in Serilog.Events.LogEvent.MessageTemplate.
        public IDictionary<string, object> Properties { get; set; }
        //
        // Summary:
        //     The time at which the event occurred.
        public DateTimeOffset Timestamp { get; set; }

    }


}