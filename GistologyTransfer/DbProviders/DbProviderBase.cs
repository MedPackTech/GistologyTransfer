using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace GistologyTransfer.DbProviders
{
    public class DbProviderBase
    {
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

        protected void EnsureReaderClose(IDataReader reader)
        {
            if (reader != null)
            {
                reader.Close();
            }
        }

        protected void EnsureCommandDispose(IDbCommand command)
        {
            if (command != null)
            {
                command.Dispose();
            }
        }

        protected void EnsureConnectionDispose(IDbConnection connection)
        {
            if (connection != null)
            {
                connection.Dispose();
            }
        }
    }
}
