using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using static GistologyTransfer.Program;

namespace GistologyTransfer.TreeView
{
    public class Icd10Nodes
    {
        public string value { get; set; }
        public bool isChecked { get; set; }
        public IList<ChildNode> children { get; set; }

        public static void ChildNodes(TreeNode parent, IList<ChildNode> children)
        {
            foreach (var item in children)
            {
                TreeNode node = new TreeNode(item.value);
                node.Checked = item.isChecked;
                parent.Nodes.Add(node);
                if (item.children != null)
                    ChildNodes(node, item.children);
            }
        }

        public static void GetNodesRecursive(TreeNode oParentNode)
        {
            // Start recursion on all subnodes.
            foreach (TreeNode oSubNode in oParentNode.Nodes)
            {
                if (oSubNode.Checked)
                {
                    Globals.IcdValues.Add(oSubNode.Text);
                }
                GetNodesRecursive(oSubNode);
            }
        }
    }
    public class ChildNode
    {
        public string value { get; set; }
        public bool isChecked { get; set; }
        public IList<ChildNode> children { get; set; }

    }

}
