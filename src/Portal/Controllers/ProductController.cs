using Academy.Models;
using Academy.ViewModels;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Academy.Controllers
{
    public class ProductController : WebController
    {
        public ActionResult Index()
        {
            // 获取分类数据（Menu = 3 表示菜品分类，Status = 1 表示上线的）
            var categories = db.Categories
                .Where(c => c.Menu == 3 && c.Status == 1)
                .OrderBy(c => c.SortOrder)
                .ToList();

            ViewBag.CategoryList= categories;
            // 2. 构建 ViewBag.CategoryList（用于顶部 Tab 栏，包含 DomId 和分类名）
            ViewBag.CategoryList = categories.Select(c => new
            {
                c.Id,
                c.Name,
                DomId = GenerateDomId(c.Name)
            }).ToList();

            // 3. 获取所有菜品（News表中 Menu=3 表示菜品，Status=1 启用）
            var dishesQuery = db.Newss
                .Where(n => n.Menu == 3 && n.Status == 1)
                .OrderBy(n => n.Sort)
                .ToList();

            // 4. 按分类组装 ViewModel
            var model = new List<DishCategoryViewModel>();
            foreach (var cat in categories)
            {
                var catDishes = dishesQuery
                    .Where(d => d.CataID == cat.Id)
                    .Select(d => new DishViewModel
                    {
                        Id = d.ID,
                        Name = d.Title ?? "無題",
                        ImageUrl = d.ImagePath ?? "",
                        ThumbUrl = d.ImagePath ?? "",
                        Summary = d.Note ?? "",
                        Description = d.Content ?? "",
                        SortOrder = d.Sort ?? 0
                    })
                    .OrderBy(d => d.SortOrder)
                    .ToList();

                model.Add(new DishCategoryViewModel
                {
                    Id = cat.Id,
                    Name = cat.Name,
                    NameEn = "",  // 若 Category 表没有英文名，可留空或从其他字段映射
                    DomId = GenerateDomId(cat.Name),
                    IconBlockHtml = GetCategoryIconBlock(cat.Name),
                    SortOrder = cat.SortOrder,
                    Dishes = catDishes
                });
            }

            return View(model);
        }

        // 辅助：中文分类名转英文 ID（用于锚点）
        private string GenerateDomId(string catName)
        {
            if (string.IsNullOrEmpty(catName)) return "category";

            // 传统 switch 语句，不使用表达式
            switch (catName)
            {
                case "三杯類": return "sanpei";
                case "炒類": return "stir";
                case "炸類": return "fried";
                case "烤類": return "grill";
                case "蒸類": return "steam";
                case "鐵板類": return "teppan";
                case "桌菜": return "table";
                case "港式點心": return "dimsum";
                case "湯類": return "soup";
                default: return catName.Replace(" ", "").ToLower();
            }
        }
        /// <summary>
        /// 获取分类图标完整区块（带渐变背景的 div + SVG）
        /// </summary>
        /// <param name="categoryName">分类名称（中文）</param>
        /// <returns>完整的 HTML 字符串，用于分类头部展示</returns>
        private string GetCategoryIconBlock(string categoryName)
        {
            string gradient = GetCategoryBgGradient(categoryName);
            string svg = GetIconSvgByCategory(categoryName);

            return $@"<div class='cat-icon' style='background:{gradient};'>{svg}</div>";
        }

        /// <summary>
        /// 获取分类渐变背景（与原样式一致）
        /// </summary>
        private string GetCategoryBgGradient(string categoryName)
        {
            switch (categoryName)
            {
                case "三杯類": return "linear-gradient(135deg,#7A0E20,#C8102E)";
                case "炒類": return "linear-gradient(135deg,#5A2800,#8B4A00)";
                case "炸類": return "linear-gradient(135deg,#6B3800,#B86010)";
                case "烤類": return "linear-gradient(135deg,#6B1A00,#A83000)";
                case "蒸類": return "linear-gradient(135deg,#004A5A,#006B80)";
                case "鐵板類": return "linear-gradient(135deg,#1A1A2E,#2E2E50)";
                case "桌菜": return "linear-gradient(135deg,#5A0612,#C8102E)";
                case "港式點心": return "linear-gradient(135deg,#3D0060,#6B0090)";
                case "湯類": return "linear-gradient(135deg,#002A5A,#004080)";
                default: return "linear-gradient(135deg,#333,#666)";
            }
        }

        /// <summary>
        /// 获取分类内嵌 SVG 图标（纯 SVG 代码，不带外层 div）
        /// </summary>
        private string GetIconSvgByCategory(string categoryName)
        {
            switch (categoryName)
            {
                case "三杯類":
                    return @"<svg viewBox='0 0 40 40' fill='none'><path d='M8 30 Q10 18 20 16 Q30 18 32 30Z' fill='rgba(255,255,255,.9)'/><ellipse cx='20' cy='30' rx='12' ry='2.5' fill='rgba(255,255,255,.5)'/><circle cx='14' cy='22' r='2.5' fill='#F0CC78'/><circle cx='20' cy='20' r='2.5' fill='#F0CC78'/><circle cx='26' cy='22' r='2.5' fill='#F0CC78'/><rect x='18' y='8' width='4' height='8' rx='2' fill='rgba(255,255,255,.8)'/></svg>";
                case "炒類":
                    return @"<svg viewBox='0 0 40 40' fill='none'><path d='M6 26 Q8 14 20 12 Q32 14 34 26L32 28 Q20 32 8 28Z' fill='rgba(255,255,255,.85)'/><path d='M8 14 Q12 8 18 10' stroke='#F0CC78' stroke-width='2' stroke-linecap='round'/><path d='M16 12 Q20 6 26 9' stroke='#F0CC78' stroke-width='2' stroke-linecap='round'/><rect x='17' y='30' width='6' height='5' rx='1' fill='rgba(255,255,255,.7)'/></svg>";
                case "炸類":
                    return @"<svg viewBox='0 0 40 40' fill='none'><rect x='6' y='18' width='28' height='14' rx='4' fill='rgba(255,255,255,.85)'/><circle cx='14' cy='24' r='4' fill='#F0CC78'/><circle cx='26' cy='24' r='4' fill='#F0CC78'/><path d='M4 18L36 18' stroke='rgba(255,255,255,.5)' stroke-width='2'/><path d='M12 8 Q14 4 16 8 Q18 12 20 8 Q22 4 24 8' stroke='#F0CC78' stroke-width='1.5' stroke-linecap='round' fill='none'/></svg>";
                case "烤類":
                    return @"<svg viewBox='0 0 40 40' fill='none'><path d='M8 28L32 28L30 34L10 34Z' fill='rgba(255,255,255,.6)'/><rect x='6' y='22' width='28' height='6' rx='2' fill='rgba(255,255,255,.85)'/><path d='M12 22L12 14M20 22L20 12M28 22L28 14' stroke='#F0CC78' stroke-width='2'/><ellipse cx='12' cy='17' rx='3' ry='3.5' fill='rgba(255,255,255,.8)'/><ellipse cx='28' cy='17' rx='3' ry='3.5' fill='rgba(255,255,255,.8)'/></svg>";
                case "蒸類":
                    return @"<svg viewBox='0 0 40 40' fill='none'><rect x='6' y='20' width='28' height='14' rx='3' fill='rgba(255,255,255,.85)'/><path d='M6 20L10 12L30 12L34 20' fill='rgba(255,255,255,.35)'/><path d='M14 8 Q15 5 14 3M20 7 Q21 4 20 2M26 8 Q27 5 26 3' stroke='#F0CC78' stroke-width='1.8' stroke-linecap='round' fill='none'/></svg>";
                case "鐵板類":
                    return @"<svg viewBox='0 0 40 40' fill='none'><rect x='4' y='22' width='32' height='12' rx='2' fill='rgba(255,255,255,.85)'/><ellipse cx='20' cy='22' rx='16' ry='4' fill='rgba(255,255,255,.55)'/><path d='M14 22 Q16 14 20 12 Q24 14 26 22' fill='rgba(255,200,100,.7)'/><path d='M17 22 Q18 17 20 16 Q22 17 23 22' fill='#F0CC78'/></svg>";
                case "桌菜":
                    return @"<svg viewBox='0 0 40 40' fill='none'><circle cx='20' cy='20' r='13' stroke='rgba(255,255,255,.85)' stroke-width='2' fill='rgba(255,255,255,.12)'/><circle cx='20' cy='14' r='2.5' fill='#F0CC78'/><circle cx='26' cy='18' r='2.5' fill='#F0CC78'/><circle cx='24' cy='25' r='2.5' fill='#F0CC78'/><circle cx='16' cy='25' r='2.5' fill='#F0CC78'/><circle cx='14' cy='18' r='2.5' fill='#F0CC78'/></svg>";
                case "港式點心":
                    return @"<svg viewBox='0 0 40 40' fill='none'><rect x='10' y='16' width='20' height='14' rx='2' fill='rgba(255,255,255,.85)'/><path d='M6 14 Q20 10 34 14' stroke='rgba(255,255,255,.65)' stroke-width='2' stroke-linecap='round'/><path d='M14 24 Q17 20 20 24 Q23 28 26 24' stroke='#F0CC78' stroke-width='1.5' fill='none' stroke-linecap='round'/></svg>";
                case "湯類":
                    return @"<svg viewBox='0 0 40 40' fill='none'><path d='M8 24 Q10 34 20 35 Q30 34 32 24Z' fill='rgba(255,255,255,.85)'/><ellipse cx='20' cy='24' rx='12' ry='3.5' fill='rgba(255,255,255,.55)'/><path d='M14 12 Q13 8 15 6M20 10 Q19 6 21 4M26 12 Q25 8 27 6' stroke='#F0CC78' stroke-width='1.8' stroke-linecap='round' fill='none'/></svg>";
                default:
                    return @"<svg viewBox='0 0 40 40' fill='none'><circle cx='20' cy='20' r='14' fill='rgba(255,255,255,.8)'/><path d='M12 20 L28 20 M20 12 L20 28' stroke='#C8102E' stroke-width='2'/><circle cx='20' cy='20' r='5' fill='#F0CC78'/></svg>";
            }
        }

        [HttpPost]
        public JsonResult GetPortfolioList(int categoryId = 0)
        {
            try
            {
                // 从 News 表查询作品数据（Menu = 4 表示作品集）
                var query = from n in db.Newss
                            join c in db.Categories on n.CataID equals c.Id
                            where n.Status == 1 && n.Menu == 4
                            select new
                            {
                                n.ID,
                                n.Title,
                                n.ImagePath,
                                n.CataID,
                                n.CDate,
                                CategoryName = c.Name
                            };

                // 如果不是全部，按分类筛选
                if (categoryId > 0)
                {
                    query = query.Where(p => p.CataID == categoryId);
                }

                var list = query.OrderByDescending(p => p.CDate)
                    .Select(p => new
                    {
                        p.ID,
                        p.Title,
                        // 优先使用 ImagePath，如果没有则使用 Photo
                        ImageUrl = !string.IsNullOrEmpty(p.ImagePath) ? p.ImagePath : p.ImagePath,
                        p.CataID,
                        p.CategoryName,
                        // 如果没有 IsWide/IsTall 字段，可以去掉或设置默认值
                        IsWide = false,
                        IsTall = false
                    })
                    .ToList();

                return Json(new { success = true, data = list }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

    }
}
