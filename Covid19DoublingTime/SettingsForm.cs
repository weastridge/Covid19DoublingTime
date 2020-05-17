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
        Form1 _form1Ref = null;
        public SettingsForm(Form1 form1ref)
        {
            InitializeComponent();
            _form1Ref = form1ref;
        }
        /// <summary>
        /// to return results to caller
        /// </summary>
        internal string Log = string.Empty;

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            
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
                        this.Log = this.Log + sbLog.ToString();
                        //MessageBox.Show("Downloaded " + outputFileName + " and " + outputDeathsFileName);
                    }//for i
                     //close form
                    buttonClose_Click(sender, e);
                }
                catch (Exception er)
                {
                    Wve.MyEr.Show(this, er, true);
                }
            }//using
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }

        private void listBoxTypeOfData_SelectedIndexChanged(object sender, EventArgs e)
        {
            using (Wve.HourglassCursor waitCursor = new Wve.HourglassCursor())
            {
                try
                {
                    if ((!_ignoreChangeEvents) && (listBoxTypeOfData.SelectedItem != null))
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
    }
}
