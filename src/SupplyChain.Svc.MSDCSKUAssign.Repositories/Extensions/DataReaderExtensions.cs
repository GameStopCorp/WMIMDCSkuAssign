using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace SupplyChain.Svc.MSDCSKUAssign.Repositories.Extensions
{
    public static class DataReaderExtensions
    {
        #region [ Public Methods (8) ]

        /// <summary>
        /// Checks to see if the column exists in the reader.
        /// </summary>
        public static bool ContainsColumn(this IDataReader r, string columnName)
        {
            var schemaTable = r.GetSchemaTable();

            if (schemaTable == null) return false;

            schemaTable.DefaultView.RowFilter = "ColumnName= '" + columnName + "'";

            return schemaTable.DefaultView.Count > 0;
        }

        public static bool HasColumn(this IDataReader dataReader, string columnName)
        {
            for (var i = 0; i < dataReader.FieldCount; i++)
            {
                if (dataReader.GetName(i).Equals(columnName, StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }


        /// <summary>
        /// Strongly typed get to the column's data.
        /// </summary>
        public static bool Get<T>(this IDataReader r, string name, out T val)
        {
            return Get(r, r.GetOrdinal(name), out val);
        }

        /// <summary>
        /// Strongly typed get to the column's data.
        /// </summary>
        public static bool Get<T>(IDataReader r, string name, out T? val)
            where T : struct
        {
            return Get<T>(r, r.GetOrdinal(name), out val);
        }

        /// <summary>
        /// Strongly typed get to the column's data.
        /// </summary>
        public static bool Get<T>(this IDataReader r, int ordinal, out T val)
        {
            if (r.IsDBNull(ordinal))
            {
                val = default(T);
                return false;
            }

            if (typeof(T).IsEnum)
            {
                string v = r.GetValue(ordinal).ToString();
                val = (T)Enum.Parse(typeof(T), v);
            }
            else
            {
                val = (T)r.GetValue(ordinal);
            }

            return true;
        }

        /// <summary>
        /// Strongly typed get to the column's data.
        /// </summary>
        public static bool Get<T>(this IDataReader r, int ordinal, out T? val)
            where T : struct
        {
            if (r.IsDBNull(ordinal))
            {
                val = null;
                return false;
            }

            val = (T)r.GetValue(ordinal);

            return true;
        }


        public static T Get<T>(this IDataReader r, string name, T defaultValue)
        {
            return Get(r, r.GetOrdinal(name), defaultValue);
        }

        public static T Get<T>(this IDataReader r, int ordinal, T defaultValue)
        {
            T value;
            return Get<T>(r, ordinal, out value) ? value : defaultValue;
        }

        /// <summary>
        /// Gets a byte array from the specified column.
        /// </summary>
        public static byte[] GetBytes(this IDataReader r, string name)
        {
            int ord = r.GetOrdinal(name);

            if (r.IsDBNull(ord))
            {
                return null;
            }

            long size = r.GetBytes(ord, 0, null, 0, 0);
            var data = new byte[size];
            int blockSize = 8192;

            for (long ix = 0; ix < size;)
            {
                ix += r.GetBytes(ord, ix, data, (int)ix, blockSize);
            }

            return data;
        }

        /// <summary>
        /// Strongly typed get of a named column.
        /// </summary>
        public static T Get<T>(this IDataReader r, string name)
        {
            T val = default(T);

            Get(r, r.GetOrdinal(name), out val);

            return val;
        }

        /// <summary>
        /// Gets the specified r.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="K"></typeparam>
        /// <param name="r">The r.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public static T Get<T, K>(this IDataReader r, string name)
        {
            int ordinal = r.GetOrdinal(name);

            if (r.IsDBNull(ordinal))
            {
                return default(T);
            }
            if (typeof(K).IsEnum)
            {
                string v = r.GetValue(ordinal).ToString();
                return (T)Enum.Parse(typeof(K), v);
            }
            else
            {
                return (T)r.GetValue(ordinal);
            }
        }

        #endregion
    }
}
