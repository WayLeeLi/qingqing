using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Academy.ViewModels
{
    public class MediaItemViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string CategoryName { get; set; }
        public string TagColorClass { get; set; }   // CSS class
        public string Source { get; set; }          // 電視台/媒體名稱
        public string Description { get; set; }
        public string YouTubeId { get; set; }
        public string ThumbnailUrl { get; set; }
        public DateTime PublishDate { get; set; }
    }
}