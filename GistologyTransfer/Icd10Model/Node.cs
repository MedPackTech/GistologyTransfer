using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace GistologyTransfer
{
    /// <summary>
    /// Общий класс представления и работы с нодами TreeView
    /// </summary>
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class Node
    {
        public Node()
        {
            this.children = new List<Node>();
        }

        public Node(string _value, List<Node> _children = null, bool _isChecked = false)
        {
            Value = _value;
            isChecked = _isChecked;
            if (_children != null)
            {
                children = _children;
            }
        }
        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("isChecked")]
        public bool isChecked { get; set; }

        [JsonProperty("children", NullValueHandling = NullValueHandling.Ignore)]
        public List<Node> children { get; set; }

        [JsonIgnore]
        public string JSon
        {
            get
            {
                return JsonConvert.SerializeObject(this);
            }
        }

        public static List<Node> RunNode(TreeNode node)
        {
            List<Node> nodeOut = new List<Node>();
            foreach (TreeNode child in node.Nodes)
            {
                List<Node> grandchild = RunNode(child);
                nodeOut.Add(new Node(child.Text, grandchild, child.Checked));
            }
            return nodeOut;
        }
    }
}
