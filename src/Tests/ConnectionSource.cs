using System;
using Baseline;

namespace Tests
{
    public class ConnectionSource
    {
        //"User ID=postgres;Password=postgres;Host=localhost;Port=5432;Database=martentesting;"
        public static readonly string ConnectionString = Environment.GetEnvironmentVariable("marten-testing-database");

    }
}