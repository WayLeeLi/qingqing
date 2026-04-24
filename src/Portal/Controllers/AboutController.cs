using Academy.Common;
using Academy.Models;
using Academy.ViewModels;
using Newtonsoft.Json;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Academy.Controllers
{
    public class AboutController : WebController
    {
        public ActionResult Index()
        {
            var model = new AboutViewModel();

            // 1. 餐廳介紹
            var aboutIntro = db.DictSets.FirstOrDefault(d => d.Code == "OurStory");
            if (aboutIntro != null)
            {
                model.AboutIntro = new ContentItem
                {
                    Title = aboutIntro.Name,
                    Content = aboutIntro.Value
                };
            }

            // 2. 主廚介紹
            var chefIntro = db.DictSets.FirstOrDefault(d => d.Code == "AboutChef");
            if (chefIntro != null)
            {
                model.ChefIntro = new ContentItem
                {
                    Title = chefIntro.Name,
                    Content = chefIntro.Value
                };
            }

            // 3. 學經歷榮耀
            var credentials = db.DictSets.FirstOrDefault(d => d.Code == "AboutAcademicHonors");
            if (credentials != null)
            {
                model.ChefCredentials = new ContentItem
                {
                    Title = credentials.Name,
                    Content = credentials.Value
                };
            }

            // 4. 廚藝哲學
            var philosophy = db.DictSets.FirstOrDefault(d => d.Code == "AboutPhilosophyCooking");
            if (philosophy != null)
            {
                model.ChefPhilosophy = new ContentItem
                {
                    Title = philosophy.Name,
                    Content = philosophy.Value
                };
            }

            // 5. 台菜三寶
            var trinity = db.DictSets.FirstOrDefault(d => d.Code == "AboutTaiwaneseTrio");
            if (trinity != null)
            {
                model.Trinity = new ContentItem
                {
                    Title = trinity.Name,
                    Content = trinity.Value
                };
            }

            // 6. 三代傳承
            var heritage = db.DictSets.FirstOrDefault(d => d.Code == "AboutTriLegacy");
            if (heritage != null)
            {
                model.FamilyHeritage = new ContentItem
                {
                    Title = heritage.Name,
                    Content = heritage.Value
                };
            }

            // 7. 統計數據（可選，儲存為 JSON 字串）
            var statsData = db.DictSets.FirstOrDefault(d => d.Code == "AboutStats");
            if (statsData != null && !string.IsNullOrEmpty(statsData.Value))
            {
                try
                {
                    model.Stats = JsonConvert.DeserializeObject<List<StatItem>>(statsData.Value);
                }
                catch { /* 忽略解析錯誤 */ }
            }
            return View(model);
        }
        private string GenerateDomId(string catName)
        {
            if (string.IsNullOrEmpty(catName))
                return "";

            // 传统 switch 语句（兼容 C# 7.3 及更早）
            switch (catName)
            {
                case "餐廳介紹":
                    return "sec-intro";
                case "主廚介紹":
                    return "sec-chef";
                case "學歷榮耀":
                    return "sec-cred";
                case "廚藝哲學":
                    return "sec-philo";
                case "台菜三寶":
                    return "sec-trinity";
                case "三代傳承":
                    return "sec-gen";
                default:
                    return "sec-" + catName.Replace(" ", "").ToLower();
            }
        }
    }
}