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
        private readonly m.IDocumentStore documentStore;
        public TestDefaultConfiguration()
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
                .CreateLogger();
        }

        public void Dispose()
        {
            documentStore.Dispose();
        }

        [Fact]
        private void SimpleMessage()
        {
            Log.Information("SimpleMessage {count} and {2}", 1, 2);
            Log.CloseAndFlush();

            using (m.IQuerySession session = documentStore.QuerySession())
            {
                LogMessage logMessage = session.Query<LogMessage>().FirstOrDefault(x => x.MessageTemplate.StartsWith("SimpleMessage "));
                Assert.Equal("SimpleMessage {count} and {2}", logMessage.MessageTemplate);
                Assert.Equal(Serilog.Events.LogEventLevel.Information, logMessage.Level);
                Assert.Equal(DateTime.Today, logMessage.Timestamp.Date);
                Assert.Equal(2, logMessage.Properties.Count);
                Assert.Equal("SimpleMessage 1 and 2", logMessage.Message);
            }
        }


        [Fact]
        private void LogMultipleMessage()
        {
            for (int i = 0; i < 50; i++)
            {
                Log.Information("LogMultipleMessage {i}", i);
            }
            Log.CloseAndFlush();

            using (m.IQuerySession session = documentStore.QuerySession())
            {
                LogMessage[] logMessages = session.Query<LogMessage>().Where(x => x.MessageTemplate.StartsWith("LogMultipleMessage ")).ToArray();
                Assert.Equal(50, logMessages.Length);
            }
        }

        [Fact]
        public void MessageWithDynamicClass()
        {
            var sensorInput = new { Latitude = 25, Longitude = 134 };
            Log.Information("MessageWithDynamicClass {sensorInput}", sensorInput);
            Log.CloseAndFlush();

            using (m.IQuerySession session = documentStore.QuerySession())
            {
                LogMessage logMessage = session.Query<LogMessage>().FirstOrDefault(x => x.MessageTemplate.StartsWith("MessageWithDynamicClass "));
                Assert.Equal("MessageWithDynamicClass {sensorInput}", logMessage.MessageTemplate);
                Assert.Equal(Serilog.Events.LogEventLevel.Information, logMessage.Level);
                Assert.Equal(DateTime.Today, logMessage.Timestamp.Date);
                Assert.Equal(1, logMessage.Properties.Count);
                Assert.Equal("MessageWithDynamicClass \"{ Latitude = 25, Longitude = 134 }\"", logMessage.Message);
            }
        }

        [Fact]
        public void MessageWithException()
        {
            ArgumentNullException ex = new ArgumentNullException("paramName");
            Log.Error(ex, "MessageWithException {name}", "Name");
            Log.CloseAndFlush();

            using (m.IQuerySession session = documentStore.QuerySession())
            {
                LogMessage logMessage = session.Query<LogMessage>().FirstOrDefault(x => x.MessageTemplate.StartsWith("MessageWithException "));
                Assert.Equal("MessageWithException {name}", logMessage.MessageTemplate);
                Assert.Equal(Serilog.Events.LogEventLevel.Error, logMessage.Level);
                Assert.Equal(DateTime.Today, logMessage.Timestamp.Date);
                Assert.NotNull(logMessage.Exception);
                Assert.Equal(1, logMessage.Properties.Count);
                Assert.Equal("MessageWithException \"Name\"", logMessage.Message);
            }
        }

    }
}
