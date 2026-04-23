using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Academy.ViewModels
{
    // ContentItem.cs - 单个内容项的统一模型
    public class ContentItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Lable { get; set; }
        public string ImageUrl { get; set; }
        public string Content { get; set; }      // 详细内容（HTML）
        public string Summary { get; set; }      // 简介
        public DateTime PublishDate { get; set; }
        public string Category { get; set; }     // 分类名称
        public string LinkUrl { get; set; }      // 跳转链接
        public string VideoUrl { get; set; }      // 视频链接
        public int OpenType { get; set; }        // 链接打开方式（0=当前页，1=新窗口）
        public int Sort { get; set; }            // 排序
    }
}