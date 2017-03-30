using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using m = Marten;

namespace Serilog.Sinks.Marten
{
    public static class MartenDocumentStoreOptionsExtentions
    {

        /// <summary>
        /// Register the <see cref="LogMessage"/> type and add a Gin index to the data column
        /// </summary>
        /// <param name="storeOptions"></param>
        public static void MappingForSerilog(this m.StoreOptions storeOptions)
        {
            storeOptions.MappingFor(typeof(LogMessage)).AddGinIndexToData();
        }

    }
}
