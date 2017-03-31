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
    public class TestWithEnricher : IDisposable
    {
        private readonly m.IDocumentStore documentStore;
        public TestWithEnricher()
        {
            documentStore = m.DocumentStore.For(_ =>
            {
                _.Connection(ConnectionSource.ConnectionString);
                _.MappingForSerilog();
                _.AutoCreateSchemaObjects = m.AutoCreate.CreateOrUpdate;
            });
            documentStore.Advanced.Clean.CompletelyRemove(typeof(LogMessage));
            Log.Logger = new LoggerConfiguration().WriteTo.Marten(
                documentStore, 
                false,
                period: TimeSpan.FromSeconds(1)
                )
                .Enrich.WithProperty("Enrich", "Test")
                .CreateLogger();
        }

        public void Dispose()
        {
            documentStore.Dispose();
        }

        [Fact]
        private void LogMessage()
        {
            Log.Information("Test {count} and {2}", 1, 2);
            Log.CloseAndFlush();

            using (m.IQuerySession session = documentStore.QuerySession())
            {
                LogMessage logMessage = session.Query<LogMessage>().FirstOrDefault();
                Assert.Equal("Test {count} and {2}", logMessage.MessageTemplate);
                Assert.Equal(Serilog.Events.LogEventLevel.Information, logMessage.Level);
                Assert.Equal(DateTime.Today, logMessage.Timestamp.Date);
                Assert.Equal(3, logMessage.Properties.Count);
                Assert.True(logMessage.Properties.ContainsKey("Enrich"));
                Assert.Equal("Test 1 and 2", logMessage.Message);
            }
        }


    }
}
