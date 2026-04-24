using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Academy.ViewModels
{
    public class StatItem
    {
        public string Number { get; set; }    // 如 "1977"
        public string Label { get; set; }     // 如 "創立年份"
        public string SubLabel { get; set; }  // 如 "FOUNDED"
    }
}