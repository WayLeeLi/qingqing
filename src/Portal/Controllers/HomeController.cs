using Academy.Controllers;
using Academy.Models;
using Academy.ViewModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PagedList;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Academy.Controllers
{
    public class HomeController : WebController
    {
        //
        // GET: /Home/
        public ActionResult Index()
        {
            var model = new HomeViewModel();

            // 1. 關於青青
            var about = db.DictSets.FirstOrDefault(d => d.Code == "AboutInfo");
            if (about != null)
            {
                model.AboutInfo = new ContentItem
                {
                    Title = about.Name,
                    Content = about.Value
                };
            }

            // 2. 招牌菜 (Menu == 3) 关联 Category 获取分类名称
            var dishesQuery = from n in db.Newss
                              join c in db.Categories on n.CataID equals c.Id
                              where n.Status == 1 && n.Menu == 3 && c.Status == 1 && n.IsShowIndex == 1
                              orderby n.CDate descending
                              select new ContentItem
                              {
                                  Id = n.ID,
                                  Title = n.Title,
                                  ImageUrl = n.ImagePath,
                                  Summary = n.Note,
                                  Lable = n.Lable,
                                  Content = n.Content,
                                  PublishDate = n.CDate.Value,
                                  Category = c.Name
                              };
            model.SignatureDishes = dishesQuery.Take(5).ToList();

            // 3. 活動與公告 (Menu == 4) 关联 Category
            // 注意：此处不能直接在 LINQ 中使用 Url.Action，需要先查询到内存再赋值
            var newsQuery = from n in db.Newss
                            join c in db.Categories on n.CataID equals c.Id
                            where n.Status == 1 && n.Menu == 4 && c.Status == 1 && n.IsShowIndex == 1
                            orderby n.CDate descending
                            select new ContentItem
                            {
                                Id = n.ID,
                                Title = n.Title,
                                ImageUrl = n.ImagePath,
                                Summary = n.Note,
                                Content = n.Content,
                                PublishDate = n.CDate.Value,
                                Category = c.Name
                                // LinkUrl 不在此处赋值
                            };
            var newsList = newsQuery.Take(4).ToList();
            // 在内存中循环设置 LinkUrl
            foreach (var item in newsList)
            {
                item.LinkUrl = Url.Action("Detail", "News", new { id = item.Id });
            }
            model.NewsList = newsList;

            // 4. 影音專區 (Menu == 5) 关联 Category
            var videoQuery = from n in db.Newss
                             join c in db.Categories on n.CataID equals c.Id
                             where n.Status == 1 && n.Menu == 5 && c.Status == 1 && n.IsShowIndex == 1
                             orderby n.CDate descending
                             select new ContentItem
                             {
                                 Id = n.ID,
                                 Title = n.Title,
                                 ImageUrl = n.ImagePath,
                                 Summary = n.Note,
                                 Content = n.Content,
                                 PublishDate = n.CDate.Value,
                                 Category = c.Name,
                                 VideoUrl = n.VideoPath
                             };
            model.VideoList = videoQuery.Take(3).ToList();

            // 4. 服务项目 (Menu == 6) 关联 Category
            var serviceQuery = from n in db.Newss
                             join c in db.Categories on n.CataID equals c.Id
                             where n.Status == 1 && n.Menu == 6 && c.Status == 1 && n.IsShowIndex == 1
                             orderby n.CDate descending
                             select new ContentItem
                             {
                                 Id = n.ID,
                                 Title = n.Title,
                                 ImageUrl = n.ImagePath,
                                 IconUrl = c.Icon,
                                 Summary = n.Note,
                                 Content = n.Content,
                                 PublishDate = n.CDate.Value,
                                 Category = c.Name,
                                 VideoUrl = n.VideoPath
                             };
            model.ServiceList = serviceQuery.Take(5).ToList();

            ViewBag.Address = GetDictValue("Contact_Address");
            ViewBag.Phone = GetDictValue("Contact_Phone");

            ViewBag.Address = GetDictValue("Contact_Address");
            ViewBag.Phone = GetDictValue("Contact_Phone");
            ViewBag.Email = GetDictValue("Contact_Email");
            ViewBag.BusinessHours = GetDictValue("Contact_BusinessHours");
            ViewBag.MapUrl = GetDictValue("Contact_MapUrl");

            return View(model);
        }

        /// <summary>
        /// 搜索列表
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public ActionResult Search(int page = 1, string keyWord = "")
        {
            keyWord = (keyWord ?? "").Trim();

            ViewBag.KeyWord = keyWord;

            DateTime dtNow = DateTime.Now;

            var data = db.Newss
                .Where(a => a.Status == 1 || a.Status == 2 && (a.OnDate != null && a.OnDate < dtNow || a.OnDate == null) && (a.OffDate != null && a.OffDate > dtNow || a.OffDate == null))
                .Where(a => (keyWord == "") || keyWord != "" && (a.Note.Contains(keyWord) || a.Note2.Contains(keyWord) || a.Content.Contains(keyWord)))
                .OrderByDescending(a => a.PubDate);

            var data2 = db.Members
                .Where(a => a.Status == 1)
                .Where(a => (keyWord == "") || keyWord != "" && (a.Code.Contains(keyWord) || a.Name.Contains(keyWord) || a.Info.Contains(keyWord)))
                .OrderByDescending(a => a.CDate);

            List<Hashtable> list = new List<Hashtable>();
            foreach (var item in data)
            {
                Hashtable ht = new Hashtable();
                ht.Add("ID", item.ID);
                ht.Add("Title", item.Title ?? "");
                ht.Add("Note", item.Note ?? "");
                ht.Add("CDate", item.CDate == null ? "" : item.CDate.Value.ToString("yyyy/MM/dd"));
                ht.Add("Type", "news");
                list.Add(ht);
            }
            foreach (var item in data2)
            {
                Hashtable ht = new Hashtable();
                ht.Add("ID", item.ID);
                ht.Add("Title", item.Name ?? "");
                ht.Add("Note", item.Info ?? "");
                ht.Add("CDate", item.CDate == null ? "" : item.CDate.Value.ToString("yyyy/MM/dd"));
                ht.Add("Type", "member");
                list.Add(ht);
            }
            var pagedData = list.ToPagedList(pageNumber: page, pageSize: 20);

            return View(pagedData);
        }

        [HttpPost]
        public void Export()
        {
            string content = Server.UrlDecode(Request["export_content"]);
            string title = Server.UrlDecode(Request["export_title"]);

            //輸出的應用類型
            Response.ContentType = "application/vnd.ms-excel";
            //設定編碼方式，若輸出的excel有亂碼，可優先從編碼方面解決
            Response.Charset = "utf-8";
            Response.ContentEncoding = System.Text.Encoding.UTF8;
            //關閉ViewState，此屬性在Page中
            //filenames是自定義的文件名
            Response.AppendHeader("Content-Disposition", "attachment;filename=" + Server.UrlEncode(title) + DateTime.Now.ToString("MMddHHmmss") + ".xls");
            //content是步驟1的html，註意是string類型
            Response.Write(content);
            Response.End();
        }

        [HttpPost]
        public ActionResult PostMsg()
        {
            Message msg = new Message();
            msg.UserName = Request["UserName"];
            msg.Tel = Request["Tel"];
            msg.CompanyName = Request["CompanyName"];
            msg.CategoryName = Request["CategoryName"];
            msg.Mail = Request["Mail"];
            msg.Content = Request["Content"];
            msg.Status = 0;
            msg.CUser = 0;
            msg.CDate = DateTime.Now;
            msg.LUser = 0;
            msg.LDate = DateTime.Now;
            db.Messages.Add(msg);
            db.SaveChanges();

            string content = "";
            content += "<p>聯絡人：" + msg.UserName + "</p>";
            content += "<p>電話：" + msg.Tel + "</p>";
            content += "<p>E-mail：" + msg.Mail + "</p>";
            content += "<p>諮詢內容：" + msg.Content + "</p>";

            MailSet model = db.MailSets.FirstOrDefault();
            //回覆發郵件
            Common.MailHelper.SendEmail(model.ReviceMailAddr, msg.UserName + " 提交了諮詢訊息，請及時查看", content);

            return Json(new { success = true, msg = "提交成功，我們會盡快給您回覆！" });
        }

        [HttpPost]
        public ActionResult PostMemberMsg()
        {
            MemberMsg msg = new MemberMsg();
            msg.MemberID = Convert.ToInt32(Request["MemberID"]);
            msg.CompanyName = Request["CompanyName"];
            msg.UserName = Request["UserName"];
            msg.Tel = Request["Tel"];
            msg.Fax = Request["Fax"];
            msg.Mail = Request["Mail"];
            msg.Addr = Request["Addr"];
            msg.Title = Request["Title"];
            msg.Content = Request["Content"];
            msg.Status = 0;
            msg.CUser = 0;
            msg.CDate = DateTime.Now;
            msg.LUser = 0;
            msg.LDate = DateTime.Now;
            db.MemberMsgs.Add(msg);
            db.SaveChanges();

            string content = "";
            content += "<p>公司名稱：" + msg.CompanyName + "</p>";
            content += "<p>姓名：" + msg.UserName + "</p>";
            content += "<p>電話：" + msg.Tel + "</p>";
            content += "<p>傳真：" + msg.Fax + "</p>";
            content += "<p>E-mail：" + msg.Mail + "</p>";
            content += "<p>地址：" + msg.Addr + "</p>";
            content += "<p>主旨：" + msg.Title + "</p>";
            content += "<p>諮詢內容：" + msg.Content + "</p>";

            var member = db.Members.FirstOrDefault(a => a.ID == msg.MemberID);
            if (member != null)
            {
                MailSet model = db.MailSets.FirstOrDefault();
                //回覆發郵件
                Common.MailHelper.SendEmail(member.EMail, msg.CompanyName + " 公司通過青青餐廳網站提交了諮詢訊息，請及時回復", content);

                return Json(new { success = true, msg = "提交成功，我們會盡快給您回覆！" });
            }
            else
            {
                return Json(new { success = true, msg = "提交失敗，請稍後再試！" });
            }
        }

        private string GetDictValue(string code)
        {
            var dict = db.DictSets.FirstOrDefault(d => d.Code == code);
            return dict?.Value ?? "";
        }
    }
}
