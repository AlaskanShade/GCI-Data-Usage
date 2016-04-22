using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataUsage
{
    public class UsageDetail
    {
        public DateTime Day { get; set; }
        public float Downstream { get; set; }
        public float Upstream { get; set; }
        public float Combined { get; set; }
        public float RunningTotal { get; set; }

        public UsageDetail(HtmlAgilityPack.HtmlNode node)
        {
            Day = DateTime.Parse(node.SelectSingleNode("td/span[@class='full-date']").InnerText);
            Downstream = ParseData(node.SelectSingleNode("td[@class='downstream']").InnerText);
            Upstream = ParseData(node.SelectSingleNode("td[@class='upstream']").InnerText);
            Combined = ParseData(node.SelectSingleNode("td[@class='combined']").InnerText);
            var runningTotal = node.SelectSingleNode("td[@class='running-total']");
            if (runningTotal != null)
                RunningTotal = ParseData(runningTotal.InnerText);
        }

        private float ParseData(string input)
        {
            if (input == null) return 0;
            if (input.EndsWith("GB"))
                return float.Parse(input.Substring(0, input.Length - 2)) * 1024;
            return float.Parse(input);
        }

        public override string ToString()
        {
            return String.Format("{0}    {1}    {2}    {3}", Day.ToString("MMMM dd, yyyy"), Downstream, Upstream, Combined);
        }
    }
}
