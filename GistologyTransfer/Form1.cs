using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GistologyTransfer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form2 childForm = new Form2();
            //childForm.MdiParent = this;
            
            
            this.Enabled = false;

            if (childForm.ShowDialog(this) == DialogResult.OK)
            {
                this.Enabled = true;
            }

            
            //this.Hide();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            progressBar1.Visible = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
