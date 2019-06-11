using System;
using System.Data;
using System.IO;
using System.Threading;

namespace ScopusFetcher
{
    public enum SearchType
    {
        EID,
        REFEID
    }

    public struct InputParam
    {
        public string WSName { get; set; }
        public string CodeColumnName { get; set; }
        public string EIDColumnName { get; set; }
    }

    public class Worker
    {
        private string downloadDir { get; set; }
        private string inputFilePath { get; set; }
        private Crawler crawler { get; set; }
        private Action<string> sendMsg { get; set; }
        private SearchType searchType { get; set; }
        private InputParam param { get; set; }
        private readonly int crawlerDelay = 1200;

        public Worker(string InputFilePath, string DownloadDir, Action<string> SendMsg, SearchType Type, InputParam Param, int CrawlerDelay)
        {
            downloadDir = DownloadDir;
            inputFilePath = InputFilePath;
            sendMsg = SendMsg;
            searchType = Type;
            param = Param;
            crawlerDelay = CrawlerDelay;
        }

        public void Download()
        {
            // check file exist
            string[] files = Directory.GetFiles(downloadDir);
            if (files.Length > 0)
            {
                sendMsg($"目錄 {downloadDir} 已內有檔案, 請先清空資料夾");
                return;
            }

            if (crawler == null)
            {
                sendMsg("啟動網頁爬蟲");
                crawler = new Crawler(downloadDir, crawlerDelay);
            }

            Thread t = new Thread(() => downloadStep());
            t.Start();
        }

        private delegate void SendMsgDelegate(string msg);
        private void downloadStep()
        {
            DataTable worksheet = Utils.GetDataTableFromExcel(inputFilePath, param.WSName);
            string searchFunc = (searchType == SearchType.EID) ? "EID" : "REFEID";
            SendMsgDelegate dg = new SendMsgDelegate(sendMsg);

            crawler.OpenPage();

            int successCount = 0;
            int failCount = 0;

            foreach (DataRow row in worksheet.Rows)
            {
                string EID = row[param.EIDColumnName].ToString();
                string code = row[param.CodeColumnName].ToString();

                string downloadFileNameSuffix = (searchType == SearchType.EID) ? "_reference" : "_citation";
                string downloadFileName = $"{code}{downloadFileNameSuffix}";

                MainForm.Mainform.Invoke(dg, $"開始下載檔案: EID '{EID}', 編號 '{code}'");

                crawler.Query($"{searchFunc}({EID})");
                crawler.Wait();

                bool success = crawler.ClickAll();

                if (!success)
                {
                    goto Failed;
                }

                if (searchType == SearchType.EID)
                {
                    crawler.CheckRef();
                    success = crawler.ClickAll();

                    if (!success)
                    {
                        goto Failed;
                    }
                }

                crawler.DownloadCSV(downloadFileName);
                successCount += 1;
                MainForm.Mainform.Invoke(dg, $"下載檔案 {downloadFileName}.csv 完成");

            End:
                crawler.ReturnSearchPage();
                crawler.Wait();
                continue;

            Failed:
                failCount += 1;
                MainForm.Mainform.Invoke(dg, $"下載檔案 {downloadFileName}.csv 失敗");
                goto End;
            }

            crawler.Close();
            MainForm.Mainform.Invoke(dg, $"全部下載完成: 共 {successCount} 份檔案, {failCount} 份無法下載");
        }
    }
}
