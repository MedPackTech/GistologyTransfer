using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GistologyTransfer
{
    public class Icd10
    {
        public int? Id { get; set; }
        public string Title { get; set; }
        public int? ParentId { get; set; }
        public bool Checked { get; set; }

        public static explicit operator TreeNode(Icd10 e) { return new TreeNode(e.Title); }
    }

    public static class Icd10Extension
    {
        public static void Add(this Dictionary<int, List<Icd10>> Icd10s, int Id, string Title, int ParentId)
        {
            if (!Icd10s.ContainsKey(ParentId)) Icd10s[ParentId] = new List<Icd10>();

            Icd10s[ParentId].Add(new Icd10() { Id = Id, Title = Title, ParentId = ParentId });
        }
    }

}
