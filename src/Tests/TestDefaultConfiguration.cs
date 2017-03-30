using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

using Serilog;
using Serilog.Sinks;
using Serilog.Sinks.Marten;
using m = Marten;

namespace Tests
{
    public class TestDefaultConfiguration : IDisposable
    {
        m.IDocumentStore documentStore;
        public TestDefaultConfiguration()
        {
            documentStore = m.DocumentStore.For(_ =>
            {
                _.Connection(ConnectionSource.ConnectionString);
                _.MappingForSerilog();
                _.AutoCreateSchemaObjects = m.AutoCreate.CreateOrUpdate;
            });
            documentStore.Advanced.Clean.DeleteDocumentsFor(typeof(LogMessage));
            Log.Logger = new LoggerConfiguration().WriteTo.Marten(
                documentStore, false,
                period: TimeSpan.FromSeconds(1)
                )
                .CreateLogger();

            //Log.Logger = new LoggerConfiguration()
            //    .WriteTo.Console(new Serilog.Formatting.Json.JsonFormatter())
            //    .CreateLogger();
        }

        public void Dispose()
        {
            documentStore.Dispose();
        }

        [Fact]
        private void SimpleMessage()
        {
            Log.Information("Test {count} and {2}", 1, 2);

            Serilog.Formatting.Json.JsonFormatter format = new Serilog.Formatting.Json.JsonFormatter();

            Thread.Sleep(TimeSpan.FromSeconds(1));
            using (m.IQuerySession session = documentStore.QuerySession())
            {
                LogMessage logMessage = session.Query<LogMessage>().FirstOrDefault();
                Assert.Equal("Test {count}", logMessage.MessageTemplate);
                Assert.Equal(Serilog.Events.LogEventLevel.Information, logMessage.Level);
                Assert.Equal(DateTime.Today, logMessage.Timestamp.Date);
                Assert.Equal(1, logMessage.Properties.Count);
                Assert.Equal("Test 1", logMessage.Message);
            }
        }
    }
}
