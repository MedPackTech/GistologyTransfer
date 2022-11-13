using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using System.Security.Cryptography;
using System.Data.Common;
using GistologyTransfer.DbModels;
using Npgsql.Internal.TypeHandlers;
using System.Runtime.ConstrainedExecution;

namespace GistologyTransfer.DbProviders
{
    public class PgSystem : DbProviderBase
    {
        public PgSystem(string connectionString) : base(connectionString) { }

        public List<UnimCase> GetCases()
        {
            string request = @"Select cast(c.id as varchar(36)) id, c.external_label, c.title,  c.creation_date,  
                                cast(extract(year from c.creation_date) as varchar(4)) as cyear,
                                r.data::jsonb->>'icd10' as icd10,
                                r.data::jsonb->>'diagnosis' as diagnosis,
                                f.title,
                                cast(count(*) over (partition by c.id,substr(f.title,position('-' in f.title)+1,1)) as varchar(10)) as prepnumber,
                                substr(f.title,position('-' in f.title)+1,1) as morder,
                                'abcabc' as model,
                                20 as vision,
                                1 as focus,
                                r.micro_description_protocol_text,
                                c.macro_description_protocol_text
                                From cases as c
                                Inner Join reports as r on c.id = r.case_id
                                Inner join files as f on f.case_id = c.id and f.type = 'snapshot' and f.title not like '%S%'
                                Where 
                                c.status = 'validated'
                                and c.creation_date between @Bdate::TIMESTAMP and @Fdate::TIMESTAMP
                                ORDER BY id, morder, f.title";

            List<UnimCase> res = new List<UnimCase>();

            using (NpgsqlConnection Odbc = new NpgsqlConnection(_connection))
            {
                using (NpgsqlCommand exec = new NpgsqlCommand(request, Odbc))
                {

                    exec.Parameters.Add(new NpgsqlParameter()
                    {
                        ParameterName = "Bdate",
                        DbType = DbType.String,
                        Value = Properties.Settings.Default.DateFrom.ToString("yyyyMMdd")
                    });
                    exec.Parameters.Add(new NpgsqlParameter()
                    {
                        ParameterName = "Fdate",
                        DbType = DbType.String,
                        Value = Properties.Settings.Default.DateTo.ToString("yyyyMMdd")
                    });

                    Odbc.Open();
                    if (Odbc.State == ConnectionState.Open)
                    {
                        using (NpgsqlDataReader rd = exec.ExecuteReader())
                        {
                            if (rd != null && rd.HasRows)
                            {
                                
                                string cId = "";
                                string cSid = "";
                                UnimCase cc = null;
                                Seria ser = null;
                                while (rd.Read())
                                {
                                    string currCaseId = rd.IsDBNull(0) ? "" : rd.GetString(0).Trim();
                                    if (currCaseId != cId)
                                    {
                                        if (cc != null)
                                        {
                                            res.Add(cc);
                                        }

                                        cc = new UnimCase();
                                        cId = currCaseId;

                                        cc.IdIssled = rd.IsDBNull(0) ? String.Empty : rd.GetString(0).Trim();
                                        cc.ExternalId = rd.IsDBNull(1) ? String.Empty : rd.GetString(1).Trim();
                                        cc.YearIssled = rd.IsDBNull(4) ? String.Empty : rd.GetString(4).Trim();
                                        cc.Macro = rd.IsDBNull(14) ? String.Empty : rd.GetString(14).Trim();
                                        cc.Micro = rd.IsDBNull(13) ? String.Empty : rd.GetString(13).Trim();
                                        cc.Series = new List<Seria>();
                                    }
                                    string currSeriaId = rd.IsDBNull(7) ? "" : rd.GetString(7).Trim();
                                    if (currSeriaId != cSid)
                                    {
                                        cSid = currSeriaId;

                                        ser = new Seria();
                                        ser.IdSeria = rd.IsDBNull(7) ? "" : rd.GetString(7).Trim();
                                        ser.PrepNumber = rd.IsDBNull(9) ? "" : rd.GetString(9).Trim();
                                        ser.Icd10 = rd.IsDBNull(5) ? "" : rd.GetString(5).Trim();
                                        ser.Diagnosis = rd.IsDBNull(6) ? "" : rd.GetString(6).Trim();
                                        ser.Files = new List<File>();

                                        cc.Series.Add(ser);
                                    }

                                    File f = new File();
                                    f.FileReq = rd.IsDBNull(7) ? "" : rd.GetString(7).Trim();
                                    f.Scanner = "Scanner";
                                    f.Resolution = "20";
                                    f.Focus = "1";
                                    ser.Files.Add(f);

                                }
                                if (cc != null)
                                {
                                    res.Add(cc);
                                }
                            }
                        }

                    }
                }
            }
            return res;
        }

    }
}