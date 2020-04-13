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
using System.Windows.Forms.DataVisualization.Charting;

namespace Covid19DoublingTime
{
    /// <summary>
    /// doubling time
    /// </summary>
    public partial class Form1 : Form
    {

        /// <summary>
        /// display doubling time
        /// </summary>
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            using (Wve.HourglassCursor waitCursor = new Wve.HourglassCursor())
            {
                try
                {
                    //initialize status bar
                    statusStrip1.Items.Clear();
                    statusStrip1.Items.Add(".");
                    //sizing required to avoid problems with long text
                    statusStrip1.Items[0].TextAlign = ContentAlignment.MiddleLeft;
                    statusStrip1.LayoutStyle = ToolStripLayoutStyle.Flow;

                    //check if file exists
                    FileInfo fi = new FileInfo(MainClass.DataFileName);
                    if (!fi.Exists)
                    {
                        MessageBox.Show("Hi.  I don't see the data file we're looking for, named \r\n" +
                            MainClass.DataFileName +
                            ".  Please see Tools, About to see who to load that from the Internet.  ");
                        return;
                    }
                    loadData();
                    //load combobox
                    for(int i=0; i<MainClass.CovidDataSet.Length; i++)
                    {
                        comboBoxPlaces.Items.Add(new MainClass.DataPlace(
                            MainClass.CovidDataSet[i][0], MainClass.CovidDataSet[i][1], i));
                    }
                    if(comboBoxPlaces.Items.Count > 0)
                    {
                        foreach (object o in comboBoxPlaces.Items)
                        {
                            if (((MainClass.DataPlace)o).Place.Trim() == "US")
                            {
                                comboBoxPlaces.SelectedItem = o;
                            }
                        }
                    }
                }
                catch (Exception er)
                {
                    Wve.MyEr.Show(this, er, true);
                }
            }
        }

        private void loadData()
        {
            //check if file exists
            FileInfo fi = new FileInfo(MainClass.DataFileName);
            if (!fi.Exists)
            {
                MessageBox.Show("Hi.  I don't see the data file we're looking for, named \r\n" +
                    MainClass.DataFileName +
                    ".  Please see Tools, About to see who to load that from the Internet.  ");
                return;
            }
            using (System.IO.StreamReader sr = new StreamReader(MainClass.DataFileName))
            {
                MainClass.CovidDataSet = new string[1000][];
                string[] parts;
                string line;
                int lineNum = 0;
                while ((line = sr.ReadLine()) != null)
                {
                    parts = Wve.WveTools.ReadCsvLine(line);
                    MainClass.CovidDataSet[lineNum] = parts;
                    lineNum++;
                }
                //resize dataset
                string[][] resized = new string[lineNum][];
                for(int i=0; i<lineNum; i++)
                {
                    resized[i] = MainClass.CovidDataSet[i];
                }
                MainClass.CovidDataSet = resized;
                //report
                StringBuilder sb = new StringBuilder();
                sb.Append("read ");
                sb.Append(lineNum);
                sb.Append(" lines.");
                sb.Append(Environment.NewLine);
                sb.Append("First datum is ");
                sb.Append(MainClass.CovidDataSet[0][0]);
                MessageBox.Show(sb.ToString());
            }//using
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonGo_Click(object sender, EventArgs e)
        {
            calculateAndDisplay();
            
        }

        private void calculateAndDisplay()
        {
            using (Wve.HourglassCursor waitCursor = new Wve.HourglassCursor())
            {
                try
                {
                    if (comboBoxPlaces.SelectedItem != null)
                    {
                        int firstDataColIX = 4; //fifth col starts data
                        MainClass.DataPlace place = (MainClass.DataPlace)comboBoxPlaces.SelectedItem;
                        string outputFileName = MainClass.DefaultOutputName.Replace("XXX", place.SubPlace + place.Place);
                        string[] stringCasesRow = MainClass.CovidDataSet[place.Index];
                        double[] casesRow = new double[stringCasesRow.Length - firstDataColIX];
                        for (int i = 0; i < casesRow.Length; i++)
                        {
                            casesRow[i] = double.Parse(stringCasesRow[i + firstDataColIX]);
                        }
                        double[] logRow = (double[])casesRow.Clone();
                        //initialize  with zeros
                        double[] doublingRow = (double[])casesRow.Clone();
                        //data starts at fifth row
                        for (int i = 0; i < doublingRow.Length; i++)
                        {
                            doublingRow[i] = 0;
                        }
                        double[] newCasesRow = (double[])doublingRow.Clone();
                        double[] spreadRateRow = (double[])doublingRow.Clone();

                        //calculate
                        double casesRef = 0; //the number of cases we are looking to double after so many days
                        for (int i = 0; i < casesRow.Length; i++)
                        {
                            logRow[i] = casesRow[i] > 0 ? Math.Log10(casesRow[i]) : 0;
                            if (i > 0)
                            {
                                newCasesRow[i] = casesRow[i] - casesRow[i - 1];
                            }
                            if (i > MainClass.IncubationDays)
                            {
                                if (newCasesRow[i - MainClass.IncubationDays] != 0)
                                {
                                    spreadRateRow[i] = newCasesRow[i] / newCasesRow[i - MainClass.IncubationDays];
                                }
                            }

                            //i is the row we are counting from to find next doubling
                            casesRef = casesRow[i];
                            for (int j = i + 1; j < casesRow.Length; j++)
                            {
                                if (casesRow[j] >= casesRef * 2)
                                {
                                    doublingRow[j] = (j - i);
                                    //break;
                                }
                            }
                        }

                        //write to file
                        using (StreamWriter sw = new StreamWriter(outputFileName,
                               false)) //false to append
                        {
                            string[] datesRow = new string[stringCasesRow.Length - firstDataColIX + 1];
                            Array.Copy(MainClass.CovidDataSet[0], firstDataColIX, datesRow, 1, datesRow.Length - 1);
                            //format
                            string[] strCases = new string[casesRow.Length + 1];
                            string[] strDoubling = new string[casesRow.Length + 1];
                            string[] strLog = new string[casesRow.Length + 1];
                            string[] strNewCases = new string[casesRow.Length + 1];
                            string[] strSpreadRate = new string[casesRow.Length + 1];
                            strCases[0] = "Cases";
                            strDoubling[0] = "DoublingTime";
                            strLog[0] = "Log";
                            strNewCases[0] = "NewCases";
                            strSpreadRate[0] = "Spread";
                            for (int i = 0; i < casesRow.Length; i++)
                            {
                                strCases[i + 1] = string.Format(String.Format("{0:0.#}", casesRow[i]));
                                strDoubling[i + 1] = string.Format(String.Format("{0:0.#}", doublingRow[i]));
                                strLog[i + 1] = string.Format(String.Format("{0:0.#}", logRow[i]));
                                strNewCases[i + 1] = string.Format(String.Format("{0:0.#}", newCasesRow[i]));
                                strSpreadRate[i + 1] = string.Format(String.Format("{0:0.#}", spreadRateRow[i]));
                            }
                            sw.WriteLine(Wve.WveTools.WriteCsv(datesRow, false, false, false));  // row, last comma, force quotes, newline
                            sw.WriteLine(Wve.WveTools.WriteCsv(strCases, false, false, false));  // row, last comma, force quotes, newline
                            sw.WriteLine(Wve.WveTools.WriteCsv(strDoubling, false, false, false));  // row, last comma, force quotes, newline
                            sw.WriteLine(Wve.WveTools.WriteCsv(strLog, false, false, false));  // row, last comma, force quotes, newline
                            sw.WriteLine(Wve.WveTools.WriteCsv(strNewCases, false, false, false));  // row, last comma, force quotes, newline
                            sw.WriteLine(Wve.WveTools.WriteCsv(strSpreadRate, false, false, false));  // row, last comma, force quotes, newline
                            sw.Flush();
                        }
                        MessageBox.Show("wrote file " + outputFileName);

                        //now graphs
                        chart1.Series.Clear();
                        chart2.Series.Clear();
                        chart3.Series.Clear();
                        chart4.Series.Clear();
                        //chart1.ChartAreas.Clear();
                        //chart2.ChartAreas.Clear();
                        //chart3.ChartAreas.Clear();
                        //chart4.ChartAreas.Clear();

                        Series s = new Series("cases"); //cases will be the legendText
                        //s.LegendText = "legend text";
                        s.ChartType = SeriesChartType.Line;
                        s.BorderWidth = 3;
                        s.MarkerStyle = MarkerStyle.Circle;
                        s.MarkerSize = 3;
                        s.MarkerColor = Color.Black;
                        //s.IsVisibleInLegend = false;

                        chart1.Series.Add(s);
                        for (int i = 0; i < casesRow.Length; i++)
                        {
                            s.Points.AddXY(i, casesRow[i]);
                        }


                        Series s2 = new Series("log");
                        s2.ChartType = SeriesChartType.Line;
                        s2.BorderWidth = 3;
                        s2.MarkerStyle = MarkerStyle.Circle;
                        s2.MarkerSize = 3;
                        s2.MarkerColor = Color.Black;
                        //s2.IsVisibleInLegend = false;

                        chart3.Series.Add(s2);
                        for (int i = 0; i < logRow.Length; i++)
                        {
                            s2.Points.AddXY(i, logRow[i]);
                        }

                        Series s3 = new Series("new cases");
                        s3.ChartType = SeriesChartType.Candlestick;
                        s3.BorderWidth = 3;
                        s3.MarkerStyle = MarkerStyle.Circle;
                        s3.MarkerSize = 3;
                        s3.MarkerColor = Color.Black;
                        //.IsVisibleInLegend = false;

                        chart2.Series.Add(s3);
                        for (int i = 0; i < newCasesRow.Length; i++)
                        {
                            s3.Points.AddXY(i, newCasesRow[i]);
                        }

                        Series s4 = new Series("doubling time");
                        s4.ChartType = SeriesChartType.Line;
                        s4.BorderWidth = 3;
                        s4.MarkerStyle = MarkerStyle.Circle;
                        s4.MarkerSize = 3;
                        s4.MarkerColor = Color.Black;
                        //s4.IsVisibleInLegend = false;

                        chart4.Series.Add(s4);
                        for (int i = 0; i < doublingRow.Length; i++)
                        {
                            s4.Points.AddXY(i, doublingRow[i]);
                        }

                        chart1.ChartAreas[0].RecalculateAxesScale();
                        chart2.ChartAreas[0].RecalculateAxesScale();
                        chart3.ChartAreas[0].RecalculateAxesScale();
                        chart4.ChartAreas[0].RecalculateAxesScale();
                    }
                }
                catch (Exception er)
                {
                    Wve.MyEr.Show(this, er, true);
                }
            }
        }

        private void comboBoxPlaces_SelectedIndexChanged(object sender, EventArgs e)
        {
            using (Wve.HourglassCursor waitCursor = new Wve.HourglassCursor())
            {
                try
                {
                    if(comboBoxPlaces.SelectedItem != null)
                    {
                        labelTitle.Text = comboBoxPlaces.SelectedItem.ToString();
                    }
                    calculateAndDisplay();
                }
                catch (Exception er)
                {
                    Wve.MyEr.Show(this, er, true);
                }
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {

            using (Wve.HourglassCursor waitCursor = new Wve.HourglassCursor())
            {
                try
                {

                    StringBuilder sb = new StringBuilder();
                    sb.Append("To calculate and display the doubling rate of Covid-19 cases in the U.S. as a function of time \r\n" +

                    "Data comes from Johns Hopkins Coronavirus COVID - 19 Global Cases by the Center for Systems Science and Engineering(CSSE) at Johns Hopkins University(JHU) \r\n" +
                    "  https://www.arcgis.com/apps/opsdashboard/index.html#/bda7594740fd40299423467b48e9ecf6 \r\n" +
                    "and in particular, from their daily updated dataset at github \r\n " +
                    " https://github.com/CSSEGISandData/COVID-19/tree/master/csse_covid_19_data/csse_covid_19_time_series \r\n");
                    // was "  https://github.com/CSSEGISandData/COVID-19/tree/master/csse_covid_19_data \r\n");

                    sb.Append(Environment.NewLine);
                    sb.Append(Environment.NewLine);
                    sb.Append(" The data must be copied and saved with the name ");
                    sb.Append(MainClass.DataFileName);
                    sb.Append(".");

                    Wve.TextViewer.ShowText("About this program", sb.ToString());
                }
                catch (Exception er)
                {
                    Wve.MyEr.Show(this, er, true);
                }
            }
        }
    }
}
