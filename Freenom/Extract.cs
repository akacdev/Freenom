using HtmlAgilityPack;
using System;
using System.Linq;
using System.Web;

namespace Freenom
{
    public static class Extract
    {
        public const string CSRFXPath = "//input[@name='token']";
        public static string CSRF(string html)
        {
            HtmlDocument doc = new();
            doc.LoadHtml(html);

            HtmlNode node = doc.DocumentNode.SelectSingleNode(CSRFXPath);
            if (node is null) throw new FreenomException("Exception while parsing HTML for CSRF tokens: Couldn't find a matching DOM element.");

            return node.GetAttributeValue("value", null);
        }

        public const string HelloPrefix = "Hello ";
        public const string HelloXPath = "//h1[@class='splash']";
        public static string Name(string html)
        {
            HtmlDocument doc = new();
            doc.LoadHtml(html);

            HtmlNode node = doc.DocumentNode.SelectSingleNode(HelloXPath);
            if (node is null) throw new FreenomException("Exception while parsing HTML for account name: Couldn't find a matching DOM element.");

            int helloIndex = node.InnerText.IndexOf(HelloPrefix);
            if (helloIndex == -1) throw new FreenomException("Exception while parsing HTML for account name: Couldn't find a target index in the element.");

            return node.InnerText[(helloIndex + HelloPrefix.Length)..];
        }

        public const string FirstNameXPath = "//input[@id='firstname']";
        public const string LastNameXPath = "//input[@id='lastname']";
        public const string EmailXPath = "//input[@id='email']";
        public const string PhoneXPath = "//input[@id='phonenumber']";
        public static AccountInfo AccountInfo(string html)
        {
            HtmlDocument doc = new();
            doc.LoadHtml(html);

            return new AccountInfo()
            {
                FirstName = doc.DocumentNode.SelectSingleNode(FirstNameXPath).GetAttributeValue("value", null),
                LastName = doc.DocumentNode.SelectSingleNode(LastNameXPath).GetAttributeValue("value", null),
                Email = doc.DocumentNode.SelectSingleNode(EmailXPath).GetAttributeValue("value", null),
                Phone = doc.DocumentNode.SelectSingleNode(PhoneXPath).GetAttributeValue("value", null)
            };
        }

        public static RenewalDomain[] Renewals(string html)
        {
            HtmlDocument doc = new();
            doc.LoadHtml(html);

            HtmlNodeCollection trs = doc.DocumentNode.SelectNodes("//table/tr");
            if (trs is null) throw new FreenomException("Exception while parsing HTML for an order number: Couldn't find a matching DOM element.");

            RenewalDomain[] renewals = new RenewalDomain[trs.Count];

            for (int i = 0; i < renewals.Length; i++)
            {
                HtmlNodeCollection tds = trs[i].SelectNodes("td");

                RenewalDomain renewal = new()
                {
                    Value = tds[0].InnerText,
                    Status = tds[1].InnerText
                };

                renewals[i] = renewal;

                HtmlNode remaining = tds[2].ChildNodes.FirstOrDefault(x => x.NodeType == HtmlNodeType.Element);
                if (remaining is null) throw new FreenomException("Exception while getting renewals: Domain remaining duration element is missing.");

                if (remaining.HasClass("textred")) renewal.Color = DomainColor.Red;
                else if (remaining.HasClass("textgreen")) renewal.Color = DomainColor.Green;

                string rawRemaining = remaining.InnerText[..remaining.InnerText.IndexOf(' ')];
                renewal.Remaining = TimeSpan.FromDays(int.Parse(rawRemaining));

                HtmlNode message = tds[3].ChildNodes.FirstOrDefault(x => x.NodeType == HtmlNodeType.Element);
                if (message is null) throw new FreenomException("Exception while getting renewals: Message element is missing.");

                renewal.Message = message.InnerText;

                string href = tds[4].ChildNodes.FirstOrDefault(x => x.NodeType == HtmlNodeType.Element).GetAttributeValue("href", null);
                if (href is null) throw new FreenomException("Exception while getting renewals: Href element is missing.");

                renewal.Uri = new($"https://{href}");
                renewal.Id = long.Parse(HttpUtility.ParseQueryString(renewal.Uri.Query)["domain"]);
            }

            return renewals;
        }

        public static long OrderNumber(string html)
        {
            HtmlDocument doc = new();
            doc.LoadHtml(html);

            HtmlNode node = doc.DocumentNode.SelectSingleNode("//strong");
            if (node is null) throw new FreenomException("Exception while parsing HTML for an order number: Couldn't find a matching DOM element.");

            int from = node.InnerText.LastIndexOf(' ') + 1;
            if (from == -1) throw new FreenomException("Exception while parsing HTML for an order number: Couldn't find a target index in the element.");

            string raw = node.InnerText[from..];

            return long.Parse(raw);
        }
    }
}