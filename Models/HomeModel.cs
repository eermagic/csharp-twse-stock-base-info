using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StockBaseInfo.Models
{
    public class HomeModel
    {
        public class GetDataIn
        {
            public  string Q_MARKET_TYPE { get; set; }
        }

        public class GetDataOut
        {
            public string ErrMsg { get; set; }
            public List<StockRow> gridList { get; set; }
        }

        public class StockRow
        {
            // 只示範前面幾個欄位
            public string CompanyCode { get; set; }
            public string CompanyName { get; set; }
            public string CompanyAbbreviation { get; set; }
            public string IndustryCategory { get; set; }
            public string ForeignCompanyRegistrationCountry { get; set; }
            public string Address { get; set; }
            public string UniformNumberProfitBusiness { get; set; }
            public string Chairman { get; set; }
            public string GeneralManage { get; set; }
        }
    }
}