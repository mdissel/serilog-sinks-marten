# serilog-sinks-marten
A Serilog sink that writes events to Postgresql using Marten

Register in the documentstore or create your own mapping for the type `LogMessage`
```csharp
documentStore = m.DocumentStore.For(_ =>
{
  ..
  _.MappingForSerilog();
  ..
});
```

Create the sink for serilog
```csharp
Log.Logger = new LoggerConfiguration()
  .WriteTo.Marten(
    documentStore, 
    false
   )
  .CreateLogger();
```

Done!
