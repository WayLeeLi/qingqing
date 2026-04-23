using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Academy.ViewModels
{
    // HomeViewModel.cs - 首页视图模型
    public class HomeViewModel
    {
        // Banner 轮播图列表
        public List<ContentItem> Banners { get; set; }

        // 關於青青（单条）
        public ContentItem AboutInfo { get; set; }

        // 招牌菜列表
        public List<ContentItem> SignatureDishes { get; set; }

        // 活動與公告列表
        public List<ContentItem> NewsList { get; set; }

        // 影音專區列表
        public List<ContentItem> VideoList { get; set; }
    }
}