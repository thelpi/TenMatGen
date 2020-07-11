using System;
using System.Data;

namespace TenMat.Sql
{
    /// <summary>
    /// Extention methods for the <see cref="IDataReader"/> type.
    /// </summary>
    public static class IDataReaderExtensions
    {
        /// <summary>
        /// Gets the value from a column name, parsed into the targeted type.
        /// </summary>
        /// <typeparam name="T">The targeted type.</typeparam>
        /// <param name="reader">Instance of <see cref="IDataReader"/>.</param>
        /// <param name="columnName">The column name.</param>
        /// <returns>The value; <c>default</c> of <typeparamref name="T"/> if the source value is <see cref="DBNull.Value"/> or if the cast into <typeparamref name="T"/> fails.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="reader"/> is <c>Null</c>.</exception>
        /// <exception cref="ArgumentException">The <paramref name="reader"/> is closed.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="columnName"/> is <c>Null</c>, empty or white spaces only.</exception>
        /// <exception cref="ArgumentException"><paramref name="columnName"/> is not in <paramref name="reader"/>.</exception>
        public static T Get<T>(this IDataReader reader, string columnName)
        {
            if (reader.IsDBNull(columnName))
            {
                return default(T);
            }

            try
            {
                Type t = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);

                object valueToParse = reader[columnName];

                return (T)(t.IsEnum ? Enum.Parse(typeof(T), valueToParse.ToString())
                    : Convert.ChangeType(valueToParse, t));
            }
            catch
            {
                return default(T);
            }
        }

        /// <summary>
        /// Checks if the data reader contains the specified column name.
        /// The comparison is case insensitive.
        /// </summary>
        /// <param name="reader">Instance of <see cref="IDataReader"/>.</param>
        /// <param name="columnName">The column name.</param>
        /// <returns><c>True</c> if the data reader contains the column name; otherwise <c>False</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="reader"/> is <c>Null</c>.</exception>
        /// <exception cref="ArgumentException">The <paramref name="reader"/> is closed.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="columnName"/> is <c>Null</c>, empty or white spaces only.</exception>
        public static bool HasColumn(this IDataReader reader, string columnName)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            if (reader.IsClosed)
            {
                throw new ArgumentException("The data reader is closed.", nameof(reader));
            }

            if (string.IsNullOrWhiteSpace(columnName))
            {
                throw new ArgumentNullException(nameof(columnName));
            }

            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals(columnName, StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if the specified column is <see cref="DBNull.Value"/> in the data reader.
        /// </summary>
        /// <param name="reader">Instance of <see cref="IDataReader"/>.</param>
        /// <param name="columnName">The column name.</param>
        /// <returns><c>True</c> if the column is <see cref="DBNull.Value"/>; <c>False</c> otherwise.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="reader"/> is <c>Null</c>.</exception>
        /// <exception cref="ArgumentException">The <paramref name="reader"/> is closed.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="columnName"/> is <c>Null</c>, empty or white spaces only.</exception>
        /// <exception cref="ArgumentException"><paramref name="columnName"/> is not in <paramref name="reader"/>.</exception>
        public static bool IsDBNull(this IDataReader reader, string columnName)
        {
            if (!reader.HasColumn(columnName))
            {
                throw new ArgumentException("The specified column name is not in the data reader.", nameof(columnName));
            }

            return reader.IsDBNull(reader.GetOrdinal(columnName));
        }
    }
}
