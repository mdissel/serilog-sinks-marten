using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Serilog.Debugging;
using Serilog.Events;
using Serilog.Formatting.Json;
using Serilog.Sinks.PeriodicBatching;

using m = Marten;

namespace Serilog.Sinks.Marten
{
    /// <summary>
    ///     Writes log events as rows in a table of MSSqlServer database.
    /// </summary>
    public class MartenSink : PeriodicBatchingSink
    {
        private readonly int _BatchPostingLimit;
        /// <summary>
        ///     A reasonable default for the number of events posted in
        ///     each batch.
        /// </summary>
        public const int DefaultBatchPostingLimit = 1000;

        /// <summary>
        ///     A reasonable default time to wait between checking for event batches.
        /// </summary>
        public static readonly TimeSpan DefaultPeriod = TimeSpan.FromSeconds(5);

        private readonly IFormatProvider _formatProvider;
        private readonly m.IDocumentStore _DocumentStore;
        private readonly bool _RenderMessage;
        private readonly bool _ExceptionAsString;


        /// <summary>
        ///     Construct a sink posting to the specified database.
        /// </summary>
        /// <param name="documentStore">The document store</param>
        /// <param name="batchPostingLimit">The maximum number of events to post in a single batch.</param>
        /// <param name="period">The time to wait between checking for event batches.</param>
        /// <param name="formatProvider">Supplies culture-specific formatting information, or null.</param>
        /// <param name="autoCreateSqlTable">Create log table with the provided name on destination sql server.</param>
        /// <param name="columnOptions">Options that pertain to columns</param>
        public MartenSink(
            m.IDocumentStore documentStore,
            int batchPostingLimit,
            TimeSpan period,
            bool exceptionAsString,
            IFormatProvider formatProvider
            )
            : base(batchPostingLimit, period)
        {
            _DocumentStore = documentStore ?? throw new ArgumentNullException("documentStore");
            _formatProvider = formatProvider;
            _BatchPostingLimit = batchPostingLimit;
            _ExceptionAsString = exceptionAsString;
        }

        /// <summary>
        ///     Emit a batch of log events
        /// </summary>
        /// <param name="events">The events to emit.</param>
        protected override void EmitBatch(IEnumerable<LogEvent> events)
        {
            if (events == null)
                return;
            IList<LogMessage> renderedEvents = new List<LogMessage>();
            foreach (LogEvent @event in events)
            {
                LogMessage logMessage = new LogMessage(@event);
                try
                {
                    logMessage.Message = @event.RenderMessage(_formatProvider);
                }
                catch (Exception ex)
                {
                    logMessage.Message = "Failed to render Serilog message from template: " + ex.Message;
                }
                if (_ExceptionAsString && @event.Exception != null)
                {
                    logMessage.ExceptionMessage = @event.Exception.ToString();
                }
                renderedEvents.Add(logMessage);
            }
            try
            {
                _DocumentStore.BulkInsert<LogMessage>(renderedEvents.ToArray(), m.BulkInsertMode.InsertsOnly, _BatchPostingLimit);
            }
            catch (Exception ex)
            {
                SelfLog.WriteLine("Unable to write {0} log events to Marten due to following error: {1}", events.Count(), ex.Message);
            }
        }

    }
}