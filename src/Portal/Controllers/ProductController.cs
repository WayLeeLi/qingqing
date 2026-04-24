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
