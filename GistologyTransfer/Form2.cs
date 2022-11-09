using GistologyTransfer.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GistologyTransfer
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
            textBox1.Text = Properties.Settings.Default.Folder;
            dateTimePicker1.Value = Properties.Settings.Default.DateFrom;
            dateTimePicker2.Value = Properties.Settings.Default.DateTo;
            textBox2.Text = Properties.Settings.Default.ConnString;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {

                    textBox1.Text = fbd.SelectedPath;

                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.Folder = textBox1.Text;
            Properties.Settings.Default.DateFrom = this.dateTimePicker1.Value;
            Properties.Settings.Default.DateTo = this.dateTimePicker2.Value;
            Properties.Settings.Default.ConnString = textBox2.Text;
            Properties.Settings.Default.Save();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            
            if (this.dateTimePicker1.Value > this.dateTimePicker2.Value)
            {
                MessageBox.Show("Дата начала не может быть больше даты окончания");
                dateTimePicker1.Value = Properties.Settings.Default.DateFrom;
            }
        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.DialogResult == DialogResult.Cancel || this.DialogResult == DialogResult.Abort)
            {
                this.DialogResult = DialogResult.OK;
            }
        }
    }
}
