using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Npgsql;
using GistologyTransfer.DbModels;
using NpgsqlTypes;

namespace GistologyTransfer.DbProviders
{
    /// <summary>
    /// Класс коннектора к PostgreSQL. Наследование от DbProviderBase
    /// </summary>
    public class PgSystem : DbProviderBase
    {
        public PgSystem(string connectionString) : base(connectionString) { }

        /// <summary>
        /// Основной запрос данных от UNIM для выгрузки файлов и формирования отчета
        /// Запрос возвращает линейные данные, с дальнейщей оберткой под класс UNIM-case
        /// Возвращает список случаев List UnimCase
        /// </summary>
        /// <returns></returns>
        public List<UnimCase> GetCases()
        {
            string request = @"WITH a
                                AS (
	                                SELECT cast(c.id as varchar(36)) id
		                                ,c.external_label
		                                ,c.title
		                                ,c.creation_date
		                                ,cast(extract(year FROM c.creation_date) AS varchar(4)) AS cyear
		                                ,f.title AS ftitle
                                        ,r.data::jsonb ->> 'patientGender' as gender
                                        ,r.data::jsonb ->> 'patientBirthday' as birthday
                                        ,r.data::jsonb ->> 'icdO' as icdO
		                                ,r.data::jsonb #> '{materials,slides}' AS s
		                                ,r.data::jsonb ->> 'icd10' AS icd10
		                                ,r.data::jsonb ->> 'pathologicalReport' AS diagnosis
		                                ,substring(f.title,1,length(f.title) - position('-' IN reverse(f.title))) as seria
		                                ,reverse(substring(reverse(f.title),1, position('-' IN reverse(f.title)) - 1)) AS prep
		                                ,cast('Leica AT2' as varchar(9)) AS model
		                                ,cast('20' as varchar(2)) AS vision
		                                ,cast('' as varchar(1)) AS focus
		                                ,r.micro_description_protocol_text
		                                ,c.macro_description_protocol_text
	                                FROM cases AS c
	                                INNER JOIN reports AS r ON c.id = r.case_id
	                                INNER JOIN files AS f ON f.case_id = c.id
		                                AND f.type = 'snapshot'
		                                AND f.title NOT LIKE '%S%'
	                                WHERE c.STATUS = 'validated'
		                                AND r.validation_ended_date BETWEEN @Bdate::date 
											AND @Fdate::date + 1 - interval '1 sec'
                                                AND replace(r.data::jsonb ->> 'icd10',' ','') = ANY(@Icd10)
	                                )
                                SELECT a.id
	                                ,a.external_label
	                                ,a.title
	                                ,a.creation_date
	                                ,a.cyear
	                                ,replace(a.icd10,' ','') as icd10
	                                ,a.diagnosis as diagnosis
	                                ,a.ftitle
	                                ,a.seria
	                                ,a.prep
	                                ,a.model
	                                ,a.vision
	                                ,a.focus
	                                ,a.micro_description_protocol_text
	                                ,a.macro_description_protocol_text
	                                ,val::jsonb ->> 'stain' AS stain
                                    ,a.icdO
                                    ,CASE WHEN lower(a.gender)='m' THEN 'М' ELSE 'Ж' END as gender
                                    ,CAST(EXTRACT(YEAR FROM AGE(a.creation_date,a.birthday::date)) as VARCHAR(3)) as age
                                FROM a
                                JOIN lateral jsonb_array_elements(a.s) obj(val) ON obj.val ->> 'unimCode' = a.ftitle
                                ORDER BY id, seria, prep";

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
                    exec.Parameters.Add(new NpgsqlParameter()
                    {
                        ParameterName = "Icd10",
                        NpgsqlDbType = NpgsqlDbType.Array | NpgsqlDbType.Varchar,
                        Value = Properties.Settings.Default.Icd10Arr.Cast<string>().ToList() //Globals.IcdValues
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
                                        cc.ExternalId = rd.IsDBNull(2) ? String.Empty : rd.GetString(2).Trim();
                                        cc.YearIssled = rd.IsDBNull(4) ? String.Empty : rd.GetString(4).Trim();
                                        cc.Macro = rd.IsDBNull(14) ? String.Empty : rd.GetString(14).Trim();
                                        cc.Micro = rd.IsDBNull(13) ? String.Empty : rd.GetString(13).Trim();
                                        cc.Diagnosis = rd.IsDBNull(6) ? "" : rd.GetString(6).Trim();
                                        cc.Series = new List<Seria>();
                                        cc.Gender = rd.IsDBNull(17) ? "" : rd.GetString(17).Trim();
                                        cc.Age = rd.IsDBNull(18) ? "" : rd.GetString(18).Trim();
                                    }
                                    string currSeriaId = rd.IsDBNull(8) ? "" : rd.GetString(8).Trim();
                                    if (currSeriaId != cSid)
                                    {
                                        cSid = currSeriaId;

                                        ser = new Seria();
                                        ser.IdSeria = rd.IsDBNull(8) ? "" : rd.GetString(8).Trim();
                                        ser.PrepNumber = rd.IsDBNull(9) ? "" : rd.GetString(9).Trim();
                                        ser.Icd10 = rd.IsDBNull(5) ? "" : rd.GetString(5).Trim();
                                        ser.Files = new List<File>();
                                        ser.Icd0 = rd.IsDBNull(16) ? "" : rd.GetString(16).Trim();

                                        cc.Series.Add(ser);
                                    }

                                    File f = new File();
                                    f.FileReq = rd.IsDBNull(7) ? "" : rd.GetString(7).Trim();
                                    f.Scanner = rd.IsDBNull(10) ? "" : rd.GetString(10).Trim();
                                    f.Resolution = rd.IsDBNull(11) ? "" : rd.GetString(11).Trim();
                                    f.Focus = rd.IsDBNull(12) ? "" : rd.GetString(12).Trim();
                                    f.Color = rd.IsDBNull(15) ? "" : rd.GetString(15).Trim();
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