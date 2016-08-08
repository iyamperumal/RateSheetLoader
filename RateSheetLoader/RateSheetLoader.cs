using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using OleDbLoader;

namespace RateSheetLoader
{
    public partial class RateSheetLoader : Form
    {
        private string ExecutingPath;

        public RateSheetLoader()
        {
            InitializeComponent();

            this.ExecutingPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase).Replace("file:\\", "") + "\\Data";
            if (!Directory.Exists(this.ExecutingPath))
                Directory.CreateDirectory(this.ExecutingPath);
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            lblStatus.Text = string.Empty;
            OpenFileDialog openRateSheetFileDialog = new OpenFileDialog();
            openRateSheetFileDialog.FileName = "*.xls;*.xlsx";
            openRateSheetFileDialog.FilterIndex = 1;
            openRateSheetFileDialog.RestoreDirectory = true;
            openRateSheetFileDialog.InitialDirectory = this.ExecutingPath;
            DialogResult result = openRateSheetFileDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                txtFileUpload.Text = openRateSheetFileDialog.FileName;
            }
        }

        private void btnUpload_Click(object sender, EventArgs e)
        {
            lblStatus.Text = "Processing...";
            lblStatus.ForeColor = Color.Red;

            this.DisableEnableForm(false);

            Utilities utilities = new Utilities();

            string rateSheetFilePath = txtFileUpload.Text;

            utilities.PopulateSheetsOfExcelFileOleDB(rateSheetFilePath);

            this.DisableEnableForm(true);
            lblStatus.Text = "Rate Sheed loaded successfully!!!";
            lblStatus.ForeColor = Color.Green;
            txtFileUpload.Text = string.Empty;
        }

        private void DisableEnableForm(bool isEnabled)
        {
            this.btnUpload.Enabled = isEnabled;
            this.btnBrowse.Enabled = isEnabled;
        }
    }
}
