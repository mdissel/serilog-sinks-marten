using System;
using Serilog.Configuration;
using Serilog.Events;
using Serilog.Sinks.Marten;
using m = Marten;
using Serilog.Core;

namespace Serilog
{
    public static class MartenSinkExtensions
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="loggerConfiguration"></param>
        /// <param name="documentStore">a reference to the marten documentstore</param>
        /// <param name="renderMessage">if enabled dont store the messagetemplate, but the rendered message expanding all variables</param>
        /// <param name="exceptionAsString">if enabled render the exception as a string using <code>exception.ToString()</code></param>
        /// <param name="restrictedToMinimumLevel"></param>
        /// <param name="batchPostingLimit">defaults to 1000, marten default for batching</param>
        /// <param name="period">defaults to 5 seconds</param>
        /// <param name="formatProvider"></param>
        /// <returns></returns>
        public static LoggerConfiguration Marten(
            this LoggerSinkConfiguration loggerConfiguration,
            m.IDocumentStore documentStore,
            bool exceptionAsString,
            LogEventLevel restrictedToMinimumLevel = LogEventLevel.Verbose,
            LoggingLevelSwitch levelSwitch = null,
            int batchPostingLimit = MartenSink.DefaultBatchPostingLimit,
            TimeSpan? period = null,
            IFormatProvider formatProvider = null)
        {
            if (loggerConfiguration == null) throw new ArgumentNullException("loggerConfiguration");
            period = period ?? MartenSink.DefaultPeriod;

            return loggerConfiguration.Sink(
                new MartenSink(
                    documentStore,
                    batchPostingLimit,
                    period.Value,
                    exceptionAsString,
                    formatProvider
                    ),
                restrictedToMinimumLevel,
                levelSwitch);
        }
    }
}