using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Academy.ViewModels
{
    public class NewsItemViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string CategoryName { get; set; }  // 分类名称（用于 data-tag 和显示）
        public string TagColorClass { get; set; } // t-red, t-gold 等（根据分类名或固定映射）
        public DateTime PublishDate { get; set; }  // PubDate 字段
        public string Summary { get; set; }       // Note 或 Note2
        public string DetailUrl { get; set; }     // 详情页链接（可使用 LinkPath）
       
    }
}