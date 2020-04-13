using System;
using System.Collections.Generic;
using System.Linq;
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
        /// name of the data file
        /// </summary>
        internal static string DataFileName = @"c:\Covid19\time_series_covid19_confirmed_global.csv";
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
