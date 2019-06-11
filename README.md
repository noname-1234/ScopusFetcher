# ScopusFetcher

利用 Selenium 抓取 Scopus 上依 EID, REFEID 搜尋論文並下載 CSV 格式之資訊檔

![Fetcher](https://github.com/Zack-Cheng/ScopusFetcher/blob/master/UI.PNG)

輸入檔案格式:

**編號**|**EID**
:-----:|:-----:
a0001|2-s2.0-0021891374
a0002|2-s2.0-0029256709
a0003|2-s2.0-78649592630
a0004|2-s2.0-84867069655
a0005|2-s2.0-0020005803

抓取後的 CSV 檔案檔名會依 "{編號}_referece.csv" / "{編號}_citation.csv" 命名

範例檔可用 `input.xlsx` 進行測試
