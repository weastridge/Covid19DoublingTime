using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Covid19DoublingTime
{
    public partial class AboutForm : Form
    {
        public AboutForm()
        {
            InitializeComponent();
        }

        private void AboutForm_Load(object sender, EventArgs e)
        {
            using (Wve.HourglassCursor waitCursor = new Wve.HourglassCursor())
            {
                try
                {

                    StringBuilder sb = new StringBuilder();
                    sb.Append("To calculate and display the doubling rate of Covid-19 cases in the U.S. as a function of time \r\n" +

                    "Data comes from Johns Hopkins Coronavirus COVID - 19 Global Cases by the Center for Systems Science and Engineering(CSSE) at Johns Hopkins University(JHU) \r\n" +
                    "  https://www.arcgis.com/apps/opsdashboard/index.html#/bda7594740fd40299423467b48e9ecf6 \r\n" +
                    "and in particular, from their daily updated dataset at github \r\n " +
                    " https://github.com/CSSEGISandData/COVID-19/tree/master/csse_covid_19_data/csse_covid_19_time_series \r\n");
                    // was "  https://github.com/CSSEGISandData/COVID-19/tree/master/csse_covid_19_data \r\n");

                    sb.Append(Environment.NewLine);
                    sb.Append(Environment.NewLine);
                    sb.Append(" The global data must be saved with the name ");
                    sb.Append(MainClass.DataFileNameforCountries);
                    sb.Append(".");

                    sb.Append("\r\n\r\n");
                    sb.Append("This is a work in progress.  I mainly wanted to see the doubling time, ");
                    sb.Append("that is, the number of days it took to reach the cases total from half the total.");
                    sb.Append("\r\nAlso interesting is the spread rate, i.e. the number of people each case spreads to.  ");
                    sb.Append("I don't have a sophisticated way to calculate that.  I just took the median incubation ");
                    sb.Append("period of 5 days and divided each daily new cases by the new cases 5 days prior.  ");
                    sb.Append("If somebody knows a better way, please let me know.  ");
                    sb.Append("\r\nThe hope is, if we can keep that spread rate below 1, the infection dies out. ");

                    sb.Append(Environment.NewLine);
                    sb.Append(Environment.NewLine);
                    sb.Append("Source code is at GitHub at \r\n");
                    sb.Append("https://github.com/weastridge/Covid19DoublingTime.git");

                    sb.Append("\r\n\r\nwesley@eastridges.com");

                    textBoxDisplay.Text = sb.ToString();
                    //Wve.TextViewer.ShowText("About this program...", sb.ToString());

                    //show assembly version
                    Assembly asm = Assembly.GetAssembly(this.GetType());
                    StringBuilder sbVersion = new StringBuilder();
                    sbVersion.Append("Version: ");
                    sbVersion.Append(System.Reflection.Assembly.GetEntryAssembly()
                    .GetName().Version.ToString());
                    //sbVersion.Append("  Release:1   (mm3 ");
                    //sbVersion.Append(asm.GetName().Version.ToString());
                    //sbVersion.Append(")");
                    labelVersion.Text = sbVersion.ToString();

                    //show build date if version format supports it 
                    //  (i.e. build and revision numbers indicate date and time)
                    StringBuilder sbBuild = new StringBuilder();
                    DateTime buildDate = DateTime.Parse("2000/01/01").AddDays(
                        asm.GetName().Version.Build);
                    DateTime buildTime = DateTime.Today.AddSeconds(
                        asm.GetName().Version.Revision * 2);
                    if ((asm.GetName().Version.Build != 0) &&
                        (buildDate < DateTime.Parse("2100/01/01")))
                    {
                        sbBuild.Append("Built on ");
                        sbBuild.Append(buildDate.ToLongDateString());
                        sbBuild.Append(" at ");
                        sbBuild.Append(buildTime.ToShortTimeString());
                        sbBuild.Append(" Standard Time");
                    } // testin subversion -chris
                    else
                    {
                        //unreasonable build date, so don't show it
                    }
                    labelBuildDate.Text = sbBuild.ToString();
                    //take focus away from textbox
                    textBoxDisplay.SelectionStart = 0;
                    textBoxDisplay.SelectionLength = 0;
                    buttonOK.Focus();
                }
                catch (Exception er)
                {
                    Wve.MyEr.Show(this, er, true);
                }
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }
    }
}
