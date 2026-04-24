using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Academy.ViewModels
{
    public class NewsListViewModel
    {
        public List<NewsItemViewModel> NewsList { get; set; }
        public List<CategoryFilter> Categories { get; set; }  // 用于生成筛选按钮
        public int TotalCount { get; set; }
    }
}