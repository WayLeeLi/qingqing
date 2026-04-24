using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;

namespace Academy.ViewModels
{
    public class AboutViewModel
    {
        public ContentItem AboutIntro { get; set; }      // 餐廳介紹
        public ContentItem ChefIntro { get; set; }       // 主廚介紹
        public ContentItem ChefCredentials { get; set; } // 學經歷榮耀
        public ContentItem ChefPhilosophy { get; set; }  // 廚藝哲學
        public ContentItem Trinity { get; set; }         // 台菜三寶
        public ContentItem FamilyHeritage { get; set; }  // 三代傳承
        public List<StatItem> Stats { get; set; }        // 統計數據（可選）
    }
}