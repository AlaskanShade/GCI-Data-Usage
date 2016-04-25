using ScrapySharp.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCI_Data_Usage
{
    public static class GCIUsageChecker
    {
        public static IEnumerable<Service> Check(string username, string password)
        {
            var sb = new ScrapingBrowser();
            sb.AllowAutoRedirect = true;
            sb.AllowMetaRedirect = true;
            var loginPage = sb.NavigateToPage(new Uri("https://login.gci.com"));
            var loginForm = loginPage.FindFormById("login-form");
            loginForm["username"] = username;
            loginForm["password"] = password;
            var resultPage = loginForm.Submit();
            loginForm = resultPage.FindFormById("login-form");
            if (loginForm != null)
                throw new UsageException("Login failed");

            var usagePage = sb.NavigateToPage(new Uri("https://apps.gci.com/um/overview"));

            var services = new List<Service>();
            foreach (var container in usagePage.Html.SelectNodes("//section[contains(@class, 'services-container')]"))
            {
                var service = new Service(container);
                services.Add(service);

                // load detail
                var linkDetail = container.SelectSingleNode("descendant::a[starts-with(@href,'/um/service/')]");
                if (linkDetail != null)
                {
                    var detailPage = sb.NavigateToPage(new Uri(new Uri("https://apps.gci.com"), linkDetail.Attributes["href"].Value));
                    var dailyRows = detailPage.Html.SelectNodes("//tr[contains(@class, 'data')]");
                    service.DailyDetail = dailyRows.Select(r => new UsageDetail(r)).ToArray();
                    var historyRows = detailPage.Html.SelectNodes("//table[@class='data-usage-table']/tbody/tr");
                    service.History = historyRows.Select(h => new UsageDetail(h)).ToArray();
                }
            }
            return services;
        }
    }
}
