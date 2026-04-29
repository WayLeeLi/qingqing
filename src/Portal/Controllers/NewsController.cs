using Academy.Models;
using Academy.ViewModels;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

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

        /// <summary>
        /// 获取新闻列表
        /// </summary>
        /// <param name="categoryId">分类ID，0表示全部新闻</param>
        [HttpPost]
        public JsonResult GetNewsList(int categoryId = 0)
        {
            try
            {
                // 从 News 表查询新闻数据（Menu = 3 表示新闻活动）
                var query = from n in db.Newss
                            join c in db.Categories on n.CataID equals c.Id
                            where n.Status == 1 && n.Menu == 5
                            select new
                            {
                                n.ID,
                                n.Title,
                                n.ImagePath,
                                n.CataID,
                                n.CDate,
                                n.Content,
                                n.Note,
                                CategoryName = c.Name
                            };

                // 如果不是全部，按分类筛选
                if (categoryId > 0)
                {
                    query = query.Where(p => p.CataID == categoryId);
                }

                // 先获取数据到内存
                var list = query.OrderByDescending(p => p.CDate).ToList();

                // 在内存中进行格式化处理
                var result = list.Select(p => new
                {
                    p.ID,
                    p.Title,
                    ImageUrl = p.ImagePath ?? "",
                    Date = p.CDate.HasValue ? p.CDate.Value.ToString("yyyy 年 M 月") : "",
                    p.Note,
                    p.Content,
                    p.CategoryName
                }).ToList();

                return Json(new { success = true, data = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 获取新闻详情
        /// </summary>
        [HttpPost]
        public JsonResult GetNewsDetail(int id)
        {
            try
            {
                var news = (from n in db.Newss
                            join c in db.Categories on n.CataID equals c.Id
                            where n.ID == id && n.Status == 1
                            select new
                            {
                                n.ID,
                                n.Title,
                                n.ImagePath,
                                n.CDate,
                                n.Content,
                                n.Note,
                                CategoryName = c.Name
                            }).FirstOrDefault();

                if (news == null)
                {
                    return Json(new { success = false, message = "新聞不存在" }, JsonRequestBehavior.AllowGet);
                }

                var data = new
                {
                    news.ID,
                    news.Title,
                    ImageUrl = news.ImagePath ?? "",
                    Date = news.CDate.HasValue ? news.CDate.Value.ToString("yyyy 年 M 月") : "",
                    Content = news.Content ?? news.Note ?? "",
                    news.CategoryName
                };

                return Json(new { success = true, data = data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Detail(int id)
        {
            // 获取当前新闻
            var news = db.Newss
                .Include("Category")   // 或者 .Include(n => n.Category)
                .FirstOrDefault(n => n.ID == id && n.Status == 1 && n.Menu == 4);
            if (news == null)
                return RedirectToAction("Index");

            // 获取分类名称（可通过 news.Category.Name 访问，或存入 ViewBag）
            ViewBag.CategoryName = news.Category?.Name;

            int currentCataId = news.CataID;

            // 获取同一分类下的上一篇（比当前新闻时间更早且最近的一条）
            var prevNews = db.Newss
                .Where(n => n.Status == 1 && n.Menu == 4 && n.CataID == currentCataId && n.CDate < news.CDate)
                .OrderByDescending(n => n.CDate)
                .FirstOrDefault();

            // 获取同一分类下的下一篇
            var nextNews = db.Newss
                .Where(n => n.Status == 1 && n.Menu == 4 && n.CataID == currentCataId && n.CDate > news.CDate)
                .OrderBy(n => n.CDate)
                .FirstOrDefault();

            // 获取同一分类下的侧边栏新闻列表
            var rawList = db.Newss
                .Where(n => n.Status == 1 && n.Menu == 4 && n.CataID == currentCataId)
                .OrderByDescending(n => n.CDate)
                .Select(n => new {
                    n.ID,
                    n.Title,
                    n.ImagePath,
                    n.CDate,
                    n.Note,
                    n.Content
                })
                .ToList();

            var sidebarNews = rawList.Select(n => new Academy.ViewModels.SidebarNewsItem
            {
                ID = n.ID,
                Title = n.Title,
                Date = n.CDate.HasValue ? n.CDate.Value.ToString("yyyy 年 M 月") : "",
                Thumb = !string.IsNullOrEmpty(n.ImagePath) ? n.ImagePath : "/images/default-news.jpg",
                Summary = n.Note
            }).ToList();

            ViewBag.PrevNews = prevNews;
            ViewBag.NextNews = nextNews;
            ViewBag.SidebarNewsList = sidebarNews;

            ViewBag.Address = GetDictValue("Contact_Address");
            ViewBag.Phone = GetDictValue("Contact_Phone");
            ViewBag.Email = GetDictValue("Contact_Email");
            ViewBag.Fax = GetDictValue("Contact_Fax");
            ViewBag.BusinessHours = GetDictValue("Contact_BusinessHours");
            ViewBag.TrafficInfo = GetDictValue("Contact_TrafficGuide");  // 交通指引
            ViewBag.LineLink = GetDictValue("Contact_LineLink");         // 如果 DictSet 中有配置
            return View(news);
        }

        // 获取侧边栏新闻列表（AJAX）
        [HttpPost]
        public JsonResult GetSidebarNews(int currentId)
        {
            // 先查询数据到内存（避免在 LINQ to Entities 中使用 ToString）
            var list = db.Newss
                .Where(n => n.Status == 1 && n.Menu == 4)
                .OrderByDescending(n => n.CDate)
                .Select(n => new {
                    n.ID,
                    n.Title,
                    n.ImagePath,
                    n.CDate,
                    n.Note,
                    n.Content
                })
                .ToList();

            // 在内存中进行格式化处理
            var result = list.Select(n => new {
                n.ID,
                Title = n.Title,
                Date = n.CDate.HasValue ? n.CDate.Value.ToString("yyyy 年 M 月") : "",
                Thumb = !string.IsNullOrEmpty(n.ImagePath) ? n.ImagePath : "/images/default-news.jpg",
                Summary = n.Content
            }).ToList();

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        private string GetDictValue(string code)
        {
            var dict = db.DictSets.FirstOrDefault(d => d.Code == code);
            return dict?.Value ?? "";
        }
    }

}
