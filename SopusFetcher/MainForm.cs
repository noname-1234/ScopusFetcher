using System;
using System.Windows.Forms;

namespace SopusFetcher
{
    public partial class MainForm : Form
    {
        public static MainForm Mainform { get; set; }

        public MainForm()
        {
            InitializeComponent();
            Mainform = this;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            sendMsg("請先選擇檔案");
            string filePath = selectFile();
            if (filePath != string.Empty)
            {
                sendMsg($"已選擇檔案: {filePath}");
            }
            else
            {
                sendMsg("取消操作");
                return;
            }

            sendMsg("請選擇下載路徑");
            string downloadPath = selectPath();
            if (downloadPath != string.Empty)
            {
                sendMsg($"已選擇下載路徑: {downloadPath}");
            }
            else
            {
                sendMsg("取消操作");
                return;
            }

            InputParam param = new InputParam
            {
                WSName = tbWSName.Text,
                CodeColumnName = tbCodeCol.Text,
                EIDColumnName = tbEIDCol.Text
            };

            Worker worker = new Worker(filePath, downloadPath, sendMsg, (EID.Checked) ? SearchType.EID : SearchType.REFEID, param, (int)nudDelay.Value);
            try
            {
                worker.Download();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"錯誤: {ex.Message}", "錯誤");
            }
        }

        private string selectFile()
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Title = "選擇 Excel 檔案",
                InitialDirectory = ".\\",
                Filter = "Excel (*.*)|*.xlsx"
            };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                return dialog.FileName;
            }

            return string.Empty;
        }

        private string selectPath()
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog
            {
                Description = "選擇檔案下載位置"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                return dialog.SelectedPath;
            }

            return string.Empty;
        }

        private void sendMsg(string msg)
        {
            if (console.Text == string.Empty)
            {
                console.Text += $"{msg}";
            }
            else
            {
                console.Text += $"\n{msg}";
            }

            console.SelectionStart = console.Text.Length;
            // scroll it automatically
            console.ScrollToCaret();
        }
    }
}
