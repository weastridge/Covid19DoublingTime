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
            selfReference = this;
        }

        /// <summary>
        /// static ref
        /// </summary>
        internal static Form1 selfReference;
        /// <summary>
        /// ignore events while loading controls
        /// </summary>
        private bool ignoreChangeEvents = false;

        /// <summary>
        /// reload the main form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal static void ReloadForm1(object sender, EventArgs e)
        {
            using (Wve.HourglassCursor waitCursor = new Wve.HourglassCursor())
            {
                try
                {
                    selfReference.Form1_Load(sender, e);
                }
                catch (Exception er)
                {
                    Wve.MyEr.Show("Form1", er, true);
                }
            }
        }

        /// <summary>
        /// load form 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

                    string dataFileName = "";
                    //IX in row of place and subplace
                    int placeNameIX = 1;
                    int subplaceNameIx = 0;
                    switch (MainClass.TypeOfData)
                    {
                        case "Hopkins_World":
                            placeNameIX = 1;
                            subplaceNameIx = 0;
                            dataFileName = MainClass.DataFileNameforCountries;
                            break;
                        case "Hopkins_US":
                            placeNameIX = 6;
                            subplaceNameIx = 5;
                            dataFileName = MainClass.DataFileNameForStates;
                            break;
                        default:
                            break;
                    }
                    //check if file exists
                    FileInfo fi = new FileInfo(dataFileName);
                    if (!fi.Exists)
                    {
                        MessageBox.Show("Hi.  I don't see the data file we're looking for, named \r\n" +
                            dataFileName +
                            ".  Please see Tools, About to see who to load that from the Internet.  ");
                        return;
                    }
                    loadData(dataFileName);
                    //load combobox
                    ignoreChangeEvents = true;
                    comboBoxPlaces.Items.Clear();
                    for(int i=0; i<MainClass.CovidDataSet.Length; i++)
                    {
                        comboBoxPlaces.Items.Add(new MainClass.DataPlace(
                            MainClass.CovidDataSet[i][subplaceNameIx], MainClass.CovidDataSet[i][placeNameIX], i));
                    }
                    ignoreChangeEvents = false;
                    if(comboBoxPlaces.Items.Count > 0)
                    {
                        bool foundIt = false;
                        foreach (object o in comboBoxPlaces.Items)
                        {
                            if (((MainClass.DataPlace)o).Place.Trim() == "US")
                            {
                                comboBoxPlaces.SelectedItem = o;
                                foundIt = true;
                                break;
                            }
                        }
                        if (!foundIt)
                        {
                            comboBoxPlaces.SelectedIndex = 1;
                        }
                    }
                }
                catch (Exception er)
                {
                    Wve.MyEr.Show(this, er, true);
                }
            }
        }

        private void loadData(string datafilename)
        {
            //check if file exists
            FileInfo fi = new FileInfo(datafilename);
            if (!fi.Exists)
            {
                MessageBox.Show("Hi.  I don't see the data file we're looking for, named \r\n" +
                    datafilename +
                    ".  Please see Tools, About to see who to load that from the Internet.  ");
                return;
            }
            using (System.IO.StreamReader sr = new StreamReader(datafilename))
            {
                MainClass.CovidDataSet = new string[10000][];
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
                    statusStrip1.Items[0].Text = "calculating...";
                    if (comboBoxPlaces.SelectedItem != null)
                    {
                        int firstDataColIX = int.MinValue; //zero based index if first column containing data   
                        MainClass.DataPlace place = (MainClass.DataPlace)comboBoxPlaces.SelectedItem;
                        string outputFileName = MainClass.DefaultOutputName.Replace("XXX", place.SubPlace + place.Place);
                        string[] stringCasesRow = MainClass.CovidDataSet[place.Index];
                        string[] stringHeaderRow = MainClass.CovidDataSet[0];
                        string lastDate = stringHeaderRow[stringHeaderRow.Length - 1];
                        
                        switch(MainClass.TypeOfData) // == "Hopkins_World")
                        {
                            case "Hopkins_World":
                                firstDataColIX = 4; //fifth col starts data
                                break;
                            case "Hopkins_US":
                                firstDataColIX = 11; //12th col starts data
                                break;
                            default:
                                break;
                        }
                        
                        double[] casesRow = new double[stringCasesRow.Length - firstDataColIX];
                        for (int i = 0; i < casesRow.Length; i++)
                        {
                            casesRow[i] = double.Parse(stringCasesRow[i + firstDataColIX]);
                        }
                        double[] logRow = (double[])casesRow.Clone();
                        //initialize  with zeros
                        double[] doublingRow = (double[])casesRow.Clone();
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

                        Series s5 = new Series("spread rate");
                        //use defaults
                        //s5.ChartType = SeriesChartType.Line;
                        //s5.BorderWidth = 3;
                        //s5.MarkerStyle = MarkerStyle.Circle;
                        //s5.MarkerSize = 3;
                        //s5.MarkerColor = Color.Black;
                        //s5.IsVisibleInLegend = false;



                        chart4.Series.Add(s4);
                        for (int i = 0; i < doublingRow.Length; i++)
                        {
                            s4.Points.AddXY(i, doublingRow[i]);
                        }

                        chart4.Series.Add(s5);
                        for (int i = 0; i < doublingRow.Length; i++)
                        {
                            s5.Points.AddXY(i, spreadRateRow[i]);
                        }

                        chart1.ChartAreas[0].RecalculateAxesScale();
                        chart2.ChartAreas[0].RecalculateAxesScale();
                        chart3.ChartAreas[0].RecalculateAxesScale();
                        chart4.ChartAreas[0].RecalculateAxesScale();

                        statusStrip1.Items[0].Text = "Calculated, last data date is " + lastDate;
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
                    if (!ignoreChangeEvents)
                    {
                        if (comboBoxPlaces.SelectedItem != null)
                        {
                            labelTitle.Text = comboBoxPlaces.SelectedItem.ToString();
                        }
                        calculateAndDisplay();
                    }
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
                    AboutForm dlg = new AboutForm();
                    dlg.ShowDialog();
                }
                catch (Exception er)
                {
                    Wve.MyEr.Show(this, er, true);
                }
            }
        }


        private void settingsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            using (Wve.HourglassCursor waitCursor = new Wve.HourglassCursor())
            {
                try
                {
                    SettingsForm dlg = new SettingsForm();
                    dlg.ShowDialog();
                }
                catch (Exception er)
                {
                    Wve.MyEr.Show(this, er, true);
                }
            }
        }

        private void buttonRefresh_Click(object sender, EventArgs e)
        {
            using (Wve.HourglassCursor waitCursor = new Wve.HourglassCursor())
            {
                try
                {
                    this.Form1_Load(sender, e);
                }
                catch (Exception er)
                {
                    Wve.MyEr.Show(this, er, true);
                }
            }
        }
    }
}
