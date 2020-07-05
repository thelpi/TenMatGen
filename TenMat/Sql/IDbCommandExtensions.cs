using System;
using System.Data;

namespace TenMat.Sql
{
    /// <summary>
    /// Extension methods for the <see cref="IDbCommand"/> type.
    /// </summary>
    public static class IDbCommandExtensions
    {
        /// <summary>
        /// Adds a datetime parameter to the command.
        /// </summary>
        /// <param name="command">Instance of <see cref="IDbCommand"/>.</param>
        /// <param name="parameterName">The parameter name.</param>
        /// <param name="date">The <see cref="DateTime"/> value.</param>
        /// <exception cref="ArgumentNullException"><paramref name="command"/> is <c>Null</c>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="parameterName"/> is <c>Null</c>, empty or white spaces only.</exception>
        public static void AddDateTimeParameter(this IDbCommand command, string parameterName, DateTime date)
        {
            command.AddParameter(parameterName, date, DbType.DateTime);
        }

        /// <summary>
        /// Adds a parameter to the command.
        /// </summary>
        /// <param name="command">Instance of <see cref="IDbCommand"/>.</param>
        /// <param name="parameterName">The parameter name.</param>
        /// <param name="value">The value.</param>
        /// <param name="type">The parameter <see cref="DbType"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="command"/> is <c>Null</c>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="parameterName"/> is <c>Null</c>, empty or white spaces only.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is <c>Null</c>.</exception>
        public static void AddParameter(this IDbCommand command, string parameterName, object value, DbType type)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            if (string.IsNullOrWhiteSpace(parameterName))
            {
                throw new ArgumentNullException(nameof(parameterName));
            }

            IDbDataParameter parameter = command.CreateParameter();
            parameter.ParameterName = parameterName;
            parameter.DbType = type;
            parameter.Value = value ?? throw new ArgumentNullException(nameof(value));
            command.Parameters.Add(parameter);
        }
    }
}
