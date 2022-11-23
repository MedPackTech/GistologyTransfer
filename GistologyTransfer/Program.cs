using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GistologyTransfer
{
    internal static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }

        public static class Globals
        {
            //public static List<Icd10> dg = LoadJson(AppDomain.CurrentDomain.BaseDirectory + @"Icd10.json");
            public static List<string> IcdValues = new List<string>();
        }


        //public static List<Icd10> LoadJson(string file)
        //{

        //    using (StreamReader r = new StreamReader(file))
        //    {
        //        string json = r.ReadToEnd();
        //        List<Icd10> items = JsonConvert.DeserializeObject<List<Icd10>>(json);

        //        return items;
        //    }

        //}
    }
}
