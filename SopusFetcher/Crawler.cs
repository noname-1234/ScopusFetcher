using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace SopusFetcher
{
    public class Crawler
    {
        private IWebDriver driver { get; set; }
        private string downloadDir { get; set; }
        private bool pendoGuideBannerExist { get; set; }
        private int delay { get; set; }
        private bool extraExportInfoChecked { get; set; }

        private readonly string scopusUrl = "https://www.scopus.com/search/form.uri?display=advanced";

        public Crawler(string DownloadDir, int Delay)
        {
            downloadDir = DownloadDir;
            Directory.CreateDirectory(downloadDir);
            ChromeOptions option = getChromeDriverOption(downloadDir);
            driver = new ChromeDriver(option);
            delay = Delay;

            pendoGuideBannerExist = true;
            extraExportInfoChecked = false;
        }

        public void OpenPage()
        {
            driver.Navigate().GoToUrl(scopusUrl);
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(12);
        }

        public void Query(string QueryString)
        {
            input(By.Id("searchfield"), QueryString);
            click(By.Id("advSearch"));
        }

        public void CheckRef()
        {
            click(By.Id("moreOptionToggleBtn"));
            click(By.CssSelector("button[data-type='ViewReference']"));
        }

        public bool ClickAll()
        {
            if (!elementExist(By.Id("selectAllCheck")))
            {
                return false;
            }

            click(By.Id("selectAllCheck"));
            return true;
        }

        public void DownloadCSV(string FileName)
        {
            click(By.Id("export_results"));
            click(By.CssSelector("label[for='CSV']"));

            if (pendoGuideBannerExist && elementExist(By.Id("_pendo-close-guide_")))
            {
                click(By.Id("_pendo-close-guide_"));
                pendoGuideBannerExist = false;
            }

            if (!extraExportInfoChecked)
            {
                if (!checkElementAttributeExist(By.Id("selectedBibliographicalInformationItems-Export1"), "checked"))
                {
                    click(By.CssSelector("label[for='selectedBibliographicalInformationItems-Export1']"));
                }

                if (!checkElementAttributeExist(By.Id("selectedAbstractInformationItems-Export1"), "checked"))
                {
                    click(By.CssSelector("label[for='selectedAbstractInformationItems-Export1']"));
                }

                extraExportInfoChecked = true;
            }

            click(By.Id("exportTrigger"));

            Wait();

            renameDownloadFile("scopus.csv", $"{FileName}.csv");
        }

        public void Close()
        {
            driver.Close();
        }

        public void ReturnSearchPage()
        {
            driver.Url = scopusUrl;
        }

        public void Wait(int Delay)
        {
            Thread.Sleep(Delay);
        }

        public void Wait()
        {
            Thread.Sleep(delay);
        }

        protected bool checkElementAttributeExist(By by, string attribute)
        {
            IWebElement element = driver.FindElement(by);
            string attr = element.GetAttribute(attribute);

            return !(attr == null);
        }

        protected void renameDownloadFile(string oldName, string newName)
        {
            File.Move($"{downloadDir}\\{oldName}", $"{downloadDir}\\{newName}");
        }

        protected void input(By by, string input)
        {
            IWebElement queryForm = driver.FindElement(by);
            Wait();

            queryForm.Clear();
            Wait();

            queryForm.SendKeys(input);
            Wait();
        }

        protected bool elementExist(By by)
        {
            try
            {
                driver.FindElement(by);
                return true;
            }
            catch (NoSuchElementException)
            {
                Debug.WriteLine("");
                return false;
            }
        }

        protected void click(By by)
        {
            IWebElement searchBtn = driver.FindElement(by);
            Wait();

            searchBtn.Click();
            Wait();
        }

        private ChromeOptions getChromeDriverOption(string path)
        {
            ChromeOptions chromeOptions = new ChromeOptions();
            string downloadDirectory = path;

            chromeOptions.AddUserProfilePreference("download.default_directory", downloadDirectory);
            chromeOptions.AddUserProfilePreference("download.prompt_for_download", false);
            chromeOptions.AddUserProfilePreference("disable-popup-blocking", "true");

            return chromeOptions;
        }
    }
}
