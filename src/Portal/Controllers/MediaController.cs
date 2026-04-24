using Academy.Models;
using Academy.ViewModels;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace Academy.Controllers
{
    public class MediaController : WebController
    {
        public ActionResult Index()
        {
            // 1. 取得媒體分類 (Menu=5)
            var mediaCategories = db.Categories
                .Where(c => c.Menu == 5 && c.Status == 1)
                .OrderBy(c => c.SortOrder)
                .ToList();

            // 2. 取得媒體資料 (Menu=3, Status=1)
            var mediaQuery = db.Newss
                .Where(n => n.Menu == 5 && n.Status == 1)
                .OrderByDescending(n => n.PubDate)
                .ThenBy(n => n.Sort)
                .ToList();

            // 3. 轉換為 ViewModel
            var mediaList = new List<MediaItemViewModel>();
            foreach (var item in mediaQuery)
            {
                string categoryName = "";
                var matchedCat = mediaCategories.FirstOrDefault(c => c.Id == item.CataID);
                if (matchedCat != null)
                    categoryName = matchedCat.Name;
                else if (!string.IsNullOrEmpty(item.Lable))
                    categoryName = item.Lable;
                else
                    categoryName = "媒體報導";

                string youTubeId = ExtractYouTubeId(item.VideoPath);
                string thumbnail = !string.IsNullOrEmpty(item.ImagePath)
                    ? item.ImagePath
                    : $"https://img.youtube.com/vi/{youTubeId}/mqdefault.jpg";

                mediaList.Add(new MediaItemViewModel
                {
                    Id = item.ID,
                    Title = item.Title ?? "無標題",
                    CategoryName = categoryName,
                    TagColorClass = GetTagColorClass(categoryName),
                    Source = !string.IsNullOrEmpty(item.Dept1) ? item.Dept1 : (item.Dept2 ?? ""),
                    Description = item.Note ?? (item.Note2 ?? ""),
                    YouTubeId = youTubeId,
                    ThumbnailUrl = thumbnail,
                    PublishDate = item.PubDate ?? DateTime.Now
                });
            }

            // 4. 動態產生篩選按鈕分類
            var distinctCategories = mediaList
                .Select(m => m.CategoryName)
                .Distinct()
                .Select(c => new CategoryFilter { Name = c, Tag = c })
                .ToList();

            var viewModel = new MediaListViewModel
            {
                MediaList = mediaList,
                Categories = distinctCategories,
                TotalCount = mediaList.Count
            };

            return View(viewModel);
        }

        private string ExtractYouTubeId(string videoPath)
        {
            if (string.IsNullOrEmpty(videoPath)) return "";
            // 支援多種格式：完整網址、短網址、純 ID
            var match = Regex.Match(videoPath, @"(?:youtube\.com\/(?:watch\?v=|embed\/)|youtu\.be\/)([a-zA-Z0-9_-]{11})");
            if (match.Success) return match.Groups[1].Value;
            if (Regex.IsMatch(videoPath, @"^[a-zA-Z0-9_-]{11}$")) return videoPath;
            return "";
        }

        private string GetTagColorClass(string categoryName)
        {
            if (categoryName.Contains("專題報導")) return "tag-feature";
            if (categoryName.Contains("料理示範")) return "tag-demo";
            if (categoryName.Contains("活動紀錄")) return "tag-event";
            if (categoryName.Contains("歷史檔案")) return "tag-history";
            if (categoryName.Contains("產品介紹")) return "tag-product";
            if (categoryName.Contains("美食報導")) return "tag-food";
            if (categoryName.Contains("電視報導")) return "tag-tv";
            return "tag-default";
        }
    }
}
