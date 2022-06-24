using System.Text;
using FileParser.Models;
using FileParser.Extension;
using Serilog;
using Gehtsoft.PDFFlow.Builder;
using Gehtsoft.PDFFlow.Models.Enumerations;

namespace FileParser
{
    public partial class Form1 : Form
    {
        private StringBuilder Header { get; set; }
        private StringBuilder Content { get; set; }
        private int Count { get; set; }
        private string Language { get; set; }

        private bool StopLoop { get; set; } 

        private Serilog.Core.Logger Loop { get; set; }
        //private Serilog.Core.Logger Error { get; set; }


        enum HeaderOrContent
        {
            Header,
            Content
        }

        HeaderOrContent? Choose { get; set; }


        public Form1()
        {
            Loop = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.File("loop.txt")
                .CreateLogger();

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.File("aceds_notice.txt", rollingInterval: RollingInterval.Minute)
                .CreateLogger();

            Header = new StringBuilder();
            Content = new StringBuilder();
            Language = "ENG";
            StopLoop = true;

            //IronPdf.License.LicenseKey = "IRONPDF.DCDEPARTMENTOFHUMANSERVICES.IRO211108.2920.32138.801112-D7ACDBA004-CJUPTNQSKXFY7-4MFZDZEGQFK5-7QPQBKN6632I-WTMZLEEMTGNZ-PR4Z7DGQCWGB-ZSJMV6-LV442AGBAJOIEA-PROFESSIONAL.1YR-6XTTHV.RENEW.SUPPORT.08.NOV.2022";

            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fDlg = new();
            if (fDlg.ShowDialog() == DialogResult.OK)
            {
                string fSelectedFolder = fDlg.SelectedPath;

                textBox2.Text = fSelectedFolder;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog oDlg = new();
            if (DialogResult.OK == oDlg.ShowDialog())
            {
                string oSelectedFile = oDlg.FileName;

                textBox1.Text = oSelectedFile;
            }
        }

        private async void button3_Click(object sender, EventArgs e)
        {
            try
            {
                Header = new StringBuilder();
                Content = new StringBuilder();
                Choose = null;
                Count = 0;
                textBox3.Clear();
                StopLoop = false;

                // leave if file doesn't exists or a file has not been selected.
                if (string.IsNullOrWhiteSpace(textBox1.Text) || !File.Exists(textBox1.Text) || string.IsNullOrWhiteSpace(textBox2.Text) || !Directory.Exists(textBox2.Text))
                    return;

                textBox3.AppendText("Starting the process");

                Thread.Sleep(3000);

                textBox3.Clear();

                using StreamReader sr = new(textBox1.Text);

                string? line;

                FileParse? fileParse = new();

                // Iterating the file
                while (sr.Peek() >= 0 && StopLoop == false)
                {
                    await Task.Run(() =>
                    {
                        try
                        {
                            line = sr.ReadLine();

                            if (line != null && line.StartsWith("1Page"))
                            {
                                fileParse = CreateParseFile();

                                if (fileParse != null) ProcessParseFile(fileParse);

                                Header = new StringBuilder();
                                Content = new StringBuilder();
                            }
                            else
                            {
                                if (line != null && line.StartsWith("0CASE NAME:"))
                                {
                                    Choose = HeaderOrContent.Header;
                                }

                                if (line != null && line.StartsWith("0 "))
                                {
                                    Choose = HeaderOrContent.Content;
                                }

                                if (Choose == HeaderOrContent.Header)
                                {
                                    Header.Append(line);
                                }

                                if (Choose == HeaderOrContent.Content)
                                {
                                    Content.AppendLine(line);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                            Log.Error($"{ex.Message} --- {ex.InnerException} --- {ex.StackTrace}");
                        }
                    } );
                }

                if (StopLoop == false)
                {
                    fileParse = CreateParseFile();

                    if (fileParse != null) ProcessParseFile(fileParse);
                }
                else
                {
                    textBox3.AppendText("Process has been stopped");
                    return;
                }

                textBox3.AppendText("End of file process");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Log.Error(ex.Message);
            }
            
        }

        private FileParse? CreateParseFile()
        {
            if (Choose == null) return null;

            FileParse fileParse = new()
            {
                CaseName = Header.GetStringInBetweenSection("0CASE NAME:", "CASE NUMBER:")?.Trim() ?? "NO_CASE_NAME",
                CaseNumber = Header.GetStringInBetweenSection("CASE NUMBER:", "PROG:")?.Trim() ?? "NO_CASE_NUMBER",
                Program = Header.GetStringInBetweenSection("PROG:", "BEN MONTH:")?.Trim() ?? "NO_PROG",
                Month = Header.GetStringInBetweenSection("BEN MONTH:", "NOTICE-NUMBER:")?.Trim() ?? "NO_MONTH",
                NoticeNum = Header.GetStringInBetweenSection("NOTICE-NUMBER:", "DATE-PRINTED:")?.Trim() ?? "NO_NOTICE_NUM",
                DatePrinted = Header.GetStringInBetweenSection("DATE-PRINTED:", "FROM:")?.Trim() ?? "NO_DATE_PRINTED",
                From = Header.GetStringInBetweenSection("FROM:", "SECTION:")?.Trim() ?? "NO_FROM",
                Section = Header.GetStringInBetweenSection("SECTION:")?.Trim() ?? "NO_SECTION",
                Content = Content?.ToString() ?? "NO_CONTENT" 
            };

            return fileParse;
        }

        private void ProcessParseFile(FileParse fileParse)
        {
            if (fileParse != null)
            {
                Count++;
                string parsedFile = $"{Count:D8}-{Language}Notice_{fileParse.CaseNumber}_{fileParse.NoticeNum}_{fileParse.DatePrinted}";
                string parsedFileName = Path.Combine(textBox2.Text, parsedFile);
                string fullName = $"{parsedFileName}.pdf";

                if (File.Exists(fullName)) File.Delete(fullName);

                //using (StreamWriter sw = new(fullName))
                //{
                //    sw.WriteLine($"Case Name: {fileParse.CaseName}");
                //    sw.WriteLine($"Case #: {fileParse.CaseNumber}");
                //    sw.WriteLine($"Program: {fileParse.Program}");
                //    sw.WriteLine($"Month: {fileParse.Month}");
                //    sw.WriteLine($"Notice #: {fileParse.NoticeNum}");
                //    sw.WriteLine($"Date Printed: {fileParse.DatePrinted}");
                //    sw.WriteLine($"From: {fileParse.From}");
                //    sw.WriteLine($"Section: {fileParse.Section}");
                //    sw.WriteLine("Content:");
                //    sw.Write(fileParse.Content);
                //}

                StringBuilder sb = new();
                //sb.Append($"<div style='line-height:1em;'>Case Name: {fileParse.CaseName}</div>");
                //sb.Append($"<div style='line-height:1em;'>Case #: {fileParse.CaseNumber}</div>");
                //sb.Append($"<div style='line-height:1em;'>Program: {fileParse.Program}</div>");
                //sb.Append($"<div style='line-height:1em;'>Month: {fileParse.Month}</div>");
                //sb.Append($"<div style='line-height:1em;'>Notice #: {fileParse.NoticeNum}</div>");
                //sb.Append($"<div style='line-height:1em;'>Date Printed: {fileParse.DatePrinted}</div>");
                //sb.Append($"<div style='line-height:1em;'>From: {fileParse.From}</div>");
                //sb.Append($"<div style='line-height:1em;'>Content:</div>");
                //sb.Append($"<pre>{fileParse.Content.ToString().Replace("\r\n", "<br/>")}</pre>");

                DocumentBuilder documentBuilder = DocumentBuilder.New();

                FontBuilder docFont = FontBuilder.New().SetName("Times").SetSize(8);

                var sectionBuilder = documentBuilder.AddSection()
                    .SetSize(PaperSize.Letter)
                    .SetOrientation(Gehtsoft.PDFFlow.Models.Enumerations.PageOrientation.Portrait)
                    .SetStyleFont(docFont);

                sectionBuilder.AddTable(tb =>
                {
                    tb.SetBorder(Stroke.None).SetContentPadding(0f);
                    tb.AddColumnToTable();
                    tb.AddRow().AddCellToRow($"Case Name: { fileParse.CaseName}");
                    tb.AddRow().AddCellToRow($"Case #: {fileParse.CaseNumber}");
                    tb.AddRow().AddCellToRow($"Program: {fileParse.Program}");
                    tb.AddRow().AddCellToRow($"Benefit Month: {fileParse.Month}");
                    tb.AddRow().AddCellToRow($"Notice #: {fileParse.NoticeNum}");
                    tb.AddRow().AddCellToRow($"Date Printed: {fileParse.DatePrinted}");
                    tb.AddRow().AddCellToRow($"From: {fileParse.From}");
                    tb.AddRow().AddCellToRow($"Content:");
                    tb.SetMarginBottom(10);
                });

                // Add title
                var contentFont = FontBuilder.New()
                    .SetName("Courier")
                    .SetSize(8);

                sectionBuilder
                    .SetStyleFont(contentFont)
                    .AddParagraph(fileParse.Content)
                    .SetMarginBottom(10);

                documentBuilder.Build(fullName);

                AppendTextBox3($"{parsedFile} has been created");
                AppendTextBox3(Environment.NewLine);

                Loop.Information($"{parsedFile} has been created");
            }
        }

        private string GetLangShorthand(string language)
        {
            return language switch
            {
                "English" => "ENG",
                "Spanish" => "SPA",
                _ => string.Empty,
            };
        }

        private void CheckedChanged(object sender, EventArgs e)
        {
            var radio = groupBox1.Controls.OfType<RadioButton>
                       ().FirstOrDefault(r => r.Checked)?.Text ?? string.Empty;

            Language = GetLangShorthand(radio);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            StopLoop = true;
        }

        private void AppendTextBox3(string text)
        {
            //Check if invoke requied if so return - as i will be recalled in correct thread
            if (ControlInvokeRequired(textBox3, () => AppendTextBox3(text))) return;
            textBox3.AppendText(text);
        }

        /// <summary>
        /// Helper method to determin if invoke required, if so will rerun method on correct thread.
        /// if not do nothing.
        /// </summary>
        /// <param name="c">Control that might require invoking</param>
        /// <param name="a">action to preform on control thread if so.</param>
        /// <returns>true if invoke required</returns>
        public bool ControlInvokeRequired(Control c, Action a)
        {
            if (c.InvokeRequired) c.Invoke(new MethodInvoker(delegate { a(); }));
            else return false;

            return true;
        }
    }
}