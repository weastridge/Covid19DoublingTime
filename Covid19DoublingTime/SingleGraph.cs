using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace Covid19DoublingTime
{
    /// <summary>
    /// show single graph
    /// </summary>
    public partial class SingleGraph : Form
    {
        System.Windows.Forms.DataVisualization.Charting.Chart _chartToView;
        string _title;
        /// <summary>
        /// show single graph
        /// </summary>
        public SingleGraph(ref Chart chartToView, string title)
        {
            _chartToView = chartToView;
            _title = title;
            InitializeComponent();
        }

        private void SingleGraph_Load(object sender, EventArgs e)
        {
            using (Wve.HourglassCursor waitCursor = new Wve.HourglassCursor())
            {
                try
                {
                    labelTitle.Text = _title;
                    chart1.Series.Clear();
                    for(int i=0; i<_chartToView.Series.Count; i++)
                    {
                        chart1.Series.Add(_chartToView.Series[i]);
                    }
                    //chart1.Update();
                    chart1.Show();
                    //chart1.ChartAreas[0].RecalculateAxesScale();
                }
                catch (Exception er)
                {
                    Wve.MyEr.Show(this, er, true);
                }
            }
        }

        private void SingleGraph_FormClosing(object sender, FormClosingEventArgs e)
        {
            using (Wve.HourglassCursor waitCursor = new Wve.HourglassCursor())
            {
                try
                {
                    //sorry, but we can't allow chart to be closed because for some unknown
                    // reason, whenever the form is closed, any subsequent drawing of the
                    // same chart causes an Internal List error within DataVisualization...
                    MessageBox.Show("Chart will close when the main program is closed, " +
                        "but can hide the box now." );
                    e.Cancel = true;
                }
                catch (Exception er)
                {
                    Wve.MyEr.Show(this, er, true);
                }
            }
        }

        private void buttonHide_Click(object sender, EventArgs e)
        {
            try
            {
                this.WindowState = FormWindowState.Minimized;
            }
            catch (Exception er)
            {
                Wve.MyEr.Show(this, er, true);
            }
        }
    }
}
