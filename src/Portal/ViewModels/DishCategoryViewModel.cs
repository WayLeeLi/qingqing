using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Academy.ViewModels
{
    public class DishCategoryViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }          // 分类名称（三杯類）
        public string IconBlockHtml { get; set; }   // 分类图标 HTML
        public string NameEn { get; set; }        // 英文名称（可选）
        public string DomId { get; set; }         // 用于锚点 ID（sanpei, stir...）
        public int SortOrder { get; set; }
        public List<DishViewModel> Dishes { get; set; }
    }
}