using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Academy.ViewModels
{
    public class MediaListViewModel
    {
        public List<MediaItemViewModel> MediaList { get; set; }
        public List<CategoryFilter> Categories { get; set; }
        public int TotalCount { get; set; }
    }
}