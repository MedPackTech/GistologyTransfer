using GistologyTransfer.DbManagers;
using GistologyTransfer.DbModels;
using GistologyTransfer.SystemClasses;
using GistologyTransfer.TreeView;
using Microsoft.Office.Interop.Excel;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Forms;
using static GistologyTransfer.Program;
using Excel = Microsoft.Office.Interop.Excel;

namespace GistologyTransfer
{
    public partial class Form1 : Form
    {
        int set = 0;

        static DateTime starttime = DateTime.Now;

        TimeSpan timespent = DateTime.Now - starttime;

        public Form1()
        {
            InitializeComponent();
            
            //backgroundWorker1.DoWork += backgroundWorker1_DoWork;
            backgroundWorker1.ProgressChanged += backgroundWorker1_ProgressChanged;
            backgroundWorker1.RunWorkerCompleted += backgroundWorker1_RunWorkerCompleted;
            backgroundWorker1.WorkerReportsProgress = true;
            backgroundWorker1.WorkerSupportsCancellation = true;
        }

        /// <summary>
        /// Настройки программы. Открытие дочерней формы.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            Form2 childForm = new Form2();            
            
            this.Enabled = false;

            if (childForm.ShowDialog(this) == DialogResult.OK)
            {
                this.Enabled = true;
            }

        }

        /// <summary>
        /// Основной механизм. Выгрузка исследований по кнопке в форме.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {

            if (!Directory.Exists(Properties.Settings.Default.ArchivFolder))
            {
                MessageBox.Show("Файловый архив недоступен, проверьте правильность указанного пути", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!Directory.Exists(Properties.Settings.Default.Folder))
            {
                MessageBox.Show("Путь выгрузки недоступен, проверьте правильность указанного пути", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string cs = "";

            try
            {
                cs = Encryptor.AES_Decrypt(Properties.Settings.Default.ConnString);
            }
            catch (Exception connex)
            {
                MessageBox.Show("Ошибка расшифровки строки подключения: " + connex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            label1.Text = "Ищем случаи в DP  " + Properties.Settings.Default.DateFrom.ToString("dd.MM.yyyy") + "-" + Properties.Settings.Default.DateTo.ToString("dd.MM.yyyy");

            List<UnimCase> lst = null;

            PgDbManager pg = new PgDbManager(cs);
            try
            {
                lst = pg.GetCases();

                //Считаем количество файлов к выгрузке для вывода в окно и прогресс бара
                if (lst.Count > 0)
                {
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
                }
                else
                {
                    MessageBox.Show("Нет случаев за указанные даты", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    label1.Text = "";
                    return;
                }
            }
            catch (Exception dbex)
            {
                MessageBox.Show("Ошибка подключения к БД: " + dbex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                label1.Text = "";
                return;
            }

            button4.Enabled = true;
            button4.Visible = true;
            button4.Text = "Стоп";
            button3.Visible = false;
            button3.Enabled = false;
            button1.Enabled = false;

            if (!pictureBox1.Visible)
            {
                button1.Text = "Should try KONAMI code";
            }
            
            //Активируем прогресс бар
            progressBar1.Visible = true;
            progressBar1.Minimum = 0;
            progressBar1.Maximum = set;
            progressBar1.Value = 0;
            progressBar1.Step = 1;

            //Запускаем обход файлов в фоновой задаче
            backgroundWorker1.RunWorkerAsync(argument: lst);
            
        }

        /// <summary>
        /// Закрытие
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Файловая рекурсия. Первоначально полностью сканируем выбранную директорию архива и сохраняем её структуру.
        /// Получаем список List FileArray, к которому в последующем обращаемся при сопоставлении случаев БД и файлов
        /// Чтобы не нагружать дисковую подсистему
        /// </summary>
        /// <param name="sDir"></param>
        /// <param name="resp"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Пасхалка
        /// </summary>
        public class KonamiSequence
        {
            private static readonly Keys[] KonamiCode = { Keys.Up, Keys.Up, Keys.Down, Keys.Down, Keys.Left, Keys.Right, Keys.Left, Keys.Right, Keys.B, Keys.A };

            private readonly Queue<Keys> _inputKeys = new Queue<Keys>();

            public bool IsCompletedBy(Keys inputKey)
            {
                _inputKeys.Enqueue(inputKey);

                while (_inputKeys.Count > KonamiCode.Length)
                    _inputKeys.Dequeue();

                return _inputKeys.SequenceEqual(KonamiCode);
            }
        }

        private readonly KonamiSequence _konamiSequence = new KonamiSequence();

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            if (_konamiSequence.IsCompletedBy(e.KeyCode))
                pictureBox1.Visible = true;
        }

        /// <summary>
        /// Системное прерывание. Обновляет глобальную переменную _isRunning
        /// При входе в новый цикл обработки случая процесс поиска завершается
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button4_Click(object sender, EventArgs e)
        {
            button4.Enabled = false;
            button4.Text = "Останавливаем...";

            if (backgroundWorker1.IsBusy)
            {
                backgroundWorker1.CancelAsync();
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            List<UnimCase> lst = (List<UnimCase>) e.Argument;

            int fileprogress = 0;

            List<FileArray> Resp = new List<FileArray>();

            Resp = DirSearch(Properties.Settings.Default.ArchivFolder, Resp);
            
            try
            {
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
                    return;
                }

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

                starttime = DateTime.Now;

                foreach (var item in lst)
                {
                    if (backgroundWorker1.CancellationPending)
                    {
                        e.Cancel = true;
                        backgroundWorker1.ReportProgress(0);
                        return;
                    }

                    r = r + 1;

                    int cr = r;

                    try
                    {
                        if (Directory.Exists(di.FullName) && !Properties.Settings.Default.ReportCheck)
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
                            if (backgroundWorker1.CancellationPending)
                            {
                                e.Cancel = true;
                                backgroundWorker1.ReportProgress(0);
                                return;
                            }

                            fileprogress = fileprogress + 1;
                            Regex reg = new Regex(@".*" + file.FileReq + @".*" + Properties.Settings.Default.ImgType);
                            int ind = Resp.FindIndex(s => reg.Match(s.fullpath).Success);
                            if (ind != -1)
                            {
                                spcount = spcount + 1;
                                pcount = pcount + 1;
                                file.FilePath = Resp[ind].fullpath;
                                file.FileName = Resp[ind].filename;
                                if (!System.IO.File.Exists(rp.FullName.ToString() + @"\" + Path.GetFileName(file.FilePath)) && !Properties.Settings.Default.ReportCheck)
                                {
                                    try
                                    {
                                        FileCopy.CopyFile(file.FilePath, rp.FullName.ToString() + @"\" + Path.GetFileName(file.FilePath));
                                    }
                                    catch (Exception fex)
                                    {
                                        Console.WriteLine(fex.Message);
                                    }
                                }

                                r = r + 1;

                                myexcelWorksheet.Cells[r, 3] = file.FileName;
                                myexcelWorksheet.Cells[r, 8] = file.Scanner;
                                myexcelWorksheet.Cells[r, 9] = file.Resolution;
                                myexcelWorksheet.Cells[r, 10] = file.Focus;
                                myexcelWorksheet.Cells[r, 12] = file.Color;

                            }

                            timespent = DateTime.Now - starttime;

                            backgroundWorker1.ReportProgress(fileprogress);
                                
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
                    }
                    if (spcount == 0 && !Properties.Settings.Default.ReportCheck)
                    {
                        r = r - 1;
                    }
                }
                try
                {
                    myexcelApplication.ActiveWorkbook.SaveAs(di.FullName + "\\" + DateTime.Now.ToString("yyyyMMdd") + ".xls", Excel.XlFileFormat.xlWorkbookNormal);
                    myexcelWorkbook.Close();
                    myexcelApplication.Quit();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Ошибка записи Excel-файла", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                backgroundWorker1.ReportProgress(set);
               
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;

            label1.Text = "Выгружаем изображения: " + e.ProgressPercentage.ToString() + @"/" + set.ToString();

            
            int secondsremaining = (int)(timespent.TotalSeconds / progressBar1.Value * (progressBar1.Maximum - progressBar1.Value));

            TimeSpan time = TimeSpan.FromSeconds(secondsremaining);
            string str = time.ToString(@"dd\ hh\:mm\:ss");

            label2.Visible = true;
            label2.Text = "Оставшееся время: " + str;
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                label1.Text = "Отменено";
            }
            else if (e.Error != null)
            {
                label1.Text = "Ошибка";
            }
            else
            {
                label1.Text = "Завершено";
                MessageBox.Show("Выгрузка Завершена", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            set = 0;

            button4.Enabled = false;
            button4.Visible = false;
            button3.Visible = true;
            button3.Enabled = true;
            button1.Enabled = true;
            button1.Text = "Настройка выгрузки";
            label2.Text = "";
        }
    }
}
