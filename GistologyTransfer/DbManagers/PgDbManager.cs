using GistologyTransfer.DbModels;
using GistologyTransfer.DbProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GistologyTransfer.DbManagers
{
    internal class PgDbManager
    {
        public PgDbManager(string connectionString)
        {
            _connectionStr = connectionString;
        }

        private string _connectionStr;

        public Task<List<UnimCase>> GetCasesAsync()
        {
            return Task.Factory.StartNew(() => { return GetCases(); });
        }

        public List<UnimCase> GetCases()
        {
            PgSystem _pg = new PgSystem(_connectionStr);
            return _pg.GetCases();
        }

    }
}
