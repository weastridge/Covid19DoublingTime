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
        /// list of places in deaths data set
        /// </summary>
        MainClass.DataPlace[] _deathsPlaces = null;
        /// <summary>
        /// running text string of results of operations
        /// </summary>
        private string _results = "For comparison, US covid death rate was 80000/331002651 = 24/100k on 5/9/2020 and all cause mortality 60/100k/month or 720/100k/year.\r\n\r\n";
        
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
                    string dataDeathsFileName = "";
                    //IX in row of place and subplace
                    int placeNameIX = 1;
                    int subplaceNameIx = 0;
                    switch (MainClass.TypeOfData)
                    {
                        case "Hopkins_World":
                            placeNameIX = 1;
                            subplaceNameIx = 0;
                            dataFileName = MainClass.DataFileNameforCountries;
                            dataDeathsFileName = MainClass.DataFileNameForCountriesDeaths;
                            break;
                        case "Hopkins_US":
                            placeNameIX = 6;
                            subplaceNameIx = 5;
                            dataFileName = MainClass.DataFileNameForStates;
                            dataDeathsFileName = MainClass.DataFileNameForStatesDeaths;
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
                            ".  Please see Tools, Settings to see how to load that from the Internet.  ");
                        return;
                    }
                    fi = new FileInfo(dataDeathsFileName);
                    if (!fi.Exists)
                    {
                        MessageBox.Show("Hi.  I don't see the data file we're looking for, named \r\n" +
                            dataDeathsFileName +
                            ".  Please see Tools, Settings to see how to load that from the Internet.  ");
                        return;
                    }
                    loadData(dataFileName, dataDeathsFileName);
                    //load deaths places
                    _deathsPlaces = null;
                    if(MainClass.CovidDeathsDataSet != null)
                    {
                        _deathsPlaces = new MainClass.DataPlace[MainClass.CovidDeathsDataSet.Length];
                        for(int i=0; i<MainClass.CovidDeathsDataSet.Length; i++)
                        {
                            _deathsPlaces[i] = new MainClass.DataPlace(
                                MainClass.CovidDeathsDataSet[i][subplaceNameIx], 
                                MainClass.CovidDeathsDataSet[i][placeNameIX],
                                i);
                        }
                    }
                    //load combobox
                    ignoreChangeEvents = true;
                    comboBoxPlaces.Items.Clear();
                    for(int i=0; i<MainClass.CovidDataSet.Length; i++)
                    {
                        comboBoxPlaces.Items.Add(new MainClass.DataPlace(
                            MainClass.CovidDataSet[i][subplaceNameIx], 
                            MainClass.CovidDataSet[i][placeNameIX], 
                            i));
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
                            else if(((MainClass.DataPlace)o).Place.Trim() == "Ballad Area")
                            {
                                comboBoxPlaces.SelectedItem = o;
                                foundIt = true;
                                break;
                            }
                            //else if((((MainClass.DataPlace)o).Place.Trim() == "Tennessee") &&
                            //    (((MainClass.DataPlace)o).SubPlace.Trim() == "Sullivan"))
                            //{
                            //    comboBoxPlaces.SelectedItem = o;
                            //    foundIt = true;
                            //    break;
                            //}
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

        private void loadData(string datafilename, string deathsDataFileName)
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
            //read cases data into data set
            using (System.IO.StreamReader sr = new StreamReader(datafilename))
            {
                MainClass.CovidDataSet = new string[10000][];
                MainClass.CovidDeathsDataSet = null; //unless data file found for i
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
                statusStrip1.Items[0].Text = sb.ToString();
                //MessageBox.Show(sb.ToString());
            }//using
             //append Ballad if this is states data
            if (datafilename == MainClass.DataFileNameForStates)
            {
                MainClass.AggregateBallad agg = new MainClass.AggregateBallad();
                int rowsFound = agg.AppendAggregate(ref MainClass.CovidDataSet, 6, 5, 11);
                string s = rowsFound.ToString();
            }
            //and deaths data into deaths data set
            if ((!string.IsNullOrEmpty(deathsDataFileName)) && File.Exists(deathsDataFileName))
            {
                using (System.IO.StreamReader sr = new StreamReader(deathsDataFileName))
                {
                    MainClass.CovidDeathsDataSet = new string[10000][];
                    string[] parts;
                    string line;
                    int lineNum = 0;
                    while ((line = sr.ReadLine()) != null)
                    {
                        parts = Wve.WveTools.ReadCsvLine(line);
                        MainClass.CovidDeathsDataSet[lineNum] = parts;
                        lineNum++;
                    }
                    //resize dataset
                    string[][] resized = new string[lineNum][];
                    for (int i = 0; i < lineNum; i++)
                    {
                        resized[i] = MainClass.CovidDeathsDataSet[i];
                    }
                    MainClass.CovidDeathsDataSet = resized;
                    //report
                    StringBuilder sb = new StringBuilder();
                    sb.Append("read ");
                    sb.Append(lineNum);
                    sb.Append(" lines.");
                    sb.Append(Environment.NewLine);
                    sb.Append("First datum is ");
                    sb.Append(MainClass.CovidDeathsDataSet[0][0]);
                    statusStrip1.Items[0].Text = sb.ToString();
                    //MessageBox.Show(sb.ToString());
                }//using
                //append Ballad if this is states data
                if(deathsDataFileName == MainClass.DataFileNameForStatesDeaths)
                {
                    MainClass.AggregateBallad agg = new MainClass.AggregateBallad();
                    int rowsFound = agg.AppendAggregate(ref MainClass.CovidDeathsDataSet, 6, 5, 11); //11 is population
                    //string s = rowsFound.ToString();
                }
            }//if not null deathsdatafilename
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonGo_Click(object sender, EventArgs e)
        {
            using (Wve.HourglassCursor waitCursor = new Wve.HourglassCursor())
            {
                try
                {
                    List<PointF> points = new List<PointF>()
                    {
                        new PointF(5, -0.15f),
                        new PointF(6, -0.13f),
                        new PointF(7, -0.11f),
                        new PointF(8, -0.09f),
                        new PointF(9, -0.07f),
                        new PointF(10, -0.05f),
                        new PointF(11, -0.03f),
                        new PointF(12, -0.01f),
                        new PointF(13, 0.01f),
                        new PointF(14, 0.03f)
                    };
                    double m;
                    double b;
                    MainClass.FindLinearLeastSquaresFit(points, out m, out b);
                    MessageBox.Show("m=" + m.ToString() + ", b=" + b.ToString() +
                        "\r\nand if x=10 then y=" + (m * 10 + b).ToString()
                        ) ;
                }
                catch (Exception er)
                {
                    Wve.MyEr.Show(this, er, true);
                }
            }
        }

        private void calculateAndDisplay()
        {
            using (Wve.HourglassCursor waitCursor = new Wve.HourglassCursor())
            {
                try
                {
                    statusStrip1.Items[0].Text = "calculating...";
                    labelTitle.Text = "Calculating for ...";
                    //TextBoxResults.Clear();
                    StringBuilder sbResults = new StringBuilder();
                    if (comboBoxPlaces.SelectedItem != null)
                    {
                        int firstDataColIX = int.MinValue; //zero based index if first column containing data   
                        MainClass.DataPlace place = (MainClass.DataPlace)comboBoxPlaces.SelectedItem;
                        string outputFileName = MainClass.DefaultOutputName.Replace("XXX", place.SubPlace + place.Place);
                        string[] stringCasesRow = MainClass.CovidDataSet[place.Index];
                        string[] stringDeathsRow = null; //unless found
                        if (MainClass.CovidDeathsDataSet != null)
                        {
                            //find matching place in deaths set
                            for (int i=0; i<_deathsPlaces.Length; i++)
                            {
                                if((place.Place.Trim().ToLower() == _deathsPlaces[i].Place.Trim().ToLower()) &&
                                    (place.SubPlace.Trim().ToLower() == _deathsPlaces[i].SubPlace.Trim().ToLower()))
                                {
                                    stringDeathsRow = MainClass.CovidDeathsDataSet[i];
                                    break;
                                }
                            }
                        }
                        string[] stringHeaderRow = MainClass.CovidDataSet[0];
                        string[] stringDeathsHeaderRow = MainClass.CovidDeathsDataSet[0];
                        string lastDate = stringHeaderRow[stringHeaderRow.Length - 1];
                        double population = double.MinValue; //unless population found in deaths row
                        double totalDeaths = int.MinValue; //unless assigned
                        int populationColumns = 0;//unless Hopkins_US the deaths table includes a col for population
                        
                        switch(MainClass.TypeOfData) // == "Hopkins_World")
                        {
                            case "Hopkins_World":
                                firstDataColIX = 4; //fifth col starts data
                                break;
                            case "Hopkins_US":
                                firstDataColIX = 11; //12th col starts data
                                //but notice the deaths dataset has population in 12th col and deaths starting 13th
                                populationColumns = 1;
                                break;
                            default:
                                break;
                        }
                        double[] deathsRow = null; //unless created
                        if (stringDeathsRow != null)
                        {
                            deathsRow = new double[stringDeathsRow.Length - firstDataColIX - populationColumns];//because indlude population
                            for (int i = 0; i < deathsRow.Length; i++)
                            {
                                deathsRow[i] = double.Parse(stringDeathsRow[i + firstDataColIX + populationColumns]);
                                //save last one
                                if(i==deathsRow.Length-1) //last one
                                {
                                    totalDeaths = deathsRow[i];
                                }
                            }
                            population = double.Parse(stringDeathsRow[firstDataColIX]);
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
                        //doubling rate row calculated from last t days of exponential growth
                        double[] doublingExpRow = (double[])doublingRow.Clone();
                        double[] newCasesRow = (double[])doublingRow.Clone();
                        //reproduction rate comparing raw linear rate over t days of median incubation period
                        double[] reproRateRow = (double[])doublingRow.Clone();
                        //reproduction rate comparing logarithmic growth rate
                        double[] reproRateExpRow = (double[])doublingRow.Clone();
                        List<PointF> points; //for calculating logarithmic growth past (incubationdays) days.
                        List<PointF> pointsLast14;  //for calculating last 14 days slope
                        double m; //slope of log rate
                        double b; //b intercept of log least squares fit
                        double mLast14 = int.MinValue;
                        double bLast14 = int.MinValue;
                        double priorCases = int.MaxValue; //number of cases one incubation period ago
                        double[] newDeathsRow = null; //unless created

                        //calculate
                        newDeathsRow = null;
                        if(deathsRow!= null)
                        {
                            newDeathsRow = new double[deathsRow.Length];
                            newDeathsRow[0] = -1; //first col null
                            for(int i=1; i<deathsRow.Length; i++)
                            {
                                newDeathsRow[i] = deathsRow[i] - deathsRow[i - 1];
                            }
                        }
                        double casesRef = 0; //the number of cases we are looking to double after so many days
                        for (int i = 0; i < casesRow.Length; i++)
                        {
                            //log
                            logRow[i] = casesRow[i] > 0 ? Math.Log10(casesRow[i]) : 0;
                            //new cases
                            if (i > 0)
                            {
                                newCasesRow[i] = casesRow[i] - casesRow[i - 1];
                            }
                            //calculate slope of last 14 days
                            if((i> 13) && (i == newCasesRow.Length -1))
                            {
                                pointsLast14 = new List<PointF>(14);
                                for(int j=0; j<14; j++)
                                {
                                    pointsLast14.Add(new PointF(i - 13 + j, (float)newCasesRow[i - 13 + j]));
                                }
                                MainClass.FindLinearLeastSquaresFit(pointsLast14, out mLast14, out bLast14);
                            }
                            if (i > MainClass.IncubationDays - 1)
                            {



                                //**********************
                                // alternate calculation of doubling time and repro rate during exponential growth



                                //calculate logarithmic growth rate
                                points = new List<PointF>(MainClass.IncubationDays);
                                for(int j=0; j<MainClass.IncubationDays; j++)
                                {
                                    points.Add( new PointF(j, (float)logRow[i - (MainClass.IncubationDays - 1) + j]));
                                }
                                MainClass.FindLinearLeastSquaresFit(points, out m, out b);
                                // per wiki https://en.wikipedia.org/wiki/Basic_reproduction_number
                                // r(zero) = exp (K*tau) where K is logaritmic growth rate we call m and tau is infectivity period we call Incubation days
                                //reproRateExpRow[i] = Math.Exp(m * MainClass.IncubationDays);
                                reproRateExpRow[i] = Math.Pow(10, m * MainClass.IncubationDays);
                                //and doubling time is ln(2)/K
                                //doublingExpRow[i] = Math.Log(2) / m;
                                doublingExpRow[i] = Math.Log10(2) / m;



                                //****************************

                                //repro rate
                                if (i > MainClass.IncubationDays + 1)
                                {
                                    priorCases = (newCasesRow[i - MainClass.IncubationDays - 1] +
                                        newCasesRow[i - MainClass.IncubationDays] +
                                        newCasesRow[i - MainClass.IncubationDays + 1]) / 3;
                                    if (priorCases > 0)
                                    {
                                        reproRateRow[i] = newCasesRow[i] / priorCases;
                                    }
                                }// from if i > incubation days + 1
                            }// from if if i > incubation days - 1


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
                        }// from for each day

                        //write to file
                        using (StreamWriter sw = new StreamWriter(outputFileName,
                               false)) //false to append
                        {
                            string[] datesRow = new string[stringCasesRow.Length - firstDataColIX + 1];
                            Array.Copy(MainClass.CovidDataSet[0], firstDataColIX, datesRow, 1, datesRow.Length - 1);
                            string[] deathDatesRow = null; 
                            if (deathsRow != null)
                            {
                                deathDatesRow = new string[stringDeathsRow.Length - firstDataColIX + 1 - populationColumns];
                                Array.Copy(MainClass.CovidDeathsDataSet[0], firstDataColIX + populationColumns, deathDatesRow, 1, deathDatesRow.Length - 1);
                            }
                            //format
                            string[] strCases = new string[casesRow.Length + 1];
                            string[] strDoubling = new string[casesRow.Length + 1];
                            string[] strLog = new string[casesRow.Length + 1];
                            string[] strNewCases = new string[casesRow.Length + 1];
                            string[] strReproRate = new string[casesRow.Length + 1];
                            string[] strDoublingExp = new string[casesRow.Length + 1];
                            string[] strReproRateExp = new string[casesRow.Length + 1];
                            string[] strDeaths = null;
                            string[] strNewDeaths = null;
                            if (deathsRow != null)
                            {
                                strDeaths = new string[deathsRow.Length + 1];
                                strNewDeaths = new string[newDeathsRow.Length + 1];
                            }

                            strCases[0] = "Cases";
                            strDoubling[0] = "DoublingTime";
                            strLog[0] = "Log";
                            strNewCases[0] = "NewCases";
                            strReproRate[0] = "ReproRate";
                            strDoublingExp[0] = "DoublingExp";
                            strReproRateExp[0] = "ReproRateExp";
                            if(deathsRow != null)
                            {
                                strDeaths[0] = "Deaths";
                                strNewDeaths[0] = "NewDeaths";
                            }
                            for (int i = 0; i < casesRow.Length; i++)
                            {
                                strCases[i + 1] = string.Format(String.Format("{0:0.#}", casesRow[i]));
                                strDoubling[i + 1] = string.Format(String.Format("{0:0.#}", doublingRow[i]));
                                strLog[i + 1] = string.Format(String.Format("{0:0.#}", logRow[i]));
                                strNewCases[i + 1] = string.Format(String.Format("{0:0.#}", newCasesRow[i]));
                                strReproRate[i + 1] = string.Format(String.Format("{0:0.#}", reproRateRow[i]));
                                strDoublingExp[i + 1] = string.Format(String.Format("{0:0.#}", doublingExpRow[i]));
                                strReproRateExp[i + 1] = string.Format(String.Format("{0:0.#}", reproRateExpRow[i]));
                            }
                            if(deathsRow != null)
                            {
                                for (int i=0; i<deathsRow.Length; i++)
                                {
                                    strDeaths[i + 1] = string.Format(String.Format("{0:0.#}", deathsRow[i]));
                                    strNewDeaths[i + 1] = string.Format(String.Format("{0:0.#}", newDeathsRow[i]));
                                }
                            }
                            sw.WriteLine(Wve.WveTools.WriteCsv(datesRow, false, false, false));  // row, last comma, force quotes, newline
                            sw.WriteLine(Wve.WveTools.WriteCsv(strCases, false, false, false));  // row, last comma, force quotes, newline
                            sw.WriteLine(Wve.WveTools.WriteCsv(strDoubling, false, false, false));  // row, last comma, force quotes, newline
                            sw.WriteLine(Wve.WveTools.WriteCsv(strDoublingExp, false, false, false));  // row, last comma, force quotes, newline
                            sw.WriteLine(Wve.WveTools.WriteCsv(strLog, false, false, false));  // row, last comma, force quotes, newline
                            sw.WriteLine(Wve.WveTools.WriteCsv(strNewCases, false, false, false));  // row, last comma, force quotes, newline
                            sw.WriteLine(Wve.WveTools.WriteCsv(strReproRate, false, false, false));  // row, last comma, force quotes, newline
                            sw.WriteLine(Wve.WveTools.WriteCsv(strReproRateExp, false, false, false));  // row, last comma, force quotes, newline
                            if(deathsRow!= null)
                            {
                                sw.WriteLine(Wve.WveTools.WriteCsv(deathDatesRow, false, false, false));  // row, last comma, force quotes, newline
                                sw.WriteLine(Wve.WveTools.WriteCsv(strDeaths, false, false, false));  // row, last comma, force quotes, newline
                                sw.WriteLine(Wve.WveTools.WriteCsv(strNewDeaths, false, false, false));  // row, last comma, force quotes, newline
                            }
                            sw.Flush();
                        }
                        sbResults.Append("Wrote file: ");
                        sbResults.Append(outputFileName);
                        sbResults.Append("\r\n");
                        //MessageBox.Show("wrote file " + outputFileName);

                        //now graphs
                        DataPoint pt;
                        chart1.Series.Clear();
                        chart2.Series.Clear();
                        chart3.Series.Clear();
                        chart4.Series.Clear();
                        chart5.Series.Clear();
                        chart6.Series.Clear();
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
                        for (int i = 1; i < casesRow.Length; i++)
                        {
                            // s.Points.AddXY(i, casesRow[i]);
                            pt = new DataPoint(i, casesRow[i]);
                            pt.AxisLabel = stringHeaderRow[i + firstDataColIX];
                            s.Points.Add(pt);
                        }


                        Series s2 = new Series("log")
                        {
                            ChartType = SeriesChartType.Line,
                            BorderWidth = 3,
                            MarkerStyle = MarkerStyle.Circle,
                            MarkerSize = 3,
                            MarkerColor = Color.Black
                        };
                        //s2.IsVisibleInLegend = false;

                        chart3.Series.Add(s2);
                        for (int i = 1; i < logRow.Length; i++)
                        {
                            //s2.Points.AddXY(i, logRow[i]);
                            pt = new DataPoint(i, logRow[i]);
                            pt.AxisLabel = stringHeaderRow[i + firstDataColIX];
                            s2.Points.Add(pt);
                        }

                        Series s3 = new Series("new cases");
                        s3.ChartType = SeriesChartType.Candlestick;
                        s3.BorderWidth = 3;
                        s3.MarkerStyle = MarkerStyle.Circle;
                        s3.MarkerSize = 3;
                        s3.MarkerColor = Color.Black;
                        //.IsVisibleInLegend = false;

                        chart2.Series.Add(s3);
                        for (int i = 1; i < newCasesRow.Length; i++)
                        {
                            //s3.Points.AddXY(i, newCasesRow[i]);
                            pt = new DataPoint(i, newCasesRow[i]);
                            pt.AxisLabel = stringHeaderRow[i + firstDataColIX];
                            s3.Points.Add(pt);
                        }

                        if ((mLast14 != double.MinValue) && (bLast14 != double.MinValue))
                        {
                            Series sLast14 = new Series("last 14");
                            sLast14.ChartType = SeriesChartType.Line;
                            sLast14.MarkerColor = Color.Red;
                            sLast14.MarkerStyle = MarkerStyle.Circle;
                            chart2.Series.Add(sLast14);
                            //y=mx + b
                            pt = new DataPoint(casesRow.Length - 14, (mLast14 * (casesRow.Length - 14) + bLast14));
                            sLast14.Points.Add(pt);
                            pt = new DataPoint(casesRow.Length - 1, (mLast14 * (casesRow.Length - 1) + bLast14));
                            sLast14.Points.Add(pt);
                        }

                        Series s4 = new Series("doubling time");
                        s4.ChartType = SeriesChartType.Line;
                        s4.BorderWidth = 3;
                        s4.MarkerStyle = MarkerStyle.Circle;
                        s4.MarkerSize = 3;
                        s4.MarkerColor = Color.Black;
                        //s4.IsVisibleInLegend = false;

                        Series s5 = new Series("repro rate");
                        //use defaults
                        //s5.ChartType = SeriesChartType.Line;
                        //s5.BorderWidth = 3;
                        //s5.MarkerStyle = MarkerStyle.Circle;
                        //s5.MarkerSize = 3;
                        //s5.MarkerColor = Color.Black;
                        //s5.IsVisibleInLegend = false;



                        chart4.Series.Add(s4);
                        for (int i = 1; i < doublingRow.Length; i++)
                        {
                            //s4.Points.AddXY(i, doublingRow[i]);
                            //pt = new DataPoint(i, doublingRow[i]);
                            pt = new DataPoint(i, (doublingRow[i] >= 0 && doublingRow[i] <= 29) ? doublingRow[i] : 29);
                            pt.AxisLabel = stringHeaderRow[i + firstDataColIX];
                            s4.Points.Add(pt);
                        }

                        chart4.Series.Add(s5);
                        for (int i = 1; i < doublingRow.Length; i++)
                        {
                            //s5.Points.AddXY(i, reproRateRow[i]);
                            pt = new DataPoint(i, reproRateRow[i]);
                            pt.AxisLabel = stringHeaderRow[i + firstDataColIX];
                            s5.Points.Add(pt);
                        }


                        Series s6 = new Series("doubling exp");
                        s6.ChartType = SeriesChartType.Line;
                        s6.BorderWidth = 3;
                        s6.MarkerStyle = MarkerStyle.Circle;
                        s6.MarkerSize = 3;
                        s6.MarkerColor = Color.Black;
                        //s6.IsVisibleInLegend = false;
                        Series s7 = new Series("repro rate exp");
                        chart5.Series.Add(s6);
                        chart5.Series.Add(s7);
                        for (int i = 1; i < doublingRow.Length; i++)
                        {
                            //display -1 for out of range
                            //s6.Points.AddXY(i, (doublingExpRow[i] >= 0 && doublingExpRow[i] <= 30) ? doublingExpRow[i] : -1);
                            pt = new DataPoint(i, (doublingExpRow[i] >= 0 && doublingExpRow[i] <= 29) ? doublingExpRow[i] : 29);
                            pt.AxisLabel = stringHeaderRow[i + firstDataColIX];
                            s6.Points.Add(pt);
                            //s7.Points.AddXY(i, reproRateExpRow[i]);
                            pt = new DataPoint(i, reproRateExpRow[i]);
                            pt.AxisLabel = stringHeaderRow[i + firstDataColIX];
                            s7.Points.Add(pt);
                        }

                        Series s8 = new Series("new deaths");
                        chart6.Series.Add(s8);
                        if (newDeathsRow != null)
                        {
                            for (int i = 0; i < newDeathsRow.Length; i++)
                            {
                                pt = new DataPoint(i, (newDeathsRow[i] >= 0 ? newDeathsRow[i] : -1));
                                pt.AxisLabel = stringHeaderRow[i + firstDataColIX];// i hope dates are same for deaths as cases??  prob should calcuate separately
                                s8.Points.Add(pt);
                            }
                        }


                        chart1.ChartAreas[0].RecalculateAxesScale();
                        chart2.ChartAreas[0].RecalculateAxesScale();
                        chart3.ChartAreas[0].RecalculateAxesScale();
                        chart4.ChartAreas[0].RecalculateAxesScale();
                        chart5.ChartAreas[0].RecalculateAxesScale();
                        chart6.ChartAreas[0].RecalculateAxesScale();

                        statusStrip1.Items[0].Text = "Calculated, last data date is " + lastDate;
                        labelTitle.Text = comboBoxPlaces.SelectedItem.ToString();
                        sbResults.Append(labelTitle.Text);
                        if(totalDeaths > -1) 
                        {
                            sbResults.Append(": rate ");
                            sbResults.Append(totalDeaths);
                            if (population > 0)
                            {
                                sbResults.Append("/");
                                sbResults.Append(population);
                                sbResults.Append(" = ");
                                sbResults.Append(String.Format("{0:0.#}", (totalDeaths / population * 100000)));
                                sbResults.Append(" deaths/100k");
                            }
                            else
                            {
                                sbResults.Append(" deaths. ");
                            }
                        }
                        sbResults.Append(Environment.NewLine);
                        sbResults.Append(Environment.NewLine);
                        sbResults.Append(_results);
                        _results = sbResults.ToString();
                        TextBoxResults.Text = _results;
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
                            calculateAndDisplay();
                        }
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
                    SettingsForm dlg = new SettingsForm(this);
                    if(dlg.ShowDialog() == DialogResult.OK)
                    {
                        _results = dlg.Log + _results;
                        //TextBoxResults.Text = dlg.Log + TextBoxResults.Text;
                        Form1_Load(sender, e);
                    }
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

        private void textBoxResults_Click(object sender, EventArgs e)
        {
            try
            {
                Wve.TextViewer.ShowText("results...", TextBoxResults.Text);
            }
            catch (Exception er)
            {
                Wve.MyEr.Show(this, er, true);
            }
        }

        private void radioButtonCountries_CheckedChanged(object sender, EventArgs e)
        {
            using (Wve.HourglassCursor waitCursor = new Wve.HourglassCursor())
            {
                try
                {
                    if(((RadioButton)sender).Checked)
                    {
                        MainClass.TypeOfData = ((RadioButton)sender).Text;
                        //reload
                        buttonRefresh_Click(sender, e);
                    }
                }
                catch (Exception er)
                {
                    Wve.MyEr.Show(this, er, true);
                }
            }
        }

        /// <summary>
        /// download data
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonDownload_Click(object sender, EventArgs e)
        {
            using (Wve.HourglassCursor waitCursor = new Wve.HourglassCursor())
            {
                try
                {
                    //get data
                    string url = string.Empty;
                    string outputFileName = string.Empty;
                    string urlDeaths = string.Empty;
                    string outputDeathsFileName = string.Empty;
                    //load countries, then counties
                    for (int i = 0; i < 2; i++)
                    {
                        if (i == 0)
                        {
                            url = MainClass.DataUrlForCountries;
                            outputFileName = MainClass.DataFileNameforCountries;
                            urlDeaths = MainClass.DataUrlForCountriesDeaths;
                            outputDeathsFileName = MainClass.DataFileNameForCountriesDeaths;
                        }
                        else if (i == 1)
                        {
                            url = MainClass.DataUrlForStates;
                            outputFileName = MainClass.DataFileNameForStates;
                            urlDeaths = MainClass.DataUrlForStatesDeaths;
                            outputDeathsFileName = MainClass.DataFileNameForStatesDeaths;
                        }

                        string dataRaw = MainClass.SendRequest("GET", "", null, url);
                        string dataRawDeaths = MainClass.SendRequest("GET", "", null, urlDeaths);
                        //write to files
                        DirectoryInfo di = new DirectoryInfo(@"c:\covid19");
                        if (!di.Exists)
                        {
                            di.Create();
                        }
                        //rename old data if exists
                        FileInfo fi = new FileInfo(outputFileName);
                        if (fi.Exists)
                        {
                            if (File.Exists(outputFileName + ".old"))
                            {
                                File.Delete(outputFileName + ".old");
                            }
                            fi.MoveTo(outputFileName + ".old");
                        }
                        fi = new FileInfo(outputDeathsFileName);
                        if (fi.Exists)
                        {
                            if (File.Exists(outputDeathsFileName + ".old"))
                            {
                                File.Delete(outputDeathsFileName + ".old");
                            }
                            fi.MoveTo(outputDeathsFileName + ".old");
                        }
                        //write to files
                        using (StreamWriter sw = new StreamWriter(outputFileName,
                               false)) //false to append
                        {
                            sw.Write(dataRaw);
                            sw.Flush();
                        }
                        using (StreamWriter sw = new StreamWriter(outputDeathsFileName,
                               false)) //false to append
                        {
                            sw.Write(dataRawDeaths);
                            sw.Flush();
                        }
                        StringBuilder sbLog = new StringBuilder();
                        sbLog.Append("Downloaded ");
                        sbLog.Append(outputFileName);
                        sbLog.Append(" and ");
                        sbLog.Append(outputDeathsFileName);
                        sbLog.Append("\r\n\r\n");
                        _results = sbLog.ToString() + _results;
                        //this.Log = this.Log + sbLog.ToString();
                        //MessageBox.Show("Downloaded " + outputFileName + " and " + outputDeathsFileName);
                    }//for i
                     //show new results
                    Form1_Load(sender, e);
                }
                catch (Exception er)
                {
                    Wve.MyEr.Show(this, er, true);
                }
            }
        }
    }
}
