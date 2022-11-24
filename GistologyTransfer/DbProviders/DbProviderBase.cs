using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace GistologyTransfer.DbProviders
{
    /// <summary>
    /// Универсальная база коннекторов к различным СУБД
    /// </summary>
    public class DbProviderBase
    {
        /// <summary>
        /// Строка подключения
        /// </summary>
        protected string _connection;

        public DbProviderBase(string ConnectionString)
        {
            if (String.IsNullOrWhiteSpace(ConnectionString))
            {
                throw new Exception("Attempt create Db class with empty string.");
            }
            else
            {
                _connection = ConnectionString;
            }
        }

        /// <summary>
        /// Закрытие ридера
        /// </summary>
        /// <param name="reader"></param>
        protected void EnsureReaderClose(IDataReader reader)
        {
            if (reader != null)
            {
                reader.Close();
            }
        }

        /// <summary>
        /// Уничтожение команды к СУБД
        /// </summary>
        /// <param name="command"></param>
        protected void EnsureCommandDispose(IDbCommand command)
        {
            if (command != null)
            {
                command.Dispose();
            }
        }

        /// <summary>
        /// Очистка соединения с СУБД
        /// </summary>
        /// <param name="connection"></param>
        protected void EnsureConnectionDispose(IDbConnection connection)
        {
            if (connection != null)
            {
                connection.Dispose();
            }
        }
    }
}
