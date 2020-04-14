using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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
    }
}
