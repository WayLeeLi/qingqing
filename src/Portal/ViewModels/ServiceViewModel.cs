using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Academy.ViewModels
{
    public class ServiceViewModel
    {
        public List<ServiceItem> Services { get; set; }
        public List<ServiceItem> Features { get; set; }
        public List<ServiceItem> PricePlans { get; set; }
        public List<ServiceItem> ExtraServices { get; set; }
    }
    public class ServiceItem
    {
        public string SerialNumber { get; set; }   // "01 / 06"
        public string Icon { get; set; }            // 表情符號或圖標類
        public string Title { get; set; }
        public string SubtitleEn { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public string ContactLink { get; set; }     // "tel:..."
        public string CtaText { get; set; }         // "詢問婚宴方案 →"
    }
}