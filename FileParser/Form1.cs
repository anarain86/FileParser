using System.Text;
using FileParser.Models;
using FileParser.Extension;

namespace FileParser
{
    public partial class Form1 : Form
    {
        private StringBuilder Header { get; set; }
        private StringBuilder Content { get; set; }
        private int Count { get; set; }

        enum HeaderOrContent
        {
            Header,
            Content
        }

        HeaderOrContent? Choose { get; set; }


        public Form1()
        {
            Header = new StringBuilder();
            Content = new StringBuilder();
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

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                Header = new StringBuilder();
                Content = new StringBuilder();
                Choose = null;
                Count = 0;
                textBox3.Clear();

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
                while (sr.Peek() >= 0)
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

                fileParse = CreateParseFile();

                if (fileParse != null) ProcessParseFile(fileParse);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }

        private FileParse? CreateParseFile()
        {
            if (Choose == null) return null;

            FileParse fileParse = new()
            {
                CaseName = Header.GetStringInBetweenSection("0CASE NAME:", "CASE NUMBER:")?.Trim() ?? "NO_CASE_NAME",
                CaseNumber = Header.GetStringInBetweenSection("CASE NUMBER:", "PROG:")?.Trim() ?? "NO_CASE_NUMBER",
                Program = Header.GetStringInBetweenSection("PROG:", "MONTH:")?.Trim() ?? "NO_PROG",
                Month = Header.GetStringInBetweenSection("MONTH:", "NOTICE-NUMBER:")?.Trim() ?? "NO_MONTH",
                NoticeNum = Header.GetStringInBetweenSection("NOTICE-NUMBER:", "DATE-PRINTED:")?.Trim() ?? "NO_NOTICE_NUM",
                DatePrinted = Header.GetStringInBetweenSection("DATE-PRINTED:", "FROM:")?.Trim() ?? "NO_DATE_PRINTED",
                From = Header.GetStringInBetweenSection("FROM", "SECTION:")?.Trim() ?? "NO_FROM",
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
                string parsedFile = $"{Count:D8}-{fileParse.CaseNumber}_{fileParse.NoticeNum}";
                string parsedFileName = Path.Combine(textBox2.Text, parsedFile);
                string fullName = $"{parsedFileName}.txt";

                if (File.Exists(fullName)) File.Delete(fullName);

                using (StreamWriter sw = new(fullName))
                {
                    sw.WriteLine($"Case Name: {fileParse.CaseName}");
                    sw.WriteLine($"Case Number: {fileParse.CaseNumber}");
                    sw.WriteLine($"Date Printed: {fileParse.DatePrinted}");
                    sw.Write(fileParse.Content);
                }

                textBox3.AppendText($"{parsedFile} has been created");
                textBox3.AppendText(Environment.NewLine);
            }
        }
    }
}