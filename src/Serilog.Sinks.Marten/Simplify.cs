using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Serilog.Events;

namespace Serilog.Sinks.Marten
{
    /// <summary>
    ///     Converts <see cref="LogEventProperty" /> values into simple scalars,
    ///     dictionaries and lists so that they can be persisted in Marten.
    /// </summary>
    public static class SimplifyPropertyFormatter
    {
        /// <summary>
        ///     Simplify the object so as to make handling the serialized
        ///     representation easier.
        /// </summary>
        /// <param name="value">The value to simplify (possibly null).</param>
        /// <param name="options">Options to use during formatting</param>
        /// <returns>A simplified representation.</returns>
        public static object Simplify(LogEventPropertyValue value)
        {
            var scalar = value as ScalarValue;
            if (scalar != null)
                return scalar.Value;

            var dict = value as DictionaryValue;
            if (dict != null)
            {
                IDictionary<object, object> dictionary = new Dictionary<object, object>();
                foreach (var element in dict.Elements)
                {
                    if (element.Key == null || element.Key.Value == null)
                        continue;
                    var itemValue = Simplify(element.Value);
                    if (itemValue == null)
                        continue;
                    dictionary[element.Key.Value] = itemValue;

                }
                return dictionary;
            }

            var seq = value as SequenceValue;
            if (seq != null)
            {
                IList<object> list = new List<object>();
                foreach (var element in seq.Elements)
                {
                    var itemValue = Simplify(element);
                    if (itemValue == null)
                        continue;
                    list.Add(itemValue);
                }
                return list;
            }

            var str = value as StructureValue;
            if (str != null)
            {
                return str.Properties.ToDictionary(p => p.Name, p => Simplify(p.Value));
            }
            return null;
        }


        //private static readonly IDictionary<Type, Action<string, object, IDictionary<string, object>>> LiteralWriters = new Dictionary
        //    <Type, Action<string, object, IDictionary<string, object>>>
        //{
        //    {typeof (SequenceValue), (k, v, p) => WriteSequenceValue(k, (SequenceValue) v, p)},
        //    {typeof (DictionaryValue), (k, v, p) => WriteDictionaryValue(k, (DictionaryValue) v, p)},
        //    {typeof (StructureValue), (k, v, p) => WriteStructureValue(k, (StructureValue) v, p)},
        //    {typeof (ScalarValue), (k, v, p) => WriteValue(k,((ScalarValue)v).Value,p)}
        //};

        //private static void WriteStructureValue(string key, StructureValue structureValue, IDictionary<string, object> properties)
        //{
        //    foreach (var eventProperty in structureValue.Properties)
        //    {
        //        WriteValue(key + "." + eventProperty.Name, eventProperty.Value, properties);
        //    }
        //}

        //private static void WriteDictionaryValue(string key, DictionaryValue dictionaryValue, IDictionary<string, object> properties)
        //{
        //    foreach (var eventProperty in dictionaryValue.Elements)
        //    {
        //        WriteValue(key + "." + eventProperty.Key.Value, eventProperty.Value, properties);
        //    }
        //}

        //private static void WriteSequenceValue(string key, SequenceValue sequenceValue, IDictionary<string, object> properties)
        //{
        //    int index = 0;
        //    foreach (var eventProperty in sequenceValue.Elements)
        //    {
        //        WriteValue(key + "." + index, eventProperty, properties);
        //        index++;
        //    }
        //    AppendProperty(properties, key + ".Count", index.ToString());
        //}

        //public static void WriteValue(string key, object value, IDictionary<string, object> properties)
        //{
        //    Action<string, object, IDictionary<string, object>> writer;
        //    if (value == null || !LiteralWriters.TryGetValue(value.GetType(), out writer))
        //    {
        //        AppendProperty(properties, key, value?.ToString());
        //        return;
        //    }
        //    writer(key, value, properties);
        //}

        //private static void AppendProperty(IDictionary<string, object> propDictionary, string key, string value)
        //{
        //    if (propDictionary.ContainsKey(key))
        //    {
        //        //SelfLog.WriteLine("The key {0} is not unique after simplification. Ignoring new value {1}", key, value);
        //        return;
        //    }
        //    propDictionary.Add(key, value);
        //}

    }
}