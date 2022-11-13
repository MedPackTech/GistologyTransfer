using GistologyTransfer.DbManagers;
using GistologyTransfer.SystemClasses;
using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Excel = Microsoft.Office.Interop.Excel;

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
            
            this.Enabled = false;

            if (childForm.ShowDialog(this) == DialogResult.OK)
            {
                this.Enabled = true;
            }

        }

        private async void button3_Click(object sender, EventArgs e)
        {
        

            List<FileArray> Resp = new List<FileArray>();
            label1.Text = "Сканируем архив изображений";
            Resp = DirSearch(Properties.Settings.Default.ArchivFolder, Resp);

            PgDbManager pg = new PgDbManager(await Encryptor.AES_DecryptAsync(Properties.Settings.Default.ConnString));
            try
            {
                label1.Text = "Ищем случаи в DP  " + Properties.Settings.Default.DateFrom.ToString("dd.MM.yyyy") + "-" + Properties.Settings.Default.DateTo.ToString("dd.MM.yyyy");
                var lst = await pg.GetCasesAsync();

                if (lst.Count > 0)
                {
                    int set = 0;

                    foreach (var item in lst)
                    {
                        foreach (var ser in item.Series)
                        {
                            foreach (var file in ser.Files)
                            {
                                set = set + 1;
                            }
                        }
                    }

                    label1.Text = "Выгружаем изображения: " + set.ToString();

                    Excel.Application myexcelApplication = new Excel.Application();
                    Excel.Workbook myexcelWorkbook = myexcelApplication.Workbooks.Add();
                    Excel.Worksheet myexcelWorksheet = (Excel.Worksheet)myexcelWorkbook.Sheets.Add();

                    progressBar1.Visible = true;
                    progressBar1.Minimum = 1;
                    progressBar1.Maximum = set;
                    progressBar1.Value = 1;
                    progressBar1.Step = 1;

                    DirectoryInfo di = new DirectoryInfo(Properties.Settings.Default.Folder);

                    string path = Properties.Settings.Default.Folder;

                    if (Directory.Exists(path))
                    {
                        if (!Directory.Exists(path + @"\" + Properties.Settings.Default.DateFrom.ToString("yyyyMMdd") + "_" + Properties.Settings.Default.DateTo.ToString("yyyyMMdd")))
                        {
                            di = Directory.CreateDirectory(path + @"\" + Properties.Settings.Default.DateFrom.ToString("yyyyMMdd") + "_" + Properties.Settings.Default.DateTo.ToString("yyyyMMdd"));
                        }
                        else
                        {
                            di = new DirectoryInfo(path + @"\" + Properties.Settings.Default.DateFrom.ToString("yyyyMMdd") + "_" + Properties.Settings.Default.DateTo.ToString("yyyyMMdd"));
                        }
                    }



                    int r = 0;

                    DirectoryInfo rp = new DirectoryInfo(Properties.Settings.Default.Folder);

                    foreach (var item in lst)
                    {
                        r = r + 1;

                        myexcelWorksheet.Cells[r, 1] = item.ExternalId;
                        myexcelWorksheet.Cells[r, 5] = item.YearIssled;
                        myexcelWorksheet.Cells[r, 13] = item.Macro;
                        myexcelWorksheet.Cells[r, 14] = item.Micro;



                        try
                        {
                            if (Directory.Exists(di.FullName))
                            {
                                if (!Directory.Exists(di.FullName + @"\" + item.ExternalId))
                                {
                                    rp = Directory.CreateDirectory(di.FullName + @"\" + item.ExternalId);
                                }
                                else
                                {
                                    rp = new DirectoryInfo(di.FullName + @"\" + item.ExternalId);
                                }
                            }
                        }
                        catch (Exception ex)
                        {

                        }

                        foreach (var ser in item.Series)
                        {
                            r = r + 1;

                            myexcelWorksheet.Cells[r, 2] = ser.IdSeria;
                            myexcelWorksheet.Cells[r, 4] = ser.PrepNumber;
                            myexcelWorksheet.Cells[r, 6] = ser.Icd10;
                            myexcelWorksheet.Cells[r, 11] = ser.Diagnosis;

                            foreach (var file in ser.Files)
                            {

                                Regex reg = new Regex(@".*" + file.FileReq + @".*.txt");

                                int ind = Resp.FindIndex(s => reg.Match(s.fullpath).Success);
                                if (ind != -1)
                                {
                                    file.FilePath = Resp[ind].fullpath;
                                    file.FileName = Resp[ind].filename;
                                    if (!File.Exists(rp.FullName.ToString() + @"\" + Path.GetFileName(file.FilePath)))
                                    {
                                        File.Copy(file.FilePath, rp.FullName.ToString() + @"\" + Path.GetFileName(file.FilePath));
                                    }

                                    r = r + 1;

                                    myexcelWorksheet.Cells[r, 3] = file.FileName;
                                    myexcelWorksheet.Cells[r, 8] = file.Scanner;
                                    myexcelWorksheet.Cells[r, 9] = file.Resolution;
                                    myexcelWorksheet.Cells[r, 10] = file.Focus;


                                    progressBar1.PerformStep();

                                }

                            }
                        }
                    }

                    myexcelApplication.ActiveWorkbook.SaveAs(di.FullName + "\\" + DateTime.Now.ToString("yyyyMMdd") + ".xls", Excel.XlFileFormat.xlWorkbookNormal);
                    myexcelWorkbook.Close();
                    myexcelApplication.Quit();

                    progressBar1.Value = set;

                    MessageBox.Show("Выгрузка Завершена", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    label1.Text = "";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                label1.Text = "";
                //throw;
            }
            

            


        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        static List<FileArray> DirSearch(string sDir, List<FileArray> resp)
        {

            try
            {
                foreach (string d in Directory.GetDirectories(sDir))
                {
                    foreach (string f in Directory.GetFiles(d))
                    {
                        FileArray file = new FileArray();

                        file.filename = Path.GetFileName(f); 
                        file.fullpath = f;
                        resp.Add(file);
                    }
                    DirSearch(d, resp);
                }
            }
            catch (System.Exception excpt)
            {
                Console.WriteLine(excpt.Message);
            }
            return resp;
        }
    }
}
