using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataUsage
{
    public class Service
    {
        public string Category { get; set; }
        public float Cap { get; set; }
        public float Total { get; set; }
        public UsageDetail[] DailyDetail { get; set; }
        public UsageDetail[] History { get; set; }

        public Service(HtmlNode container)
        {
            Category = container.SelectSingleNode("header").InnerText.Trim();
            Cap = container.SelectNodes("descendant::*[contains(@class, 'cap')]").Select(d => float.Parse(d.InnerText)).Sum();
            Total = container.SelectNodes("descendant::*[contains(@class, 'total')]").Select(d => float.Parse(d.InnerText)).Sum();
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine(Category);
            sb.AppendFormat("Cap: {0}, Total {1}", Cap, Total).AppendLine();
            sb.AppendLine("Daily");
            sb.Append(String.Join(Environment.NewLine, DailyDetail.Select(d => d.ToString()).ToArray())).AppendLine();
            sb.AppendLine("History");
            sb.Append(String.Join(Environment.NewLine, History.Select(d => d.ToString()).ToArray())).AppendLine();
            return sb.ToString();
        }
    }
}
