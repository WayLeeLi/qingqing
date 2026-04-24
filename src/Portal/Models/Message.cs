using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Academy.Models
{
    public class Message
    {
        [Key]
        public int ID { get; set; }

        [DisplayName("聯絡人")]
        public string UserName { get; set; }

        [DisplayName("電話")]
        public string Tel { get; set; }

        [DisplayName("公司名稱")]
        public string CompanyName { get; set; }

        [DisplayName("類別名稱")]
        public string CategoryName { get; set; }

        [DisplayName("電子信箱")]
        public string Mail { get; set; }

        [DisplayName("諮詢內容")]
        public string Content { get; set; }

        [DisplayName("狀態")]
        public int Status { get; set; }

        [DisplayName("回覆人")]
        public int? ReplyUser { get; set; }

        [DisplayName("回覆時間")]
        public DateTime? ReplyDate { get; set; }

        [DisplayName("回覆內容")]
        public string ReplyContent { get; set; }

        [DisplayName("創建人")]
        public int? CUser { get; set; }

        [DisplayName("創建時間")]
        public DateTime? CDate { get; set; }

        [DisplayName("修改人")]
        public int? LUser { get; set; }

        [DisplayName("修改時間")]
        public DateTime? LDate { get; set; }

        // ===== 新增欄位 =====
        [DisplayName("用餐日期")]
        [DataType(DataType.Date)]
        public DateTime? BookingDate { get; set; }

        [DisplayName("用餐時段")]
        [StringLength(50)]
        public string TimeSlot { get; set; }

        [DisplayName("用餐人數")]
        [StringLength(50)]
        public string Guests { get; set; }

        [DisplayName("宴席類型")]
        [StringLength(100)]
        public string EventType { get; set; }
    }
}