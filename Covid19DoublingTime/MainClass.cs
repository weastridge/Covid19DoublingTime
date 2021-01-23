using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Covid19DoublingTime
{
    /// <summary>
    /// main class
    /// </summary>
    internal static class MainClass
    {
        /// <summary>
        /// name of the data file for global
        /// </summary>
        internal static string DataFileNameforCountries = @"c:\Covid19\time_series_covid19_confirmed_global.csv";
        /// <summary>
        /// url to get data from for global
        /// </summary>
        internal static string DataUrlForCountries = 
            "https://github.com/CSSEGISandData/COVID-19/raw/master/csse_covid_19_data/csse_covid_19_time_series/time_series_covid19_confirmed_global.csv";
        /// <summary>
        /// name of data file for global deaths
        /// </summary>
        internal static string DataFileNameForCountriesDeaths = @"c:\Covid19\time_series_covid19_confirmed_global_deaths.csv";
        /// <summary>
        /// url to get data file for global deaths
        /// </summary>
        internal static string DataUrlForCountriesDeaths =
            @"https://raw.githubusercontent.com/CSSEGISandData/COVID-19/master/csse_covid_19_data/csse_covid_19_time_series/time_series_covid19_deaths_global.csv";
        /// <summary>
        /// name of the datafile for US
        /// </summary>
        internal static string DataFileNameForStates = @"c:\covid19\time_series_covid19_confirmed_US.csv";
        /// <summary>
        /// url to get data for US
        /// </summary>
        internal static string DataUrlForStates = 
            "https://github.com/CSSEGISandData/COVID-19/raw/master/csse_covid_19_data/csse_covid_19_time_series/time_series_covid19_confirmed_US.csv";
        /// <summary>
        /// name of the datafile for US deaths
        /// </summary>
        internal static string DataFileNameForStatesDeaths = @"c:\covid19\time_series_covid19_confirmed_US_deaths.csv";
        /// <summary>
        /// url to get data for US deaths
        /// </summary>
        internal static string DataUrlForStatesDeaths =
            @"https://raw.githubusercontent.com/CSSEGISandData/COVID-19/master/csse_covid_19_data/csse_covid_19_time_series/time_series_covid19_deaths_US.csv";
        /// <summary>
        /// default name, with XXX to be replaced by place name
        /// </summary>
        internal static string DefaultOutputName = @"c:\Covid19\doublingForXXX.csv";
        /// <summary>
        /// the data array
        /// </summary>
        internal static string[][] CovidDataSet;
        /// <summary>
        /// the data array for deaths
        /// </summary>
        internal static string[][] CovidDeathsDataSet;
        /// <summary>
        /// data array for world population
        /// </summary>
        internal static string[][] WorldPopulationDataSet = null;
        /// <summary>
        /// name of population file that should be installed by this program inititally
        /// I think I initially downloaded it from WorldOMeters but not sure
        /// </summary>
        internal static string WorldPopulationDataSetFilename = @"c:\Covid19\WorldPop20200428.csv";
        /// <summary>
        /// incubation period in days
        /// </summary>
        internal static int IncubationDays = 5; //1-11 but commonly 5 per WHO
        /// <summary>
        /// e.g. Hopkins_World, Hopkins_US
        /// </summary>
        internal static string TypeOfData = "Hopkins_US"; // default


        #region methods
        /// <summary>
        /// send http request
        /// </summary>
        /// <param name="postOrGet">either POST or GET</param>
        /// <param name="bodyString"></param>
        /// <param name="headers"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string SendRequest(string postOrGet,
            string bodyString,
            NameValueCollection headers,
            string url)
        {
            if (url.StartsWith("http://") ||
                url.StartsWith("https://"))
            {
                HttpWebRequest myHttpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                if ((headers != null) &&
                    (headers.Count > 0))
                {
                    myHttpWebRequest.Headers.Add(headers);
                }


                //post data
                myHttpWebRequest.Method = postOrGet;
                ////// Set the ContentType property of the WebRequest.  This seems 
                //to be required; anything else says bad media type
                //Usually the content type is application/x-www-form-urlencoded, so the request body uses
                //the same format as the query string:
                //parameter = value & also = another
                myHttpWebRequest.ContentType = "application/x-www-form-urlencoded";
                //media type
                //myHttpWebRequest.MediaType = "application/json";
                if (!string.IsNullOrEmpty(bodyString))
                {

                    // Create POST data and convert it to a byte array.
                    if (bodyString == null)
                        bodyString = string.Empty;
                    byte[] byteArray = Encoding.UTF8.GetBytes(bodyString);

                    // Set the ContentLength property of the WebRequest.
                    myHttpWebRequest.ContentLength = byteArray.Length;

                    // Get the request stream.
                    using (Stream postDataStream = myHttpWebRequest.GetRequestStream())
                    {
                        // Write the data to the request stream.
                        postDataStream.Write(byteArray, 0, byteArray.Length);
                        postDataStream.Flush();
                    }//postDataStream closes here
                }//from if not null post string


                string result; // = string.Empty;
                StringBuilder sb = new StringBuilder();
                using (HttpWebResponse myHttpWebResponse = (HttpWebResponse)myHttpWebRequest.GetResponse())
                {
                    //debug:  Console.WriteLine("\nThe HttpHeaders are \n\n\tName\t\tValue\n{0}", myHttpWebRequest.Headers);
                    // Print the HTML contents of the page to the string. 
                    using (Stream streamResponse = myHttpWebResponse.GetResponseStream())
                    {
                        using (StreamReader streamRead = new StreamReader(streamResponse))
                        {
                            Char[] readBuff = new Char[256];
                            int count = streamRead.Read(readBuff, 0, 256);
                            while (count > 0)
                            {
                                String outputData = new String(readBuff, 0, count);
                                sb.Append(outputData);
                                count = streamRead.Read(readBuff, 0, 256);
                            }
                        }//from using streamRead
                    }//from using streamResponse
                }//from using myHttpWebResponse
                result = sb.ToString();
                return result;
            }
            else
            {
                throw new Exception(" Error posting request:  " +
                    "address must start with http(s)");
            }
        }

        //least squares functions:  Find a linear least squares fit for a set of points in C#
        //Posted on October 30, 2014 by Rod Stephens
        // http://csharphelper.com/blog/2014/10/find-a-linear-least-squares-fit-for-a-set-of-points-in-c/

        /// <summary>
        /// Return the error squared.
        /// </summary>
        /// <param name="points"></param>
        /// <param name="m"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static double ErrorSquared(List<PointF> points,
            double m, double b)
        {
            double total = 0;
            foreach (PointF pt in points)
            {
                double dy = pt.Y - (m * pt.X + b);
                total += dy * dy;
            }
            return total;
        }
        /// <summary>
        /// Find the least squares linear fit. Return the total error.
        /// </summary>
        /// <param name="points"></param>
        /// <param name="m"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static double FindLinearLeastSquaresFit(
            List<PointF> points, out double m, out double b)
        {
            // Perform the calculation.
            // Find the values S1, Sx, Sy, Sxx, and Sxy.
            double S1 = points.Count;
            double Sx = 0;
            double Sy = 0;
            double Sxx = 0;
            double Sxy = 0;
            foreach (PointF pt in points)
            {
                Sx += pt.X;
                Sy += pt.Y;
                Sxx += pt.X * pt.X;
                Sxy += pt.X * pt.Y;
            }

            // Solve for m and b.
            m = (Sxy * S1 - Sx * Sy) / (Sxx * S1 - Sx * Sx);
            b = (Sxy * Sx - Sy * Sxx) / (Sx * Sx - S1 * Sxx);

            return Math.Sqrt(ErrorSquared(points, m, b));
        }

        #endregion methods


        /// <summary>
        /// places to select from
        /// </summary>
        internal class DataPlace
        {
            /// <summary>
            /// e.g. state or province of country, or county of state
            /// </summary>
            internal string SubPlace;
            /// <summary>
            /// nation or state
            /// </summary>
            internal string Place;
            /// <summary>
            /// index in data set
            /// </summary>
            internal int Index;
            /// <summary>
            /// population, if known
            /// </summary>
            internal int Population = int.MinValue;
            /// <summary>
            /// 
            /// </summary>
            /// <param name="subPlace"></param>
            /// <param name="place"></param>
            /// <param name="index"></param>
            internal DataPlace(string subPlace, string place, int index)
            {
                SubPlace = subPlace;
                Place = place;
                Index = index;
            }
            /// <summary>
            /// show subplace, place
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                //sb.Append(Index.ToString());
                //sb.Append(": ");
                sb.Append(SubPlace);
                sb.Append(string.IsNullOrEmpty(SubPlace) ? "" : ", ");
                sb.Append(Place);
                return sb.ToString();
            }
        } //data place

        /// <summary>
        /// class to make aggregate data line for Tricities area covered by Ballad
        /// </summary>
        public class AggregateBallad
        {
            //will have method to parce CovidDataSet and CovidDeathsDataSet and append a line for Ballad
            Tuple<string, string>[] locations = new Tuple<string, string>[]
            {
                new Tuple<string, string>("Lee", "Virginia"),
                new Tuple<string, string>("Wise", "Virginia"),
                new Tuple<string, string>("Scott", "Virginia"),
                new Tuple<string, string>("Dickenson", "Virginia"),
                new Tuple<string, string>("Buchanan", "Virginia"),
                new Tuple<string, string>("Russell", "Virginia"),
                new Tuple<string, string>("Washington", "Virginia"),
                new Tuple<string, string>("Tazewell", "Virginia"),
                new Tuple<string, string>("Smyth", "Virginia"),
                new Tuple<string, string>("Bristol", "Virginia"),//added 9/9
                //new Tuple<string, string>("Bland", "Virginia"),
                //new Tuple<string, string>("Wythe", "Virginia"),
                //new Tuple<string, string>("Grayson", "Virginia"),
                new Tuple<string, string>("Johnson", "Tennessee"),
                new Tuple<string, string>("Sullivan", "Tennessee"),
                new Tuple<string, string>("Carter", "Tennessee"),
                new Tuple<string, string>("Hawkins", "Tennessee"),
                new Tuple<string, string>("Washington", "Tennessee"),
                new Tuple<string, string>("Unicoi", "Tennessee"),
                new Tuple<string, string>("Greene", "Tennessee"),
                new Tuple<string, string>("Hancock", "Tennessee")
                //new Tuple<string, string>("Hamblen", "Tennessee"),
            };

                ///// <summary>
                ///// the data rows we parce and append to
                ///// </summary>
                //private string[][] _rows;
                ///// <summary>
                ///// index of col containing the place (e.g. country or state)
                ///// </summary>
                //private int _placeIX = int.MinValue;
                ///// <summary>
                ///// index of col containing subplace (e.g. city or county)
                ///// </summary>
                //private int _subPlaceIX = int.MinValue;

                //#region constructors
                //public AggregateBallad(ref string[][] rows, int placeIX, int subPlaceIX)
                //{
                //    _rows = rows;
                //    _placeIX = placeIX;
                //    _subPlaceIX = subPlaceIX;
                //}
                //#endregion constructors

                /// <summary>
                /// parse rows to make an aggregate row of the rows containing matching place and subPlace 
                /// and append that aggregate row to the array of rows, then return the number of matching
                /// rows found.
                /// </summary>
                /// <param name="placeIX">index of col containing place (eg country or state) or int.MinValue to match all</param>
                /// <param name="rows">the data rows we parce and append the aggregate row to</param>
                /// <param name="subPlaceIX">index of col contining the subplace (eg city or county) or int.MinValue to match all</param>
                /// <param name="firstDataColIX">index of first column containing counts data</param>
                /// <returns></returns>
                public int AppendAggregate(ref string[][] rows,
                int placeIX, 
                int subPlaceIX,
                int firstDataColIX)
            {
                int rowsFound = 0;
                bool foundMatch = false;
                double[] aggregateNumbers = new double[rows[0].Length - firstDataColIX];
                //doubles should have initialized as zero but leave nothing to chance
                for(int i=0; i<aggregateNumbers.Length; i++)
                {
                    aggregateNumbers[i] = 0;
                }
                string[] aggregateRow = new string[rows[0].Length];
                aggregateRow[placeIX] = "Ballad Area";
                aggregateRow[subPlaceIX] = string.Empty;

                for (int i = 0; i < rows.Length; i++)
                {
                    foundMatch = false;
                    for (int j = 0; j < locations.Length; j++)
                    {
                        if ((subPlaceIX == int.MinValue) || ((rows[i][subPlaceIX]).Trim().ToLower() == locations[j].Item1.Trim().ToLower()))
                        {
                            if ((placeIX == int.MinValue) || (rows[i][placeIX].Trim().ToLower() == locations[j].Item2.Trim().ToLower()))
                            {
                                foundMatch = true;
                                rowsFound++;
                                break;
                            }
                        }
                    }
                    if(foundMatch)
                    {
                        //add that row's numbers to aggregate
                        double x;
                        for (int j=0; j<aggregateNumbers.Length; j++)
                        {
                            if(double.TryParse(rows[i][j+firstDataColIX], out x))
                            {
                                aggregateNumbers[j] += x;
                            }
                            else
                            {
                                //debugging
                                MessageBox.Show("oops, couldn't parse in row " + i.ToString() + " col " + (j + firstDataColIX).ToString());
                            }
                        }
                        //got here
                    }//from if found match
                }//from for each row
                //now append the new aggreegate row
                for(int j=0; j<aggregateNumbers.Length; j++)
                {
                    aggregateRow[j + firstDataColIX] = aggregateNumbers[j].ToString();
                }
                string[][] result = new string[rows.Length + 1][];
                for(int i=0; i<rows.Length; i++)
                {
                    result[i] = rows[i];
                }
                result[rows.Length] = aggregateRow;
                rows = result;
                return rowsFound;
            }
        }//end class
        public class AggregateHolston
        {

            //will have method to parce CovidDataSet and CovidDeathsDataSet and append a line for Holston
            //list comes from https://www.holston.org/plans-for-in-person-worship
            Tuple<string, string>[] locations = new Tuple<string, string>[]
            {
                    //ballad va
                new Tuple<string, string>("Lee", "Virginia"),
                new Tuple<string, string>("Wise", "Virginia"),
                new Tuple<string, string>("Scott", "Virginia"),
                new Tuple<string, string>("Dickenson", "Virginia"),
                new Tuple<string, string>("Buchanan", "Virginia"),
                new Tuple<string, string>("Russell", "Virginia"),
                new Tuple<string, string>("Washington", "Virginia"),
                new Tuple<string, string>("Tazewell", "Virginia"),
                new Tuple<string, string>("Smyth", "Virginia"),
                new Tuple<string, string>("Bristol", "Virginia"),
                //ballad border:
                new Tuple<string, string>("Bland", "Virginia"),
                new Tuple<string, string>("Wythe", "Virginia"),
                new Tuple<string, string>("Grayson", "Virginia"),
                //ballad tn
                new Tuple<string, string>("Johnson", "Tennessee"),
                new Tuple<string, string>("Sullivan", "Tennessee"),
                new Tuple<string, string>("Carter", "Tennessee"),
                new Tuple<string, string>("Hawkins", "Tennessee"),
                new Tuple<string, string>("Washington", "Tennessee"),
                new Tuple<string, string>("Unicoi", "Tennessee"),
                new Tuple<string, string>("Greene", "Tennessee"),
                new Tuple<string, string>("Hancock", "Tennessee"),
                //ballad border:
                new Tuple<string, string>("Hamblen", "Tennessee"),
                //appalachian all in ballad
                //clinch mountain all in ballad
                    //hiwassee
                new Tuple<string, string>("Bradley", "Tennessee"),
                new Tuple<string, string>("McMinn", "Tennessee"),
                new Tuple<string, string>("Meigs", "Tennessee"),
                new Tuple<string, string>("Monroe", "Tennessee"),
                new Tuple<string, string>("Polk", "Tennessee"),
                new Tuple<string, string>("Rhea", "Tennessee"),
                //mountain view
                new Tuple<string, string>("Claiborne", "Tennessee"),
                new Tuple<string, string>("Cocke", "Tennessee"),
                new Tuple<string, string>("Grainger", "Tennessee"),
                //greene
                //hamblen
                //hancock
                //hawkins
                new Tuple<string, string>("Jefferson", "Tennessee"),
                new Tuple<string, string>("Sevier", "Tennessee"),
                // new river
                //bland
                new Tuple<string, string>("Carroll", "Virginia"),
                new Tuple<string, string>("Floyd", "Virginia"),
                new Tuple<string, string>("Giles", "Virginia"),
                //grayson
                new Tuple<string, string>("Montgomery", "Virginia"),
                new Tuple<string, string>("Pulaski", "Virginia"),
                //tazewell
                //wythe
                new Tuple<string, string>("Galax", "Virginia"),
                new Tuple<string, string>("Radford", "Virginia"),
                //scenic south
                new Tuple<string, string>("Catoosa", "Georgia"),
                new Tuple<string, string>("Dade", "Georgia"),
                new Tuple<string, string>("Walker", "Georgia"),
                new Tuple<string, string>("Bledsoe", "Tennessee"),
                new Tuple<string, string>("Hamilton", "Tennessee"),
                new Tuple<string, string>("Marion", "Tennessee"),
                new Tuple<string, string>("Sequatchie", "Tennessee"),
                //smoky mountain
                new Tuple<string, string>("Blount", "Tennessee"),
                new Tuple<string, string>("Knox", "Tennessee"),
                new Tuple<string, string>("Loudon", "Tennessee"),
                new Tuple<string, string>("Monroe", "Tennessee"),
                new Tuple<string, string>("Roane", "Tennessee"),
                new Tuple<string, string>("Sevier", "Tennessee"),
                //tennessee valley
                new Tuple<string, string>("Anderson", "Tennessee"),
                new Tuple<string, string>("Campbell", "Tennessee"),
                new Tuple<string, string>("Claiborne", "Tennessee"),
                //knox again
                //loudon again
                new Tuple<string, string>("Morgan", "Tennessee"),
                //roane again
                new Tuple<string, string>("Scott", "Tennessee"),
                new Tuple<string, string>("Union", "Tennessee")
                //three rivers all in ballad
            };


            /// <summary>
            /// parse rows to make an aggregate row of the rows containing matching place and subPlace 
            /// and append that aggregate row to the array of rows, then return the number of matching
            /// rows found.
            /// </summary>
            /// <param name="placeIX">index of col containing place (eg country or state) or int.MinValue to match all</param>
            /// <param name="rows">the data rows we parce and append the aggregate row to</param>
            /// <param name="subPlaceIX">index of col contining the subplace (eg city or county) or int.MinValue to match all</param>
            /// <param name="firstDataColIX">index of first column containing counts data</param>
            /// <returns></returns>
            public int AppendAggregate(ref string[][] rows,
            int placeIX,
            int subPlaceIX,
            int firstDataColIX)
            {
                int rowsFound = 0;
                bool foundMatch = false;
                double[] aggregateNumbers = new double[rows[0].Length - firstDataColIX];
                //doubles should have initialized as zero but leave nothing to chance
                for (int i = 0; i < aggregateNumbers.Length; i++)
                {
                    aggregateNumbers[i] = 0;
                }
                string[] aggregateRow = new string[rows[0].Length];
                aggregateRow[placeIX] = "Holston Conference";
                aggregateRow[subPlaceIX] = string.Empty;

                for (int i = 0; i < rows.Length; i++)
                {
                    foundMatch = false;
                    for (int j = 0; j < locations.Length; j++)
                    {
                        if ((subPlaceIX == int.MinValue) || ((rows[i][subPlaceIX]).Trim().ToLower() == locations[j].Item1.Trim().ToLower()))
                        {
                            if ((placeIX == int.MinValue) || (rows[i][placeIX].Trim().ToLower() == locations[j].Item2.Trim().ToLower()))
                            {
                                foundMatch = true;
                                rowsFound++;
                                break;
                            }
                        }
                    }
                    if (foundMatch)
                    {
                        //add that row's numbers to aggregate
                        double x;
                        for (int j = 0; j < aggregateNumbers.Length; j++)
                        {
                            if (double.TryParse(rows[i][j + firstDataColIX], out x))
                            {
                                aggregateNumbers[j] += x;
                            }
                            else
                            {
                                //debugging
                                MessageBox.Show("oops, couldn't parse in row " + i.ToString() + " col " + (j + firstDataColIX).ToString());
                            }
                        }
                        //got here
                    }//from if found match
                }//from for each row
                //now append the new aggreegate row
                for (int j = 0; j < aggregateNumbers.Length; j++)
                {
                    aggregateRow[j + firstDataColIX] = aggregateNumbers[j].ToString();
                }
                string[][] result = new string[rows.Length + 1][];
                for (int i = 0; i < rows.Length; i++)
                {
                    result[i] = rows[i];
                }
                result[rows.Length] = aggregateRow;
                rows = result;
                return rowsFound;
            }
        }
    }
}
