using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using Academy.Models;
using System.Data;
using Academy.ViewModels;

namespace Academy.Controllers
{
    public class NewsController : WebController
    {
        public ActionResult Index()
        {
            // 1. 获取新闻分类（Menu=4 表示新闻分类，Status=1 启用）
            var newsCategories = db.Categories
                .Where(c => c.Menu == 4 && c.Status == 1)
                .OrderBy(c => c.SortOrder)
                .ToList();

            // 2. 获取所有新闻（Menu=4，Status=1，按 PubDate 倒序）
            var newsQuery = db.Newss
                .Where(n => n.Menu == 4 && n.Status == 1)
                .OrderByDescending(n => n.PubDate)
                .ToList();

            // 3. 构建新闻列表
            var newsList = new List<NewsItemViewModel>();
            foreach (var news in newsQuery)
            {
                // 找出对应的分类名称
                string categoryName = "";
                var matchedCat = newsCategories.FirstOrDefault(c => c.Id == news.CataID);
                if (matchedCat != null)
                {
                    categoryName = matchedCat.Name;
                }
                else
                {
                    // 如果没有匹配分类，尝试使用 News.Lable 字段（备用）
                    if (!string.IsNullOrEmpty(news.Lable))
                        categoryName = news.Lable;
                    else
                        categoryName = "最新消息";
                }

                newsList.Add(new NewsItemViewModel
                {
                    Id = news.ID,
                    Title = news.Title ?? "無標題",
                    CategoryName = categoryName,
                    TagColorClass = GetTagColorClass(categoryName),
                    PublishDate = news.PubDate ?? DateTime.Now,
                    Summary = news.Note ?? (news.Note2 ?? ""),
                    DetailUrl = !string.IsNullOrEmpty(news.LinkPath) ? news.LinkPath : $"#/news/detail/{news.ID}"
                });
            }

            // 4. 获取所有不同的分类名称（用于筛选按钮）
            var distinctCategories = newsList
                .Select(n => n.CategoryName)
                .Distinct()
                .Select(c => new CategoryFilter { Name = c, Tag = c })
                .ToList();

            var viewModel = new NewsListViewModel
            {
                NewsList = newsList,
                Categories = distinctCategories,
                TotalCount = newsList.Count
            };

            return View(viewModel);
        }

        // 根据分类名称返回不同的 CSS 类（用于标签颜色）
        private string GetTagColorClass(string categoryName)
        {
            if (categoryName.Contains("年節") || categoryName.Contains("公告"))
                return "t-red";
            if (categoryName.Contains("新品") || categoryName.Contains("上市"))
                return "t-gold";
            if (categoryName.Contains("活動") || categoryName.Contains("參與"))
                return "t-green";
            if (categoryName.Contains("媒體") || categoryName.Contains("報導"))
                return "t-ink";
            if (categoryName.Contains("冷凍") || categoryName.Contains("外帶"))
                return "t-gold";
            return "t-blue";
        }

       
    }
}
