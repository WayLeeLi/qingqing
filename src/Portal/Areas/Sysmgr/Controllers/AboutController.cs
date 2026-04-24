using Academy.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using Academy.Models;
using System.IO;
using System.Data;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
namespace Academy.Areas.Sysmgr.Controllers
{
    public class AboutController : BaseController
    {
        /// <summary>
        /// 餐廳介紹
        /// </summary>
        /// <returns></returns>
        public ActionResult Info()
        {
            if (!db.DictSets.Any(a => a.Code == "AboutInfo"))
            {
                DictSet model = new DictSet();
                model.Code = "AboutInfo";
                model.Name = "關於青青";
                model.Value = "";
                db.DictSets.Add(model);
                db.SaveChanges();
            }

            ViewBag.Dict = db.DictSets.ToDictionary(k => k.Code, v => v.Value);

            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Info(FormCollection collection)
        {
            if (ModelState.IsValid)
            {
                var list = db.DictSets;
                foreach (var item in list)
                {
                    string val = collection.Get(item.Code);
                    if (val != null)
                    {
                        item.Value = val;
                    }
                }
                db.SaveChanges();

                return Json(new { success = true });
            }
            else
            {
                return Json(new { success = false, msg = "保存失敗，請重試！" });
            }
        }

        /// <summary>
        /// 餐廳介紹
        /// </summary>
        /// <returns></returns>
        public ActionResult OurStory()
        {
            if (!db.DictSets.Any(a => a.Code == "OurStory"))
            {
                DictSet model = new DictSet();
                model.Code = "OurStory";
                model.Name = "餐廳介紹";
                model.Value = "";
                db.DictSets.Add(model);
                db.SaveChanges();
            }

            ViewBag.Dict = db.DictSets.ToDictionary(k => k.Code, v => v.Value);

            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult OurStory(FormCollection collection)
        {
            if (ModelState.IsValid)
            {
                var list = db.DictSets;
                foreach (var item in list)
                {
                    string val = collection.Get(item.Code);
                    if (val != null)
                    {
                        item.Value = val;
                    }
                }
                db.SaveChanges();

                return Json(new { success = true });
            }
            else
            {
                return Json(new { success = false, msg = "保存失敗，請重試！" });
            }
        }


        /// <summary>
        /// 主廚介紹
        /// </summary>
        /// <returns></returns>
        public ActionResult chefInfo()
        {
            if (!db.DictSets.Any(a => a.Code == "AboutChef"))
            {
                DictSet model = new DictSet();
                model.Code = "AboutChef";
                model.Name = "主廚介紹";
                model.Value = "";
                db.DictSets.Add(model);
                db.SaveChanges();
            }

            ViewBag.Dict = db.DictSets.ToDictionary(k => k.Code, v => v.Value);

            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult ChefInfo(FormCollection collection)
        {
            if (ModelState.IsValid)
            {
                var list = db.DictSets;
                foreach (var item in list)
                {
                    string val = collection.Get(item.Code);
                    if (val != null)
                    {
                        item.Value = val;
                    }
                }
                db.SaveChanges();

                return Json(new { success = true });
            }
            else
            {
                return Json(new { success = false, msg = "保存失敗，請重試！" });
            }
        }

        /// <summary>
        /// 學歷榮耀
        /// </summary>
        /// <returns></returns>
        public ActionResult AcademicHonors()
        {
            if (!db.DictSets.Any(a => a.Code == "AboutAcademicHonors"))
            {
                DictSet model = new DictSet();
                model.Code = "AboutAcademicHonors";
                model.Name = "學歷榮耀";
                model.Value = "";
                db.DictSets.Add(model);
                db.SaveChanges();
            }

            ViewBag.Dict = db.DictSets.ToDictionary(k => k.Code, v => v.Value);

            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult AcademicHonors(FormCollection collection)
        {
            if (ModelState.IsValid)
            {
                var list = db.DictSets;
                foreach (var item in list)
                {
                    string val = collection.Get(item.Code);
                    if (val != null)
                    {
                        item.Value = val;
                    }
                }
                db.SaveChanges();

                return Json(new { success = true });
            }
            else
            {
                return Json(new { success = false, msg = "保存失敗，請重試！" });
            }
        }

        /// <summary>
        /// 廚藝哲學
        /// </summary>
        /// <returns></returns>
        public ActionResult PhilosophyCooking()
        {
            if (!db.DictSets.Any(a => a.Code == "AboutPhilosophyCooking"))
            {
                DictSet model = new DictSet();
                model.Code = "AboutPhilosophyCooking";
                model.Name = "廚藝哲學";
                model.Value = "";
                db.DictSets.Add(model);
                db.SaveChanges();
            }

            ViewBag.Dict = db.DictSets.ToDictionary(k => k.Code, v => v.Value);

            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult PhilosophyCooking(FormCollection collection)
        {
            if (ModelState.IsValid)
            {
                var list = db.DictSets;
                foreach (var item in list)
                {
                    string val = collection.Get(item.Code);
                    if (val != null)
                    {
                        item.Value = val;
                    }
                }
                db.SaveChanges();

                return Json(new { success = true });
            }
            else
            {
                return Json(new { success = false, msg = "保存失敗，請重試！" });
            }
        }

        /// <summary>
        /// 台菜三寶
        /// </summary>
        /// <returns></returns>
        public ActionResult TaiwaneseTrio()
        {
            if (!db.DictSets.Any(a => a.Code == "AboutTaiwaneseTrio"))
            {
                DictSet model = new DictSet();
                model.Code = "AboutTaiwaneseTrio";
                model.Name = "台菜三寶";
                model.Value = "";
                db.DictSets.Add(model);
                db.SaveChanges();
            }

            ViewBag.Dict = db.DictSets.ToDictionary(k => k.Code, v => v.Value);

            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult TaiwaneseTrio(FormCollection collection)
        {
            if (ModelState.IsValid)
            {
                var list = db.DictSets;
                foreach (var item in list)
                {
                    string val = collection.Get(item.Code);
                    if (val != null)
                    {
                        item.Value = val;
                    }
                }
                db.SaveChanges();

                return Json(new { success = true });
            }
            else
            {
                return Json(new { success = false, msg = "保存失敗，請重試！" });
            }
        }

        /// <summary>
        /// 三代傳承
        /// </summary>
        /// <returns></returns>
        public ActionResult TriLegacy()
        {
            if (!db.DictSets.Any(a => a.Code == "AboutTriLegacy"))
            {
                DictSet model = new DictSet();
                model.Code = "AboutTriLegacy";
                model.Name = "三代傳承";
                model.Value = "";
                db.DictSets.Add(model);
                db.SaveChanges();
            }

            ViewBag.Dict = db.DictSets.ToDictionary(k => k.Code, v => v.Value);

            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult TriLegacy(FormCollection collection)
        {
            if (ModelState.IsValid)
            {
                var list = db.DictSets;
                foreach (var item in list)
                {
                    string val = collection.Get(item.Code);
                    if (val != null)
                    {
                        item.Value = val;
                    }
                }
                db.SaveChanges();

                return Json(new { success = true });
            }
            else
            {
                return Json(new { success = false, msg = "保存失敗，請重試！" });
            }
        }
    }
}
