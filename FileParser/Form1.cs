using System.Text;
using FileParser.Models;
using FileParser.Extension;
using Serilog;
using Gehtsoft.PDFFlow.Builder;
using Gehtsoft.PDFFlow.Models.Enumerations;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace FileParser
{
    public partial class Form1 : Form
    {
        private StringBuilder Header { get; set; }
        private StringBuilder Content { get; set; }
        private string[] Footer { get; set; }
        private string[] NewHeader { get; set; }
        private int Count { get; set; }
        private string Language { get; set; }

        private bool StopLoop { get; set; }

        private Serilog.Core.Logger Loop { get; set; }
        //private Serilog.Core.Logger Error { get; set; }

        private CaseDictionary<string> English { get; set; }
        private CaseDictionary<string> Spanish { get; set; }

        enum HeaderOrContent
        {
            Header,
            Content,
            Footer
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

            //using (StreamReader r = new StreamReader(GetJson()))
            //{
            string json = GetJson();
            var jsonResult = JsonSerializer.Deserialize<FromJson>(json);
            //var jsonResult = JsonNode.Parse(json);

            if (jsonResult == null) return;

            English = jsonResult.Notices["English"];

            Spanish = jsonResult.Notices["Spanish"];
            //}

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
                Footer = new string[4];
                NewHeader = new string[5];
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

                FileParse2? fileParse = new();

                byte zeroCnt = 0;
                byte headerCnt = 0;
                byte footerCnt = 0;

                // Iterating the file
                while (sr.Peek() >= 0 && StopLoop == false)
                {
                    await Task.Run(() =>
                    {
                        try
                        {
                            line = sr.ReadLine();

                            if (line != null && line.StartsWith("1"))
                            {
                                fileParse = CreateParseFile2();

                                if (fileParse != null) ProcessParseFile2(fileParse);

                                //Header = new StringBuilder();
                                Content = new StringBuilder();
                                zeroCnt = 0;

                                Choose = null;
                            }
                            else if (line != null && line.StartsWith("0"))
                            {
                                zeroCnt++;

                                if (zeroCnt == 1)
                                {
                                    Choose = HeaderOrContent.Header;
                                    headerCnt = 0;
                                    //NewHeader[0] = line;
                                }
                                if (zeroCnt == 2)
                                {
                                    Choose = HeaderOrContent.Content;
                                    NewHeader[headerCnt] = line;
                                    //NewHeader[1] = line;
                                }

                            }
                            else if (line != null && line.StartsWith("4"))
                            {
                                //Footer[0] = line;

                                Choose = HeaderOrContent.Footer;
                                footerCnt = 0;
                            }
                            else if (Choose == HeaderOrContent.Content)
                            {
                                Content.AppendLine(line);
                            }

                            if (line != null && Choose == HeaderOrContent.Header)
                            {
                                NewHeader[headerCnt] = line;
                                headerCnt++;
                            }

                            if (line != null && Choose == HeaderOrContent.Footer)
                            {

                                Footer[footerCnt] = line;
                                footerCnt++;
                            }

                            //if (line != null && line.StartsWith("1Page"))
                            //{
                            //    fileParse = CreateParseFile2();

                            //    if (fileParse != null) ProcessParseFile(fileParse);

                            //    Header = new StringBuilder();
                            //    Content = new StringBuilder();
                            //}
                            //else
                            //{
                            //    if (line != null && line.StartsWith("0CASE NAME:"))
                            //    {
                            //        Choose = HeaderOrContent.Header;
                            //    }

                            //    if (line != null && line.StartsWith("0 "))
                            //    {
                            //        Choose = HeaderOrContent.Content;
                            //    }

                            //    if (Choose == HeaderOrContent.Header)
                            //    {
                            //        Header.Append(line);
                            //    }

                            //    if (Choose == HeaderOrContent.Content)
                            //    {
                            //        Content.AppendLine(line);
                            //    }
                            //}
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                            Log.Error($"{ex.Message} --- {ex.InnerException} --- {ex.StackTrace}");
                        }
                    });
                }

                if (StopLoop == false)
                {
                    fileParse = CreateParseFile2();

                    if (fileParse != null) ProcessParseFile2(fileParse);
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

        private FileParse2? CreateParseFile2()
        {
            var search = ":";
            if (Choose == null) return null;

            NewHeader[0] = NewHeader[0][1..].Trim();
            var h = System.Text.RegularExpressions.Regex.Split(NewHeader[0], @"\s{2,}");

            DateTime? theDate = DateTime.TryParse(h[2], out var parsedDate) ? parsedDate : null;

            NewHeader[4] = NewHeader[4][1..].Trim();

            var caseNum = h[1].Substring(h[1].IndexOf(search) + search.Length).Trim();

            Footer[0] = Footer[0][1..].Trim();
            var f = System.Text.RegularExpressions.Regex.Split(Footer[0], @"\s{2,}");

            // Add header on top of content
            Content.PrependLine(NewHeader[4]);
            Content.PrependLine(NewHeader[3]);
            Content.PrependLine(NewHeader[2]);
            Content.PrependLine(NewHeader[1]);
            Content.PrependLine(NewHeader[0]);

            // Add footer at the bottom of the content
            Content.AppendLine(Footer[0]);
            Content.AppendLine(Footer[1]);
            Content.AppendLine(Footer[2]);
            Content.AppendLine(Footer[3]);

            FileParse2 fileParse = new()
            {
                CaseNumber = caseNum ?? "NO_CASENUMBER",
                Date = theDate?.ToString("yyyyMMdd") ?? "NO_DATE",
                Type = NewHeader[4] ?? "NO_TYPE",
                Name = f[0] ?? "NO_NAME",
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
                    tb.AddRow().AddCellToRow($"Case Name: {fileParse.CaseName}");
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

        private void ProcessParseFile2(FileParse2 fileParse)
        {
            if (fileParse != null)
            {
                Language = "X";
                string? typeNumber = null;

                if (English.ContainsKeyCI(fileParse.Type))
                {
                    Language = "E";
                    typeNumber = English.AtCI[fileParse.Type]?.SingleOrDefault();
                }

                if (Spanish.ContainsKeyCI(fileParse.Type))
                {
                    Language = "S";
                    typeNumber = Spanish.AtCI[fileParse.Type]?.SingleOrDefault();
                }

                Count++;
                string parsedFile = $"{Count:D8}_{Language}_{fileParse.CaseNumber}_{typeNumber}_{fileParse.Date}";
                string parsedFileName = Path.Combine(textBox2.Text, parsedFile);
                string fullName = $"{parsedFileName}.pdf";

                if (File.Exists(fullName)) File.Delete(fullName);

                StringBuilder sb = new();

                DocumentBuilder documentBuilder = DocumentBuilder.New();

                FontBuilder docFont = FontBuilder.New().SetName("Times").SetSize(8);

                var sectionBuilder = documentBuilder.AddSection()
                    .SetSize(PaperSize.Letter)
                    .SetOrientation(PageOrientation.Portrait)
                    .SetStyleFont(docFont);

                sectionBuilder.AddTable(tb =>
                {
                    tb.SetBorder(Stroke.None).SetContentPadding(0f);
                    tb.AddColumnToTable();
                    tb.AddRow().AddCellToRow($"Case #: {fileParse.CaseNumber}");
                    tb.AddRow().AddCellToRow($"Type: {fileParse.Type}");
                    tb.AddRow().AddCellToRow($"Date Printed: {fileParse.Date}");
                    tb.AddRow().AddCellToRow($"Name: {fileParse.Name}");
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

        private static string GetJson()
        {
            string json = @"{
	            ""Data"": {
                    ""English"": {
                        ""MA APPROVAL FOR SSI RECIPIENT"": ""A602"",
			            ""SPEND-DOWN LIABILITY"": ""A603"",
			            ""INCOMPLETE RECERTIFICATION FORM (MA)"": ""A605"",
			            ""NOTICE OF ADMINISTRATIVE CHANGE"": ""A611"",
			            ""SPEND DOWN LIABILITY HCBS WAIVER"": ""A616"",
			            ""POTENTIAL MEDICARE BUY-IN ELIGIBILITY"": ""A617"",
			            ""UNDUE HARDSHIP CLAIM"": ""A618"",
			            ""APPROVAL OF UNDUE HARDSHIP CLAIM"": ""A619"",
			            ""DENIAL OF UNDUE HARDSHIP CLAIM"": ""A620"",
			            ""DENIAL / EXCESS INCOME"": ""A709"",
			            ""CHANGE IN MEDICAL ASSISTANCE COVERAGE"": ""A710"",
			            ""TERM. OF MA (FORMER RECIPIENT OF SSI"": ""A711"",
			            ""COND. DEN./EXCESS INCOME(INTERIM CHANGE"": ""A719"",
			            ""TERMINATION - MEDICAL ASSISTANCE"": ""A721"",
			            ""DC HEALTHY FAMILIES EXPANSION COVERAGE"": ""A730"",
			            ""MA CITIZENSHIP REQUEST (FAILED CHIPRA)"": ""A743"",
			            ""MA CITIZENSHIP REMINDER (FAILED CHIPRA)"": ""A744"",
			            ""ALIEN STATUS REQUEST"": ""A745"",
			            ""SUSPENSION OF DC MEDICAID COVERAGE"": ""A750"",
			            ""DENIAL/EXCESS INCOME - HCBS WAIVER"": ""A751"",
			            ""RECERT DENIAL/EXCESS INCOME-HCBS"": ""A754"",
			            ""EXCESS RESOURCES"": ""A756"",
			            ""COVERAGE PERIOD EXTENSION"": ""A757"",
			            ""EPD WAIVER TRANSITION APPROVAL"": ""A760"",
			            ""EPD TRANSITION DENIAL EXCESS INCOME"": ""A761"",
			            ""SSI MOVED OUT OF STATE"": ""A763"",
			            ""APPLICATION FOLLOW UP (30 DAYS)"": ""A801"",
			            ""CHECKLIST ITEM RECEIPT"": ""A802"",
			            ""APPLICATION REMINDER (10 DAY)"": ""A804"",
			            ""APPROVAL - MEDICAL ASSISTANCE"": ""A805"",
			            ""APPROVAL - SPEND DOWN"": ""A816"",
			            ""PATIENT PAYABILITY"": ""A817"",
			            ""QMB APPROVAL"": ""A821"",
			            ""MC APPROVAL - SHORT CERT"": ""A822"",
			            ""MA RECERTIFICATION - SHORT CERT"": ""A823"",
			            ""TERMINATION DEATH - ADULT ONLY CASE"": ""A826"",
			            ""TERMINATION DEATH - CASES WITH CHILDREN"": ""A829"",
			            ""APPROVAL - DUAL ELIGIBLE"": ""A830"",
			            ""ALLIANCE ADDITIONAL INFORMATION"": ""A832"",
			            ""PATIENT PAYABILITY CHANGE NOTICE"": ""A833"",
			            ""HOME BASED WAIVER APPROVAL"": ""A834"",
			            ""LONG TERM CARE EXCESS INCOME"": ""A844"",
			            ""LTC-NURSING HOME APPROVAL"": ""A865"",
			            ""APPROVAL-LTC SPEND DOWN"": ""A866"",
			            ""APPROVAL FOR EMERGENCY MEDICAID"": ""A868"",
			            ""SECOND SPEND DOWN- HCBS WAIVER"": ""A877"",
			            ""SECOND SPEND DOWN-NURSING HOMES"": ""A878"",
			            ""TERMINATION DEATH - ADULT ONLY CASE"": ""A881"",
			            ""REDETERMINE ELIGIBILITY (MA)"": ""C600"",
			            ""APPOINTMENT LETTER"": ""C609"",
			            ""ADMINISTRATIVE CONFERENCE/FAIR HEARING"": ""C613"",
			            ""REQUEST FOR SOCIAL SECURITY NUMBER"": ""C615"",
			            ""REQUEST FOR ADDITIONAL INFORMATION"": ""C616"",
			            ""GENERAL COMMUNICATION"": ""C617"",
			            ""NO CHANGE IN BENEFITS"": ""C618"",
			            ""NEW HOUSEHOLD MEMBER- INFORMATION NEEDED"": ""C619"",
			            ""ALLIANCE INFORMATION REQUEST"": ""C624"",
			            ""DENIAL"": ""C705"",
			            ""CITIZENSHIP STATUS REVIEW"": ""C710"",
			            ""ASSETS MAY EXCEED LIMIT"": ""C713"",
			            ""TERMINATION OF MEDICAL ASSISTANCE"": ""X703"",
			            ""TERMINATION OF EPD WAIVER PROGRAM"": ""X706"",
			            ""RECEIPT OF MEDICAL RECERTIFICATION FORM"": ""X727"",
			            ""EPD WAIVER RECERTIFICATION NOTICE"": ""X756"",
			            ""APPLICATION FOLLOW UP (30 DAY)"": ""X803"",
			            ""APPLICATION REMINDER (10 DAY)"": ""X804"",
			            ""DENIAL-ABANDONMENT"": ""X805"",
			            ""DENIAL - FAILURE TO PROVIDE VERIFICATION"": ""X806"",
			            ""NEW ALLIANCE RECERT REQUIREMENTS"": ""X815"",
			            ""SSI MOVED OUT OF STATE"": ""X818"",
			            ""Recertification Form/Label"": ""X998""
                    },
		            ""Spanish"": {
                        ""APROBACION DE MA PARA RECIPIENTE DE SSI"": ""A602"",
			            ""RESPONSABILIDAD DE SPEND-DOWN"": ""A603"",
			            ""RECERTIFICACION INCOMPLETA (MA)"": ""A605"",
			            ""RECHAZO/INGRESO EN EXCESO"": ""A709"",
			            ""CAMBIO EN EL PLAN DE ASISTENCIA MEDICA"": ""A710"",
			            ""TERMINACION - ASISTENCIA MEDICA"": ""A721"",
			            ""EXPANSION COBERTURA DC HEALTHY FAMILIES"": ""A730"",
			            ""PEDIDO DE CIUDADANIA MA (FAILED CHIPRA)"": ""A743"",
			            ""SUSPENSION DE COBERTURA DE DC MEDICAID"": ""A750"",
			            ""EXCESO DE RECURSOS"": ""A756"",
			            ""SEGUIMIENTO DE SOLICITUD (30 DIAS)"": ""A801"",
			            ""RECORDATORIO DE SOLICITUD  (10 DIAS)"": ""A804"",
			            ""APROBACION - ASISTENCIA MEDICA"": ""A805"",
			            ""CAPACIDAD DE PAGO DEL PACIENTE"": ""A817"",
			            ""APROBACION QMB"": ""A821"",
			            ""APROBACION DE AM - CERT CORTA"": ""A822"",
			            ""TERM. DEBIDO A MUERTE-ADULTOS SOLAMENTE"": ""A826"",
			            ""APROBACION - ELEGIBILIDAD DOBLE"": ""A830"",
			            ""INFORMACION ADICIONAL DE ALLIANCE"": ""A832"",
			            ""CAMBIO DE CAPACIDAD DE PAGO DEL PACIENTE"": ""A833"",
			            ""APROBACION RENUNCIA BASADA EN EL HOGAR"": ""A834"",
			            ""APROBACION DEL ASILO DE LA TERCERA EDAD"": ""A865"",
			            ""APROBACION PARA MEDICAID DE EMERGENCIA"": ""A868"",
			            ""REDETERMINACION DE ELEGILILIDAD (MA)"": ""C600"",
			            ""TRANSFER. DE CASO Y CAMBIO DE TRABAJADOR"": ""C607"",
			            ""PETICION DE INFORMACION ADICIONAL"": ""C616"",
			            ""COMUNICACION GENERAL"": ""C617"",
			            ""NO CAMBIO EN LOS BENEFICIOS"": ""C618"",
			            ""REQUISISTOS DE VERIFICACION"": ""C621"",
			            ""PETICION DE VERIFICACION DE EMPLEO"": ""C622"",
			            ""SOLICITUD DE INFORMACION DE ALIANZA"": ""C624"",
			            ""TERMINACION LA ALIANZA RECERT INCOMPLETA"": ""C627"",
			            ""NEGACION"": ""C705"",
			            ""VENCIMIENTO DE ASISTENCIA MEDICA"": ""X703"",
			            ""CANCELACION DE PROGRAMA DE EPD WAIVER"": ""X706"",
			            ""RECIBIO DE RECERTIFICACION MEDICA"": ""X727"",
			            ""AVISO DE EPD RECERTIFICACION DE RENUNCIA"": ""X756"",
			            ""SEGUIMIENTO DE SOLICITUD (30 DIAS)"": ""X803"",
			            ""RECORDATORIO DE SOLICITUD (10 DIAS)"": ""X804"",
			            ""RECHAZADO POR ABANDONO"": ""X805"",
			            ""RECHAADO - POR FALTA DE VERIFICACION"": ""X806"",
			            ""NUEVOS REQUISITOS PARA RECERT ALLIANCE"": ""X815"",
			            ""SSI FUERA DEL ESTADO"": ""X818"",
                        ""Forma de Recertificacion O Etiqueta"": ""X999""
                    }
                }
            }";

            return json;
        }
    }
}