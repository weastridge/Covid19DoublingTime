using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

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
        /// name of the datafile for US
        /// </summary>
        internal static string DataFileNameForStates = @"c:\covid19\time_series_covid19_confirmed_US.csv";
        /// <summary>
        /// default name, with XXX to be replaced by place name
        /// </summary>
        internal static string DefaultOutputName = @"c:\Covid19\doublingForXXX.csv";
        /// <summary>
        /// the data array
        /// </summary>
        internal static string[][] CovidDataSet;
        /// <summary>
        /// incubation period in days
        /// </summary>
        internal static int IncubationDays = 5; //1-11 but commonly 5 per WHO
        /// <summary>
        /// e.g. Hopkins_World, Hopkins_US
        /// </summary>
        internal static string TypeOfData = "Hopkins_World"; //by default


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
        }
    }
}
