using GistologyTransfer.DbManagers;
using GistologyTransfer.SystemClasses;
using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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

        //Кнопка настроек
        private void button1_Click(object sender, EventArgs e)
        {
            Form2 childForm = new Form2();            
            
            this.Enabled = false;

            if (childForm.ShowDialog(this) == DialogResult.OK)
            {
                this.Enabled = true;
            }

        }

        //Кнопка выгрузки
        private async void button3_Click(object sender, EventArgs e)
        {
            int fileprogress = 0;
            
            //Просматриваем рекурсивно весь архив изображений и помещаем в массив объектов
            List<FileArray> Resp = new List<FileArray>();
            label1.Text = "Сканируем архив изображений";
            Resp = DirSearch(Properties.Settings.Default.ArchivFolder, Resp);
            string cs = "";
            try
            {
                cs = await Encryptor.AES_DecryptAsync(Properties.Settings.Default.ConnString);
            }
            catch (Exception connex)
            {
                MessageBox.Show("Ошибка расшифровки строки подключения: " + connex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                label1.Text = "";
                return;
            }
            //Обращаемся в юним и ищем случаи за нужный период
            PgDbManager pg = new PgDbManager(cs);
            try
            {
                label1.Text = "Ищем случаи в DP  " + Properties.Settings.Default.DateFrom.ToString("dd.MM.yyyy") + "-" + Properties.Settings.Default.DateTo.ToString("dd.MM.yyyy");
                var lst = await pg.GetCasesAsync();

                //Считаем количество файлов к выгрузке для вывода в окно и прогресс бара
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

                    //Пробуем инициализировать и создать эксельку
                    Excel.Application myexcelApplication = null;
                    Excel.Workbook myexcelWorkbook = null;
                    Excel.Worksheet myexcelWorksheet = null;
                    try
                    {
                        myexcelApplication = new Excel.Application();
                        myexcelWorkbook = myexcelApplication.Workbooks.Add();
                        myexcelWorksheet = (Excel.Worksheet)myexcelWorkbook.Sheets.Add();
                    }
                    catch (Exception xlex)
                    {
                        MessageBox.Show("Ошибка создания Excel-файла: " + xlex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        label1.Text = "";
                        return;
                    }
                    //Активируем прогресс бар
                    progressBar1.Visible = true;
                    progressBar1.Minimum = 1;
                    progressBar1.Maximum = set;
                    progressBar1.Value = 1;
                    progressBar1.Step = 1;

                    DirectoryInfo di = new DirectoryInfo(Properties.Settings.Default.Folder);

                    string path = Properties.Settings.Default.Folder;
                    //Пробуем создать директорию выгрузки
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

                    myexcelWorksheet.Cells[1, 1] = "Номер исследования (случая)";
                    myexcelWorksheet.Cells[1, 2] = "Серия препаратов";
                    myexcelWorksheet.Cells[1, 3] = "Файлы";
                    myexcelWorksheet.Cells[1, 4] = "Слайдов в серии";
                    myexcelWorksheet.Cells[1, 5] = "Год";
                    myexcelWorksheet.Cells[1, 6] = "МКБ-10";
                    myexcelWorksheet.Cells[1, 7] = "МКБ-0-3";
                    myexcelWorksheet.Cells[1, 8] = "Сканер";
                    myexcelWorksheet.Cells[1, 9] = "Разрешение сканирования";
                    myexcelWorksheet.Cells[1, 10] = "Фокус";
                    myexcelWorksheet.Cells[1, 11] = "Гистологический диагноз";
                    myexcelWorksheet.Cells[1, 12] = "Дополнительный код";
                    myexcelWorksheet.Cells[1, 13] = "Макроскопическое описание";
                    myexcelWorksheet.Cells[1, 14] = "Микроскопическое описание";

                    int r = 1;

                    DirectoryInfo rp = new DirectoryInfo(Properties.Settings.Default.Folder);

                    foreach (var item in lst)
                    {
                        DateTime starttime = DateTime.Now;

                        
                        r = r + 1;

                        int cr = r;

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
                            MessageBox.Show("Ошибка создания директории: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            label1.Text = "";
                            return;
                        }
                        int spcount = 0;
                        foreach (var ser in item.Series)
                        {
                            int pcount = 0;

                            r = r + 1;

                            int pr = r;

                            //Счетчик изображений в серии. Считает по фактически найденным.
                            
                            foreach (var file in ser.Files)
                            {
                                fileprogress = fileprogress + 1;
                                Regex reg = new Regex(@".*" + file.FileReq + @".*.svs");
                                int ind = Resp.FindIndex(s => reg.Match(s.fullpath).Success);
                                if (ind != -1)
                                {
                                    spcount = spcount + 1;
                                    pcount = pcount + 1;
                                    file.FilePath = Resp[ind].fullpath;
                                    file.FileName = Resp[ind].filename;
                                    if (!File.Exists(rp.FullName.ToString() + @"\" + Path.GetFileName(file.FilePath)))
                                    {
                                      await FileCopy.CopyFileAsync(file.FilePath, rp.FullName.ToString() + @"\" + Path.GetFileName(file.FilePath));
                                        //  File.Copy(file.FilePath, rp.FullName.ToString() + @"\" + Path.GetFileName(file.FilePath));
                                    }

                                    r = r + 1;

                                    myexcelWorksheet.Cells[r, 3] = file.FileName;
                                    myexcelWorksheet.Cells[r, 8] = file.Scanner;
                                    myexcelWorksheet.Cells[r, 9] = file.Resolution;
                                    myexcelWorksheet.Cells[r, 10] = file.Focus;
                                    myexcelWorksheet.Cells[r, 12] = file.Color;

                                }

                                progressBar1.PerformStep();

                            }
                            if (pcount > 0)
                            {
                                myexcelWorksheet.Cells[cr, 1] = item.ExternalId;
                                myexcelWorksheet.Cells[cr, 5] = item.YearIssled;
                                myexcelWorksheet.Cells[cr, 13] = item.Macro;
                                myexcelWorksheet.Cells[cr, 14] = item.Micro;

                                myexcelWorksheet.Cells[pr, 2] = ser.IdSeria;
                                myexcelWorksheet.Cells[pr, 6] = ser.Icd10;
                                myexcelWorksheet.Cells[pr, 11] = ser.Diagnosis;
                                myexcelWorksheet.Cells[pr, 7] = ser.Icd0;
                                myexcelWorksheet.Cells[pr, 4] = pcount.ToString();
                            }
                            else
                            {
                                r = r - 1;
                            }

                            TimeSpan timespent = DateTime.Now - starttime;
                            int secondsremaining = (int)(timespent.TotalSeconds / progressBar1.Value * (progressBar1.Maximum - progressBar1.Value));

                            TimeSpan time = TimeSpan.FromSeconds(secondsremaining);
                            string str = time.ToString(@"d\ hh\:mm\:ss");

                            label2.Text = "Оставшееся время: " + str;
                            label1.Text = "Выгружаем изображения: " + fileprogress.ToString() + "/" +  set.ToString();
                        }
                        if (spcount == 0)
                        {
                            r = r - 1;
                        }
                    }

                    myexcelApplication.ActiveWorkbook.SaveAs(di.FullName + "\\" + DateTime.Now.ToString("yyyyMMdd") + ".xls", Excel.XlFileFormat.xlWorkbookNormal);
                    myexcelWorkbook.Close();
                    myexcelApplication.Quit();

                    progressBar1.Value = set;

                    MessageBox.Show("Выгрузка Завершена", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    label1.Text = "";
                }
                else
                {
                    MessageBox.Show("Нет случаев за указанные даты", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                label1.Text = "";
                return;
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
