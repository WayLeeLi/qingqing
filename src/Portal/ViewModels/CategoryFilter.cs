using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Academy.ViewModels
{
    public class CategoryFilter
    {
        public string Name { get; set; }
        public string Tag { get; set; }           // 用于筛选的标识（等同于 Name）
    }
}