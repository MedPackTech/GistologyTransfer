using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GistologyTransfer
{
    /// <summary>
    /// Расширение первоначального класса для МКБ-10. Построение дерева нод для TreeView
    /// В текущей версии не применяется
    /// </summary>
    public static class NodeRoot
    {
        public static void PopulateTreeView(Dictionary<int?, List<Icd10>> Icd10, int? ParentId, TreeNodeCollection nodes)
        {
            if (!Icd10.ContainsKey(ParentId)) return;

            foreach (Icd10 e in Icd10[ParentId])
            {
                TreeNode tn = (TreeNode)e;
                nodes.Add(tn);
                tn.Checked = e.Checked;
                PopulateTreeView(Icd10, e.Id, tn.Nodes);
            }
        }

    }
}
