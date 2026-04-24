using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using Academy.Models;
using System.Data;
using Academy.Common;
using System.IO;
using System.Text;

namespace Academy.Controllers
{
    public class ContactController : WebController
    {
        public ActionResult Index()
        {
            // 從 DictSets 讀取各項設定
            ViewBag.Address = GetDictValue("Contact_Address");
            ViewBag.Phone = GetDictValue("Contact_Phone");
            ViewBag.Email = GetDictValue("Contact_Email");
            ViewBag.Fax = GetDictValue("Contact_Fax");
            ViewBag.MapLongitude = GetDictValue("Contact_MapLongitude");
            ViewBag.MapLatitude = GetDictValue("Contact_MapLatitude");
            ViewBag.OnlineBookingText = GetDictValue("Contact_OnlineBookingText");
            ViewBag.BookingInquiry = GetDictValue("Contact_BookingInquiry");
            ViewBag.BusinessHours = GetDictValue("Contact_BusinessHours");
            ViewBag.TrafficGuide = GetDictValue("Contact_TrafficGuide");
            ViewBag.MapUrl = GetDictValue("Contact_MapUrl");

            return View();
        }

        private string GetDictValue(string code)
        {
            var dict = db.DictSets.FirstOrDefault(d => d.Code == code);
            return dict?.Value ?? "";
        }

        [HttpPost]
        public JsonResult PostMsg(string name, string phone, string email, string date,
                          string timeSlot, string guests, string eventType, string note)
        {
            try
            {
                // 必填檢查
                if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(phone) ||
                    string.IsNullOrWhiteSpace(date) || string.IsNullOrWhiteSpace(timeSlot) ||
                    string.IsNullOrWhiteSpace(guests))
                {
                    return Json(new { success = false, message = "請填寫所有必填欄位" });
                }

                if (!DateTime.TryParse(date, out DateTime bookingDate))
                {
                    return Json(new { success = false, message = "日期格式無效" });
                }

                var msg = new Message
                {
                    UserName = name.Trim(),
                    Tel = phone.Trim(),
                    Mail = email?.Trim(),
                    BookingDate = bookingDate,
                    TimeSlot = timeSlot,
                    Guests = guests,
                    EventType = eventType,
                    Content = note,
                    Status = 0,
                    CDate = DateTime.Now
                };

                db.Messages.Add(msg);   // 假設 DbSet 名稱為 Messages
                db.SaveChanges();

                return Json(new { success = true, message = "訊息已送出，我們將盡快與您聯繫！" });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "系統錯誤，請稍後再試" });
            }
        }
    }
}