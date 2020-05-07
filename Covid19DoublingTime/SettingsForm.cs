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

namespace Covid19DoublingTime
{
    /// <summary>
    /// settings
    /// </summary>
    public partial class SettingsForm : Form
    {
        /// <summary>
        /// to temporarily ignore control changes during loading etc
        /// </summary>
        bool _ignoreChangeEvents = false;
        public SettingsForm()
        {
            InitializeComponent();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            using (Wve.HourglassCursor waitCursor = new Wve.HourglassCursor())
            {
                try
                {
                    if((!_ignoreChangeEvents) && (listBoxTypeOfData.SelectedItem != null))
                    {
                        MainClass.TypeOfData = listBoxTypeOfData.SelectedItem.ToString();
                        //reload main form
                        //Form1.ReloadForm1(sender, e);
                    }
                }
                catch (Exception er)
                {
                    Wve.MyEr.Show(this, er, true);
                }
            }
        }
        /// <summary>
        /// load
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SettingsForm_Load(object sender, EventArgs e)
        {
            using (Wve.HourglassCursor waitCursor = new Wve.HourglassCursor())
            {
                try
                {
                    _ignoreChangeEvents = true;
                    listBoxTypeOfData.Items.Add("Hopkins_World");
                    listBoxTypeOfData.Items.Add("Hopkins_US");

                    for(int i=0; i<listBoxTypeOfData.Items.Count; i++)
                    {
                        if(listBoxTypeOfData.Items[i].ToString().Trim() == 
                            MainClass.TypeOfData.Trim())
                        {
                            listBoxTypeOfData.SelectedIndex = i;
                        }
                    }

                }
                catch (Exception er)
                {
                    Wve.MyEr.Show(this, er, true);
                }
                finally
                {
                    _ignoreChangeEvents = false;
                }
            }
        }

        private void buttonDownload_Click(object sender, EventArgs e)
        {
            try
            {
                //get data
                string url = string.Empty;
                string outputFileName = string.Empty;
                string urlDeaths = string.Empty;
                string outputDeathsFileName = string.Empty;
                switch (MainClass.TypeOfData)
                {
                    case "Hopkins_World":
                        url = MainClass.DataUrlForCountries;
                        outputFileName = MainClass.DataFileNameforCountries;
                        urlDeaths = MainClass.DataUrlForCountriesDeaths;
                        outputDeathsFileName = MainClass.DataFileNameForCountriesDeaths;
                        break;
                    case "Hopkins_US":
                        url = MainClass.DataUrlForStates;
                        outputFileName = MainClass.DataFileNameForStates;
                        urlDeaths = MainClass.DataUrlForStatesDeaths;
                        outputDeathsFileName = MainClass.DataFileNameForStatesDeaths;
                        break;
                }
                string dataRaw = MainClass.SendRequest("GET", "", null, url);
                string dataRawDeaths = MainClass.SendRequest("GET", "", null, urlDeaths);
                //write to files
                DirectoryInfo di = new DirectoryInfo(@"c:\covid19");
                if(!di.Exists)
                {
                    di.Create();
                }
                //rename old data if exists
                FileInfo fi = new FileInfo(outputFileName);
                if(fi.Exists)
                {
                    if(File.Exists(outputFileName + ".old"))
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
                MessageBox.Show("Downloaded " + outputFileName  + " and " + outputDeathsFileName);
            }
            catch (Exception er)
            {
                Wve.MyEr.Show(this, er, true);
            }
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }
    }
}
