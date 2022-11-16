using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GistologyTransfer.DbModels
{
    public class UnimCase
    {
        public string IdIssled { get; set; }
        public string ExternalId { get; set; }
        public string YearIssled { get; set; }
        public string Macro { get; set; }
        public string Micro { get; set; }
        public List<Seria> Series { get; set; }
    }

    public class Seria
    {
        public string IdSeria { get; set; }
        public string PrepNumber { get; set; }
        public string Icd10 { get; set; }
        public string Diagnosis { get; set; }
        public List<File> Files { get; set; }
        public string Icd0 { get; set; }
    }

    public class File
    {
        public string FileReq { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string Scanner { get; set; }
        public string Resolution { get; set; }
        public string Focus { get; set; }
        public string Color { get; set; }
    }
}
