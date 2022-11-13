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
using GistologyTransfer;
using System.Globalization;

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
            textBox3.Text = Properties.Settings.Default.ArchivFolder;
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

        private async void button2_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.Folder = textBox1.Text;
            Properties.Settings.Default.DateFrom = this.dateTimePicker1.Value;
            Properties.Settings.Default.DateTo = this.dateTimePicker2.Value;
            if (!Properties.Settings.Default.ConnString.ToLower().StartsWith("server="))
            {
                Properties.Settings.Default.ConnString = textBox2.Text;
            }
            else
            {
                Properties.Settings.Default.ConnString = await Encryptor.AES_EcnryptAsync(textBox2.Text);
            }
            Properties.Settings.Default.ArchivFolder = textBox3.Text;
            Properties.Settings.Default.Save();
            MessageBox.Show("Настройки сохранены", "Информация",MessageBoxButtons.OK,MessageBoxIcon.Information);
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
                MessageBox.Show("Дата начала не может быть больше даты окончания", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

        private void dateTimePicker2_ValueChanged(object sender, EventArgs e)
        {
            if (this.dateTimePicker2.Value < this.dateTimePicker1.Value)
            {
                MessageBox.Show("Дата окончания не может быть меньше даты начала", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                dateTimePicker2.Value = Properties.Settings.Default.DateFrom;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {

                    textBox3.Text = fbd.SelectedPath;

                }
            }
        }
    }
}
