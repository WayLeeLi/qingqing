using Academy.Controllers;
using Academy.Models;
using Academy.ViewModels;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Academy.Areas.Sysmgr.Controllers
{
    public class CategoryController : BaseController
    {

        // GET: Category
        public ActionResult Index(int page = 1, int? parentId = null, string keyword = "", string menu = "")
        {
            int pageSize = 15;
            var result = string.IsNullOrEmpty(menu) ? 0 : int.Parse(menu);
            var query = db.Categories.Where(s => s.Menu == result).AsQueryable();

            // 如果指定了 parentId，只显示该父级下的类别
            if (parentId.HasValue)
            {
                query = query.Where(c => c.ParentId == parentId);
            }
            else
            {
                // 不指定 parentId 时，显示所有类别（不分层级）
                query = query.Where(c => true);  // 显示所有类别
            }

            // 按关键词搜索
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(c => c.Name.Contains(keyword));
            }

            // 按 Path 排序，这样同层级的会排在一起
            query = query.OrderBy(c => c.Path).ThenBy(c => c.SortOrder);

            var pagedList = query.ToPagedList(page, pageSize);

            // 获取当前父类别的信息
            if (parentId.HasValue)
            {
                var parent = db.Categories.Find(parentId.Value);
                if (parent != null)
                {
                    ViewBag.Parent = parent;
                    ViewBag.ParentPath = GetCategoryPath(parent);
                }
            }

            // 为每个类别计算子类别数量
            var categoryIds = pagedList.Select(c => c.Id).ToList();
            var childCounts = db.Categories
                .Where(c => categoryIds.Contains(c.ParentId ?? 0))
                .GroupBy(c => c.ParentId)
                .Select(g => new { ParentId = g.Key, Count = g.Count() })
                .ToDictionary(x => x.ParentId.Value, x => x.Count);

            ViewBag.ChildCounts = childCounts;
            ViewBag.CurrentParentId = parentId;
            ViewBag.Keyword = keyword;

            return View(pagedList);
        }

        // GET: Category/Create
        public ActionResult Create(int? parentId, string menu = "")
        {
            var result = string.IsNullOrEmpty(menu) ? 0 : int.Parse(menu);
            var category = new Category();
            if (parentId.HasValue)
            {
                category.ParentId = parentId;
            }

            var viewModel = new CategoryViewModel
            {
                Category = category
            };

            // 获取父类别列表供下拉框使用（编辑时用）
            var categories = db.Categories.Where(s => s.Menu == result)
                .OrderBy(c => c.Path)
                .ToList();

            viewModel.ParentCategories = GetParentCategoryList(categories, parentId);

            return View(viewModel);
        }

        // POST: Category/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CategoryViewModel viewModel, string menu = "", HttpPostedFileBase iconFile = null)
        {
            if (ModelState.IsValid)
            {
                var category = viewModel.Category;
                category.CreateTime = DateTime.Now;
                category.Menu = string.IsNullOrEmpty(menu) ? 0 : int.Parse(menu);

                // ========= 新增：处理图标上传 =========
                if (iconFile != null && iconFile.ContentLength > 0)
                {
                    string extension = System.IO.Path.GetExtension(iconFile.FileName).ToLower();
                    if (extension == ".jpg" || extension == ".jpeg" || extension == ".png" || extension == ".gif")
                    {
                        string uploadDir = Server.MapPath("~/Uploads/CategoryIcons/");
                        if (!System.IO.Directory.Exists(uploadDir))
                            System.IO.Directory.CreateDirectory(uploadDir);

                        string fileName = Guid.NewGuid().ToString() + extension;
                        string filePath = System.IO.Path.Combine(uploadDir, fileName);
                        iconFile.SaveAs(filePath);
                        category.Icon = "/Uploads/CategoryIcons/" + fileName;
                    }
                    else
                    {
                        ModelState.AddModelError("", "僅支援 JPG、PNG、GIF 格式的圖片");
                        // 重新加载下拉列表后返回视图
                        var categories = db.Categories.Where(c => c.Menu == category.Menu).OrderBy(c => c.Path).ToList();
                        viewModel.ParentCategories = GetParentCategoryList(categories, viewModel.Category.ParentId);
                        ViewBag.Menu = menu;
                        return View(viewModel);
                    }
                }
                // ==================================

                // 设置层级和路径（原有逻辑）
                if (!category.ParentId.HasValue)
                {
                    category.Level = 0;
                    db.Categories.Add(category);
                    db.SaveChanges();
                    category.Path = category.Id.ToString();
                }
                else
                {
                    var parent = db.Categories.Find(category.ParentId.Value);
                    if (parent != null)
                    {
                        category.Level = parent.Level + 1;
                        db.Categories.Add(category);
                        db.SaveChanges();
                        category.Path = parent.Path + "/" + category.Id;
                    }
                }

                db.Entry(category).State = EntityState.Modified;
                db.SaveChanges();

                TempData["Success"] = "類別新增成功！";

                if (!string.IsNullOrEmpty(menu))
                    return RedirectToAction("Index", new { sub = 2, menu = menu });
                else
                    return RedirectToAction("Index", new { parentId = category.ParentId });
            }

            // 验证失败时重新加载父级类别列表
            var allCategories = db.Categories
                .Where(c => c.Menu == (string.IsNullOrEmpty(menu) ? 0 : int.Parse(menu)))
                .OrderBy(c => c.Path)
                .ToList();
            viewModel.ParentCategories = GetParentCategoryList(allCategories, viewModel.Category.ParentId);
            ViewBag.Menu = menu;
            return View(viewModel);
        }

        // GET: Category/Edit/5
        public ActionResult Edit(int id, string menu = "")
        {
            var result = string.IsNullOrEmpty(menu) ? 0 : int.Parse(menu);
            var category = db.Categories.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }

            // 获取父类别列表（排除自身）
            var categories = db.Categories
                .Where(c => c.Id != id && c.Menu == result)
                .OrderBy(c => c.Path)
                .ToList();

            var viewModel = new CategoryViewModel
            {
                Category = category,
                ParentCategories = GetParentCategoryList(categories, category.ParentId)
            };

            // 如果 URL 中传入了 menu 参数，可以放入 ViewBag 供视图使用
            ViewBag.Menu = Request["menu"];

            return View(viewModel);
        }

        // POST: Category/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(CategoryViewModel viewModel, string menu = "", HttpPostedFileBase iconFile = null, bool? deleteIcon = false)
        {
            if (ModelState.IsValid)
            {
                var category = viewModel.Category;
                category.Menu = string.IsNullOrEmpty(menu) ? 0 : int.Parse(menu);

                // 检查是否形成循环引用
                if (IsCircularReference(category.Id, category.ParentId))
                {
                    ModelState.AddModelError("Category.ParentId", "不能選擇自己的子類別作為父級");
                    var categories = db.Categories.Where(c => c.Id != category.Id).OrderBy(c => c.Path).ToList();
                    viewModel.ParentCategories = GetParentCategoryList(categories, category.ParentId);
                    ViewBag.Menu = menu;
                    return View(viewModel);
                }

                // 获取原始类别信息（用于路径更新和图标处理）
                var originalCategory = db.Categories.AsNoTracking().FirstOrDefault(c => c.Id == category.Id);
                if (originalCategory == null)
                {
                    return HttpNotFound();
                }

                // ========= 处理图标（修正版） =========
                // 1. 优先处理上传新图片
                if (iconFile != null && iconFile.ContentLength > 0)
                {
                    string ext = System.IO.Path.GetExtension(iconFile.FileName).ToLower();
                    if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".gif")
                    {
                        // 删除旧图标文件（若存在）
                        if (!string.IsNullOrEmpty(originalCategory.Icon))
                        {
                            string oldPath = Server.MapPath(originalCategory.Icon);
                            if (System.IO.File.Exists(oldPath))
                                System.IO.File.Delete(oldPath);
                        }

                        // 保存新文件
                        string uploadDir = Server.MapPath("~/Uploads/CategoryIcons/");
                        if (!System.IO.Directory.Exists(uploadDir))
                            System.IO.Directory.CreateDirectory(uploadDir);
                        string fileName = Guid.NewGuid().ToString() + ext;
                        string filePath = System.IO.Path.Combine(uploadDir, fileName);
                        iconFile.SaveAs(filePath);
                        category.Icon = "/Uploads/CategoryIcons/" + fileName;
                    }
                    else
                    {
                        ModelState.AddModelError("", "僅支援 JPG、PNG、GIF 格式的圖片");
                        // 重新加载下拉列表后返回视图
                        var cats = db.Categories.Where(c => c.Id != category.Id && c.Menu == category.Menu).OrderBy(c => c.Path).ToList();
                        viewModel.ParentCategories = GetParentCategoryList(cats, category.ParentId);
                        ViewBag.Menu = menu;
                        return View(viewModel);
                    }
                }
                // 2. 没有上传新图片，但勾选了删除
                else if (deleteIcon == true)
                {
                    if (!string.IsNullOrEmpty(originalCategory.Icon))
                    {
                        string oldPath = Server.MapPath(originalCategory.Icon);
                        if (System.IO.File.Exists(oldPath))
                            System.IO.File.Delete(oldPath);
                    }
                    category.Icon = null;
                }
                // 3. 无任何操作，保留原图标
                else
                {
                    category.Icon = originalCategory.Icon;
                }
                // ====================================
                // ====================================

                // 更新路径和层级（如果父级改变）
                if (originalCategory.ParentId != category.ParentId)
                {
                    if (!category.ParentId.HasValue)
                    {
                        category.Level = 0;
                        category.Path = category.Id.ToString();
                    }
                    else
                    {
                        var parent = db.Categories.Find(category.ParentId.Value);
                        if (parent != null)
                        {
                            category.Level = parent.Level + 1;
                            category.Path = parent.Path + "/" + category.Id;
                        }
                    }

                    db.Entry(category).State = EntityState.Modified;
                    db.SaveChanges();
                    UpdateChildrenPaths(category.Id);
                }
                else
                {
                    db.Entry(category).State = EntityState.Modified;
                    db.SaveChanges();
                }

                TempData["Success"] = "類別更新成功！";

                if (!string.IsNullOrEmpty(menu))
                    return RedirectToAction("Index", new { sub = 2, menu = menu });
                else
                    return RedirectToAction("Index", new { parentId = category.ParentId });
            }

            // 验证失败时重新加载父级类别列表
            var allCategories = db.Categories
                .Where(c => c.Id != viewModel.Category.Id && c.Menu == (string.IsNullOrEmpty(menu) ? 0 : int.Parse(menu)))
                .OrderBy(c => c.Path)
                .ToList();
            viewModel.ParentCategories = GetParentCategoryList(allCategories, viewModel.Category.ParentId);
            ViewBag.Menu = menu;
            return View(viewModel);
        }




        // POST: Category/Delete
        [HttpPost]
        public JsonResult Delete(int id)
        {
            try
            {
                var category = db.Categories
                    .Include("Children")
                    .FirstOrDefault(c => c.Id == id);

                if (category == null)
                {
                    return Json(new { success = false, message = "資料不存在" });
                }

                // 檢查是否有子類別
                if (category.Children != null && category.Children.Any())
                {
                    return Json(new { success = false, message = "該類別下有子類別，無法刪除" });
                }

                db.Categories.Remove(category);
                db.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: Category/DeleteRange
        [HttpPost]
        public JsonResult DeleteRange(List<int> ids)
        {
            try
            {
                var successList = new List<int>();
                var failList = new List<string>();

                foreach (var id in ids)
                {
                    var category = db.Categories
                        .Include("Children")
                        .FirstOrDefault(c => c.Id == id);

                    if (category != null && (category.Children == null || !category.Children.Any()))
                    {
                        db.Categories.Remove(category);
                        successList.Add(id);
                    }
                    else
                    {
                        failList.Add($"ID {id} 刪除失敗（可能有子類別）");
                    }
                }

                db.SaveChanges();

                return Json(new
                {
                    success = true,
                    message = $"成功刪除 {successList.Count} 筆，失敗 {failList.Count} 筆",
                    fails = failList
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: Category/UpdateSort
        [HttpPost]
        public JsonResult UpdateSort(string data)
        {
            try
            {
                var sortList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SortModel>>(data);

                foreach (var item in sortList)
                {
                    var category = db.Categories.Find(item.ID);
                    if (category != null)
                    {
                        category.SortOrder = item.Sort;
                        db.Entry(category).State = EntityState.Modified;
                    }
                }

                db.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        #region 私有方法

        // 获取类别路径
        private string GetCategoryPath(Category category)
        {
            var path = new List<string>();
            var current = category;

            while (current != null)
            {
                path.Insert(0, current.Name);
                if (current.ParentId.HasValue)
                {
                    current = db.Categories.Find(current.ParentId.Value);
                }
                else
                {
                    current = null;
                }
            }

            return string.Join(" > ", path);
        }

        // 构建树形结构
        private List<CategoryTreeNode> BuildTree(List<Category> categories, int? parentId)
        {
            return categories
                .Where(c => c.ParentId == parentId)
                .OrderBy(c => c.SortOrder)
                .Select(c => new CategoryTreeNode
                {
                    Id = c.Id,
                    Name = c.Name,
                    Level = c.Level,
                    SortOrder = c.SortOrder,
                    CreateTime = c.CreateTime,
                    Children = BuildTree(categories, c.Id)
                })
                .ToList();
        }

        // 获取父级类别下拉列表
        private List<SelectListItem> GetParentCategoryList(List<Category> categories, int? selectedParentId)
        {
            var list = new List<SelectListItem>
    {
        new SelectListItem { Value = "", Text = "-- 頂級類別 --" }
    };

            foreach (var category in categories)
            {
                string prefix = "";
                if (category.Level > 0)
                {
                    // 根据层级添加缩进（每级两个空格）
                    for (int i = 0; i < category.Level; i++)
                    {
                        prefix += "  "; // 两个空格
                    }
                    // 添加 └─ 符号和空格
                    prefix += "└─ ";
                }

                list.Add(new SelectListItem
                {
                    Value = category.Id.ToString(),
                    Text = prefix + category.Name,
                    Selected = (category.Id == selectedParentId)
                });
            }

            return list;
        }

        // 更新子类别路径
        private void UpdateChildrenPaths(int parentId)
        {
            var children = db.Categories
                .Where(c => c.ParentId == parentId)
                .ToList();

            foreach (var child in children)
            {
                var parent = db.Categories.Find(parentId);
                if (parent != null)
                {
                    child.Path = parent.Path + "/" + child.Id;
                    child.Level = parent.Level + 1;

                    db.Entry(child).State = EntityState.Modified;
                    db.SaveChanges();

                    // 递归更新子类别
                    UpdateChildrenPaths(child.Id);
                }
            }
        }

        // 检查是否循环引用
        private bool IsCircularReference(int categoryId, int? parentId)
        {
            if (!parentId.HasValue)
                return false;

            var currentParent = db.Categories.Find(parentId.Value);

            while (currentParent != null)
            {
                if (currentParent.Id == categoryId)
                    return true;

                if (currentParent.ParentId.HasValue)
                {
                    currentParent = db.Categories.Find(currentParent.ParentId.Value);
                }
                else
                {
                    currentParent = null;
                }
            }

            return false;
        }

        #endregion
    }

    // 排序模型
    public class SortModel
    {
        public int ID { get; set; }
        public int Sort { get; set; }
    }
}