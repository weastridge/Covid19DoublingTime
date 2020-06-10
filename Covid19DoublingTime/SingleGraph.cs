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
        public SingleGraph(Chart chartToView, string title)
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
                    chart1.ChartAreas[0].RecalculateAxesScale();
                }
                catch (Exception er)
                {
                    Wve.MyEr.Show(this, er, true);
                }
            }
        }
    }
}
