using Academy.Controllers;
using Academy.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace Academy.Areas.Sysmgr.Controllers
{
    public class NewsController : BaseController
    {
        // GET: /Sysmgr/News/Index
        public ActionResult Index(int page = 1, int cata = 0, string status = "", string ordery = "", string menu = "")
        {
            int menuValue = string.IsNullOrEmpty(menu) ? 0 : int.Parse(menu);
            ViewBag.Menu = menuValue;
            ViewBag.MenuName = GetMenuName(menuValue);

            var categories = db.Categories.Where(s => s.Menu == menuValue && s.Status == 1).OrderBy(c => c.Path).ToList();
            var categoryFilterList = new List<SelectListItem>();
            categoryFilterList.Add(new SelectListItem { Value = "", Text = "請選擇" });

            foreach (var cat in categories)
            {
                string prefix = "";
                if (cat.Level > 0)
                {
                    for (int i = 0; i < cat.Level; i++) prefix += "  ";
                    prefix += "└─ ";
                }
                categoryFilterList.Add(new SelectListItem
                {
                    Value = cat.Id.ToString(),
                    Text = prefix + cat.Name,
                    Selected = (cata == cat.Id)
                });
            }
            ViewBag.CategoryFilterList = categoryFilterList;

            var data = db.Newss.Where(a => a.Menu == menuValue).AsQueryable();

            if (cata != 0)
                data = data.Where(a => a.CataID == cata);

            if (!string.IsNullOrEmpty(status))
            {
                int nstatus = int.Parse(status);
                DateTime dtNow = DateTime.Now;
                if (nstatus == 1)
                {
                    data = data.Where(a => a.Status == 1 || (a.Status == 2 && (a.OnDate == null || a.OnDate < dtNow) && (a.OffDate == null || a.OffDate > dtNow)));
                }
                if (nstatus == 0)
                {
                    data = data.Where(a => a.Status == 0 || (a.Status == 2 && (a.OnDate != null && a.OnDate > dtNow || a.OffDate != null && a.OffDate < dtNow)));
                }
            }

            switch (ordery)
            {
                case "pubtimeasc": data = data.OrderBy(a => a.PubDate); break;
                case "pubtimedesc": data = data.OrderByDescending(a => a.PubDate); break;
                case "timeasc": data = data.OrderBy(a => a.CDate); break;
                case "timedesc": data = data.OrderByDescending(a => a.CDate); break;
                case "idasc": data = data.OrderBy(a => a.ID); break;
                case "iddesc": data = data.OrderByDescending(a => a.ID); break;
                case "sortasc": data = data.OrderBy(a => a.Sort); break;
                case "sortdesc": data = data.OrderByDescending(a => a.Sort); break;
                default: data = data.OrderByDescending(a => a.PubDate); break;
            }

            var pagedData = data.ToPagedList(pageNumber: page, pageSize: 12);

            return View(pagedData);
        }

        public ActionResult Search()
        {
            ViewBag.Users = db.Users;
            ViewBag.CataList = db.NewsCatas.OrderBy(a => a.Sort).ThenByDescending(a => a.CDate);
            return View();
        }

        [HttpGet]
        public ActionResult SearchList(int page = 1, string status = "", string ordery = "")
        {
            DateTime startDate = DateTime.Parse("2000-01-01");
            DateTime endDate = DateTime.Now;
            DateTime pubStartDate = DateTime.Parse("2000-01-01");
            DateTime pubEndDate = DateTime.Now;
            int cataId = 0;
            string keyword = "";
            int statusVal = -1;
            int createUser = 0;

            if (!string.IsNullOrEmpty(Request["StartDate"]))
            {
                startDate = DateTime.Parse(Request["StartDate"]);
                ViewBag.StartDate = startDate.ToString("yyyy-MM-dd");
            }
            else ViewBag.StartDate = "開始";

            if (!string.IsNullOrEmpty(Request["EndDate"]))
            {
                endDate = DateTime.Parse(Request["EndDate"]).AddDays(1);
                ViewBag.EndDate = endDate.ToString("yyyy-MM-dd");
            }
            else ViewBag.EndDate = "至今";

            if (!string.IsNullOrEmpty(Request["PubStartDate"]))
            {
                pubStartDate = DateTime.Parse(Request["PubStartDate"]);
                ViewBag.PubStartDate = pubStartDate.ToString("yyyy-MM-dd");
            }
            else ViewBag.PubStartDate = "開始";

            if (!string.IsNullOrEmpty(Request["PubEndDate"]))
            {
                pubEndDate = DateTime.Parse(Request["PubEndDate"]).AddDays(1);
                ViewBag.PubEndDate = pubEndDate.ToString("yyyy-MM-dd");
            }
            else ViewBag.PubEndDate = "至今";

            if (!string.IsNullOrEmpty(Request["CataID"]))
            {
                cataId = int.Parse(Request["CataID"]);
                ViewBag.CataName = Common.GetData.GetNewsCataName(cataId);
            }
            else ViewBag.Type = "未選";

            if (!string.IsNullOrEmpty(Request["KeyWord"]))
            {
                keyword = Request["KeyWord"];
                ViewBag.KeyWord = keyword;
            }
            else ViewBag.KeyWord = "未輸入";

            if (!string.IsNullOrEmpty(Request["Status"]))
            {
                switch (Request["Status"])
                {
                    case "0": statusVal = 0; ViewBag.Status = "下線中"; break;
                    case "1": statusVal = 1; ViewBag.Status = "上線中"; break;
                    default: ViewBag.Status = "全部"; break;
                }
            }
            else ViewBag.Status = "未選擇";

            if (!string.IsNullOrEmpty(Request["CreateUser"]))
            {
                createUser = int.Parse(Request["CreateUser"]);
                ViewBag.CreateUser = Common.GetData.GetUserNameByID(createUser);
            }
            else ViewBag.CreateUser = "未選擇";

            var data = db.Newss.Where(a => a.CDate >= startDate && a.CDate <= endDate && a.PubDate >= pubStartDate && a.PubDate <= pubEndDate).AsQueryable();

            if (cataId != 0) data = data.Where(a => a.CataID == cataId);
            if (!string.IsNullOrEmpty(keyword))
            {
                keyword = keyword.Trim();
                data = data.Where(a => a.Title.Contains(keyword) || a.Dept1.Contains(keyword) || a.Dept2.Contains(keyword) || a.Note.Contains(keyword) || a.Content.Contains(keyword));
            }
            if (statusVal != -1)
            {
                DateTime dtNow = DateTime.Now;
                if (statusVal == 1)
                    data = data.Where(a => a.Status == 1 || (a.Status == 2 && (a.OnDate == null || a.OnDate < dtNow) && (a.OffDate == null || a.OffDate > dtNow)));
                if (statusVal == 0)
                    data = data.Where(a => a.Status == 0 || (a.Status == 2 && (a.OnDate != null && a.OnDate > dtNow || a.OffDate != null && a.OffDate < dtNow)));
            }
            if (createUser != 0) data = data.Where(a => a.CUser == createUser);

            switch (ordery)
            {
                case "pubtimeasc": data = data.OrderBy(a => a.PubDate); break;
                case "pubtimedesc": data = data.OrderByDescending(a => a.PubDate); break;
                case "timeasc": data = data.OrderBy(a => a.CDate); break;
                case "timedesc": data = data.OrderByDescending(a => a.CDate); break;
                case "idasc": data = data.OrderBy(a => a.ID); break;
                case "iddesc": data = data.OrderByDescending(a => a.ID); break;
                case "sortasc": data = data.OrderBy(a => a.Sort); break;
                case "sortdesc": data = data.OrderByDescending(a => a.Sort); break;
                default: data = data.OrderByDescending(a => a.Sort).ThenByDescending(a => a.CDate); break;
            }

            var pagedData = data.ToPagedList(pageNumber: page, pageSize: 12);
            return View(pagedData);
        }

        // ==================== 辅助方法 ====================
        private string GetMenuName(int? menu)
        {
            switch (menu)
            {
                case 3: return "招牌菜色";
                case 4: return "最新消息";
                case 5: return "影音專區";
                default: return "服務項目";
            }
        }

        private void LoadCategorySelectList(int? selectedId = null, int? menu = null)
        {
            int menuValue = menu ?? (Request["menu"] != null ? int.Parse(Request["menu"]) : 0);
            var categories = db.Categories.Where(c => c.Menu == menuValue && c.Status == 1).OrderBy(c => c.Path).ToList();

            var selectList = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "請選擇" }
            };
            foreach (var cat in categories)
            {
                string prefix = "";
                if (cat.Level > 0)
                {
                    for (int i = 0; i < cat.Level; i++) prefix += "  ";
                    prefix += "└─ ";
                }
                selectList.Add(new SelectListItem
                {
                    Value = cat.Id.ToString(),
                    Text = prefix + cat.Name,
                    Selected = (selectedId.HasValue && cat.Id == selectedId.Value)
                });
            }
            ViewBag.CategorySelectList = selectList;
        }

        // ==================== Add ====================
        public ActionResult Add(string menu = "")
        {
            int menuValue = string.IsNullOrEmpty(menu) ? 0 : int.Parse(menu);
            ViewBag.Menu = menuValue;
            ViewBag.MenuName = GetMenuName(menuValue);
            LoadCategorySelectList(null, menuValue);

            News model = new News();
            model.Menu = menuValue;
            model.Status = 1;
            model.PubDate = DateTime.Now;
            return View(model);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Add(News model, HttpPostedFileBase file)
        {
            if (file != null && file.FileName != null && file.FileName.LastIndexOf(".") > 0)
            {
                string ext = file.FileName.Substring(file.FileName.LastIndexOf(".") + 1).ToLower();
                if (ext != "jpg" && ext != "jpeg" && ext != "png" && ext != "gif" && ext != "bmp")
                {
                    ModelState.AddModelError("", "請選擇圖片(僅支持jpg|jpeg|png|gif|bmp格式)!");
                }
                else
                {
                    try
                    {
                        string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                        string guid = Guid.NewGuid().ToString();
                        string fileName = $"{timestamp}_{guid}.{ext}";
                        string virtualPath = $"/Upload/News/{fileName}";
                        string physicalPath = Server.MapPath("~" + virtualPath);

                        string dir = Path.GetDirectoryName(physicalPath);
                        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

                        file.SaveAs(physicalPath);
                        model.ImagePath = virtualPath;

                        // 生成缩略图
                        using (var original = System.Drawing.Image.FromFile(physicalPath))
                        {
                            int thumbWidth = 300;
                            int thumbHeight = (int)((double)original.Height / original.Width * thumbWidth);
                            using (var thumb = new System.Drawing.Bitmap(thumbWidth, thumbHeight))
                            using (var g = System.Drawing.Graphics.FromImage(thumb))
                            {
                                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                                g.DrawImage(original, 0, 0, thumbWidth, thumbHeight);

                                string thumbFileName = $"thumb_{timestamp}_{guid}.jpg";
                                string thumbVirtualPath = $"/Upload/News/{thumbFileName}";
                                string thumbPhysicalPath = Server.MapPath("~" + thumbVirtualPath);
                                thumb.Save(thumbPhysicalPath, System.Drawing.Imaging.ImageFormat.Jpeg);
                                model.ThumbnailPath = thumbVirtualPath;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", "圖片處理失敗：" + ex.Message);
                        if (!string.IsNullOrEmpty(model.ImagePath) && System.IO.File.Exists(Server.MapPath(model.ImagePath)))
                            System.IO.File.Delete(Server.MapPath(model.ImagePath));
                        model.ImagePath = null;
                        model.ThumbnailPath = null;
                        LoadCategorySelectList(model.CataID, model.Menu);
                        ViewBag.MenuName = GetMenuName(model.Menu);
                        return View(model);
                    }
                }
            }
            else
            {
                ModelState.AddModelError("", "請選擇圖片!");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (model.Status != 2)
                    {
                        model.OnDate = null;
                        model.OffDate = null;
                    }
                    model.Sort = (db.Newss.Max(a => a.Sort) ?? 0) + 1;
                    model.CUser = this.LoginUser.ID;
                    model.CDate = DateTime.Now;
                    db.Newss.Add(model);
                    db.SaveChanges();
                    return RedirectToAction("Index", "News", new { menu = model.Menu, success = true });
                }
                catch
                {
                    ModelState.AddModelError("", "操作異常，請重試!");
                    LoadCategorySelectList(model.CataID, model.Menu);
                    ViewBag.MenuName = GetMenuName(model.Menu);
                    return View(model);
                }
            }
            else
            {
                LoadCategorySelectList(model.CataID, model.Menu);
                ViewBag.MenuName = GetMenuName(model.Menu);
                return View(model);
            }
        }

        // ==================== Edit ====================
        public ActionResult Edit(int id, string menu = "")
        {
            News model = db.Newss.FirstOrDefault(p => p.ID == id);
            if (model == null) return RedirectToAction("Index", "News");

            int menuValue = string.IsNullOrEmpty(menu) ? 0 : int.Parse(menu);
            ViewBag.Menu = menuValue;
            ViewBag.MenuName = GetMenuName(menuValue);
            LoadCategorySelectList(model.CataID, menuValue);
            return View(model);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Edit(News model, HttpPostedFileBase file)
        {
            var oldEntity = db.Newss.AsNoTracking().FirstOrDefault(n => n.ID == model.ID);
            if (oldEntity == null) return HttpNotFound();

            bool hasNewFile = false;
            string newImagePath = null;
            string newThumbPath = null;

            // 处理新文件上传
            if (file != null && file.ContentLength > 0)
            {
                string ext = Path.GetExtension(file.FileName).ToLower().TrimStart('.');
                if (!new[] { "jpg", "jpeg", "png", "gif", "bmp" }.Contains(ext))
                {
                    ModelState.AddModelError("", "圖片格式不支援，僅允許 jpg/jpeg/png/gif/bmp");
                }
                else
                {
                    try
                    {
                        string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                        string guid = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 8);
                        string fileName = $"{timestamp}_{guid}.{ext}";
                        string virtualPath = $"/Upload/News/{fileName}";
                        string physicalPath = Server.MapPath("~" + virtualPath);
                        string dir = Path.GetDirectoryName(physicalPath);
                        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

                        file.SaveAs(physicalPath);
                        newImagePath = virtualPath;

                        using (var original = System.Drawing.Image.FromFile(physicalPath))
                        {
                            int thumbWidth = 300;
                            int thumbHeight = (int)((double)original.Height / original.Width * thumbWidth);
                            using (var thumb = new System.Drawing.Bitmap(thumbWidth, thumbHeight))
                            using (var g = System.Drawing.Graphics.FromImage(thumb))
                            {
                                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                                g.DrawImage(original, 0, 0, thumbWidth, thumbHeight);
                                string thumbFileName = $"thumb_{timestamp}_{guid}.jpg";
                                string thumbVirtualPath = $"/Upload/News/{thumbFileName}";
                                string thumbPhysicalPath = Server.MapPath("~" + thumbVirtualPath);
                                thumb.Save(thumbPhysicalPath, System.Drawing.Imaging.ImageFormat.Jpeg);
                                newThumbPath = thumbVirtualPath;
                            }
                        }
                        hasNewFile = true;
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", "圖片處理失敗：" + ex.Message);
                        if (newImagePath != null && System.IO.File.Exists(Server.MapPath(newImagePath)))
                            System.IO.File.Delete(Server.MapPath(newImagePath));
                        newImagePath = null;
                        newThumbPath = null;
                        LoadCategorySelectList(model.CataID, model.Menu);
                        ViewBag.MenuName = GetMenuName(model.Menu);
                        return View(model);
                    }
                }
            }

            bool deleteRequested = !hasNewFile && Request["delimg"] == "1";

            if (hasNewFile)
            {
                if (!string.IsNullOrEmpty(oldEntity.ImagePath))
                {
                    string oldPhysical = Server.MapPath(oldEntity.ImagePath);
                    if (System.IO.File.Exists(oldPhysical)) System.IO.File.Delete(oldPhysical);
                }
                if (!string.IsNullOrEmpty(oldEntity.ThumbnailPath))
                {
                    string oldThumbPhysical = Server.MapPath(oldEntity.ThumbnailPath);
                    if (System.IO.File.Exists(oldThumbPhysical)) System.IO.File.Delete(oldThumbPhysical);
                }
                model.ImagePath = newImagePath;
                model.ThumbnailPath = newThumbPath;
            }
            else if (deleteRequested)
            {
                if (!string.IsNullOrEmpty(oldEntity.ImagePath))
                {
                    string oldPhysical = Server.MapPath(oldEntity.ImagePath);
                    if (System.IO.File.Exists(oldPhysical)) System.IO.File.Delete(oldPhysical);
                }
                if (!string.IsNullOrEmpty(oldEntity.ThumbnailPath))
                {
                    string oldThumbPhysical = Server.MapPath(oldEntity.ThumbnailPath);
                    if (System.IO.File.Exists(oldThumbPhysical)) System.IO.File.Delete(oldThumbPhysical);
                }
                model.ImagePath = null;
                model.ThumbnailPath = null;
            }
            else
            {
                model.ImagePath = oldEntity.ImagePath;
                model.ThumbnailPath = oldEntity.ThumbnailPath;
            }

            if (ModelState.IsValid)
            {
                try
                {
                    model.ReadCount = oldEntity.ReadCount;
                    model.Sort = oldEntity.Sort;
                    model.CUser = oldEntity.CUser;
                    model.CDate = oldEntity.CDate;

                    if (model.Status != 2)
                    {
                        model.OnDate = null;
                        model.OffDate = null;
                    }
                    model.LUser = this.LoginUser.ID;
                    model.LDate = DateTime.Now;

                    db.Entry(model).State = EntityState.Modified;
                    db.SaveChanges();

                    if (Session["ret"] != null)
                    {
                        Response.Redirect(Session["ret"].ToString());
                        return null;
                    }
                    return RedirectToAction("Index", "News", new { menu = model.Menu, success = true });
                }
                catch
                {
                    ModelState.AddModelError("", "操作異常，請重試!");
                    LoadCategorySelectList(model.CataID, model.Menu);
                    ViewBag.MenuName = GetMenuName(model.Menu);
                    return View(model);
                }
            }
            else
            {
                LoadCategorySelectList(model.CataID, model.Menu);
                ViewBag.MenuName = GetMenuName(model.Menu);
                return View(model);
            }
        }

        // ==================== 批量生成缩略图（一次性） ====================
        public ActionResult GenerateThumbnails()
        {
            int successCount = 0, failCount = 0, skipCount = 0;
            var sb = new System.Text.StringBuilder();

            var newsList = db.Newss.Where(n => n.Menu == 3 && !string.IsNullOrEmpty(n.ImagePath)).ToList();
            foreach (var news in newsList)
            {
                if (!string.IsNullOrEmpty(news.ThumbnailPath) && System.IO.File.Exists(Server.MapPath(news.ThumbnailPath)))
                {
                    skipCount++;
                    continue;
                }

                string originalPath = Server.MapPath(news.ImagePath);
                if (!System.IO.File.Exists(originalPath))
                {
                    sb.AppendLine($"⚠️ 文件不存在：{news.ImagePath} (ID:{news.ID})");
                    failCount++;
                    continue;
                }

                try
                {
                    string fileName = Path.GetFileNameWithoutExtension(originalPath);
                    string thumbFileName = $"thumb_{fileName}.jpg";
                    string thumbVirtualPath = $"/Upload/News/{thumbFileName}";
                    string thumbPhysicalPath = Server.MapPath("~" + thumbVirtualPath);

                    using (var original = System.Drawing.Image.FromFile(originalPath))
                    {
                        int thumbWidth = 300;
                        int thumbHeight = (int)((double)original.Height / original.Width * thumbWidth);
                        using (var thumb = new System.Drawing.Bitmap(thumbWidth, thumbHeight))
                        using (var g = System.Drawing.Graphics.FromImage(thumb))
                        {
                            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                            g.DrawImage(original, 0, 0, thumbWidth, thumbHeight);
                            thumb.Save(thumbPhysicalPath, System.Drawing.Imaging.ImageFormat.Jpeg);
                        }
                    }

                    news.ThumbnailPath = thumbVirtualPath;
                    db.Entry(news).State = EntityState.Modified;
                    db.SaveChanges();
                    successCount++;
                    sb.AppendLine($"✅ ID:{news.ID} - {news.Title} 成功");
                }
                catch (Exception ex)
                {
                    sb.AppendLine($"❌ ID:{news.ID} - {news.Title} 失败：{ex.Message}");
                    failCount++;
                }
            }

            string result = $"处理完成：成功 {successCount} 条，失败 {failCount} 条，跳过 {skipCount} 条。\n{sb.ToString()}";
            return Content(result, "text/plain; charset=utf-8");
        }

        // ==================== 排序、删除等 ====================
        [HttpPost]
        public ActionResult Sort(string data)
        {
            JArray dataItems = (JArray)JsonConvert.DeserializeObject(data);
            foreach (JObject item in dataItems)
            {
                int id = Convert.ToInt32(item["ID"].ToString());
                int sort = Convert.ToInt32(item["Sort"].ToString());
                var model = db.Newss.FirstOrDefault(a => a.ID == id);
                if (model != null) model.Sort = sort;
            }
            db.SaveChanges();
            return Json(true);
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            var model = db.Newss.FirstOrDefault(a => a.ID == id);
            if (model != null) db.Newss.Remove(model);
            db.SaveChanges();
            return Json(true);
        }

        [HttpPost]
        public ActionResult Deletes(string data)
        {
            JArray dataItems = (JArray)JsonConvert.DeserializeObject(data);
            foreach (JObject item in dataItems)
            {
                int id = Convert.ToInt32(item["ID"].ToString());
                var model = db.Newss.FirstOrDefault(a => a.ID == id);
                if (model != null) db.Newss.Remove(model);
            }
            db.SaveChanges();
            return Json(true);
        }
    }
}