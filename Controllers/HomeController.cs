using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using static StockBaseInfo.Models.HomeModel;

namespace StockBaseInfo.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        /// <summary>
        /// 查到股票基本資料
        /// </summary>
        /// <param name="inModel"></param>
        /// <returns></returns>
        public ActionResult GetData(GetDataIn inModel)
        {
            GetDataOut outModel = new GetDataOut();

            if (string.IsNullOrEmpty(inModel.Q_MARKET_TYPE))
            {
                outModel.ErrMsg = "請選擇市場別";
                return Json(outModel);
            }

            outModel.gridList = new List<StockRow>();
            WebClient webClient = new WebClient();

            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();

            // 市場別網址
            string QueryUrl = "https://mops.twse.com.tw/mops/web/ajax_t51sb01?encodeURIComponent=1&step=1&firstin=1&TYPEK={0}&code=";
            QueryUrl = string.Format(QueryUrl, inModel.Q_MARKET_TYPE);

            // 取得查詢回傳
            MemoryStream ms = new MemoryStream(webClient.DownloadData(QueryUrl));
            doc.Load(ms, Encoding.Default);

            // 取得另存 CSV form
            HtmlNodeCollection formNode = doc.DocumentNode.SelectNodes("//form[@name='fm']");
            if (formNode != null)
            {
                // 取得欄位
                HtmlNode filenameNode = doc.DocumentNode.SelectSingleNode("//form[@name='fm']/input[@name='filename']");
                string filenameValue = filenameNode.GetAttributeValue("value", "");

                // CSV 網址
                string csvUrl = "https://mops.twse.com.tw/server-java/t105sb02?firstin=true&step=10&filename={0}";
                csvUrl = string.Format(csvUrl, filenameValue);

                // 呼叫 CSV 網址
                string csvData = webClient.DownloadString(csvUrl);

                if (csvData.Trim().Length > 0)
                {
                    DataTable dt = new DataTable();
                    string[] lineStrs = csvData.Split('\n');
                    for (int i = 0; i < lineStrs.Length; i++)
                    {
                        string strline = lineStrs[i];
                        // 解析資料
                        ArrayList csvLine = new ArrayList();
                        this.ParseCSVData(csvLine, strline);
                        if (i == 0)
                        {
                            for (int c = 0; c < csvLine.Count; c++)
                            {
                                dt.Columns.Add(csvLine[c].ToString());
                            }
                        }
                        else
                        {
                            // 寫入 Datatable
                            DataRow dr = dt.NewRow();
                            for (int c = 0; c < csvLine.Count; c++)
                            {
                                dr[c] = csvLine[c].ToString();
                            }
                            dt.Rows.Add(dr);
                        }

                    }

                    // 輸出資料
                    foreach (DataRow dr in dt.Rows)
                    {
                        // 只示範前面幾個欄位
                        StockRow row = new StockRow();
                        row.CompanyCode = dr["公司代號"].ToString();
                        row.CompanyName = dr["公司名稱"].ToString();
                        row.CompanyAbbreviation = dr["公司簡稱"].ToString();
                        row.IndustryCategory = dr["產業類別"].ToString();
                        row.ForeignCompanyRegistrationCountry = dr["外國企業註冊地國"].ToString();
                        row.Address = dr["住址"].ToString();
                        row.UniformNumberProfitBusiness = dr["營利事業統一編號"].ToString();
                        row.Chairman = dr["董事長"].ToString();
                        outModel.gridList.Add(row);
                    }

                }

            }

            // 輸出json
            return Json(outModel);
        }

        /// <summary>
        /// 解析 CSV
        /// </summary>
        /// <param name="result"></param>
        /// <param name="data"></param>
        private void ParseCSVData(ArrayList result, string data)
        {
            int position = -1;
            while (position < data.Length)
                result.Add(ParseCSVField(ref data, ref position));
        }

        /// <summary>
        /// 解析 CSV
        /// </summary>
        /// <param name="data"></param>
        /// <param name="StartSeperatorPos"></param>
        /// <returns></returns>
        private string ParseCSVField(ref string data, ref int StartSeperatorPos)
        {
            if (StartSeperatorPos == data.Length - 1)
            {
                StartSeperatorPos++;
                return "";
            }

            int fromPos = StartSeperatorPos + 1;
            if (data[fromPos] == '"')
            {
                int nextSingleQuote = GetSingleQuote(data, fromPos + 1);
                StartSeperatorPos = nextSingleQuote + 1;
                string tempString = data.Substring(fromPos + 1, nextSingleQuote - fromPos - 1);
                tempString = tempString.Replace("'", "''");
                return tempString.Replace("\"\"", "\"");
            }

            int nextComma = data.IndexOf(',', fromPos);
            if (nextComma == -1)
            {
                StartSeperatorPos = data.Length;
                return data.Substring(fromPos);
            }
            else
            {
                StartSeperatorPos = nextComma;
                return data.Substring(fromPos, nextComma - fromPos);
            }
        }

        /// <summary>
        /// 解析 CSV
        /// </summary>
        /// <param name="data"></param>
        /// <param name="SFrom"></param>
        /// <returns></returns>
        private int GetSingleQuote(string data, int SFrom)
        {
            int i = SFrom - 1;
            while (++i < data.Length)
                if (data[i] == '"')
                {
                    if (i < data.Length - 1 && data[i + 1] == '"')
                    {
                        i++;
                        continue;
                    }
                    else
                        return i;
                }
            return -1;
        }
    }
}