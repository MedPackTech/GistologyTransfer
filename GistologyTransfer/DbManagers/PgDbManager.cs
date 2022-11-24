using GistologyTransfer.DbModels;
using GistologyTransfer.DbProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GistologyTransfer.DbManagers
{
    /// <summary>
    /// Общий класс подключения к PostgreSQL
    /// </summary>
    internal class PgDbManager
    {
        public PgDbManager(string connectionString)
        {
            _connectionStr = connectionString;
        }

        private string _connectionStr;

        /// <summary>
        /// Асинхронный вызов метода запроса к БД. Получение случаев.
        /// </summary>
        /// <returns></returns>
        public Task<List<UnimCase>> GetCasesAsync()
        {
            return Task.Factory.StartNew(() => { return GetCases(); });
        }

        /// <summary>
        /// Вход в основной запрос
        /// </summary>
        /// <returns></returns>
        public List<UnimCase> GetCases()
        {
            PgSystem _pg = new PgSystem(_connectionStr);
            return _pg.GetCases();
        }

    }
}
