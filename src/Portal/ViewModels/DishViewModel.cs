using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Academy.ViewModels
{
    public class DishViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }          // 菜品名称
        public string ImageUrl { get; set; }      // 主图路径
        public string ThumbUrl { get; set; }      // 缩略图（可复用 ImageUrl）
        public string Summary { get; set; }       // 简介（Note 字段）
        public string Description { get; set; }   // 详细描述（Content 字段）
        public int SortOrder { get; set; }
    }
}