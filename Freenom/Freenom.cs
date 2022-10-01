using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Freenom
{
    /// <summary>
    /// The base class for interacting with Freenom.
    /// </summary>
    public class FreenomClient
    {
        public static readonly string BaseUrl = "https://my.freenom.com/";
        public static readonly Uri BaseUri = new(BaseUrl);

        private readonly HttpClientHandler HttpHandler = new()
        {
            AutomaticDecompression = DecompressionMethods.All,
            AllowAutoRedirect = false
        };

        private readonly HttpClient Client;

        /// <summary>
        /// Create a new Freenom Client instance.
        /// </summary>
        /// <param name="userAgent">The User Agent to use while sending HTTP requests.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public FreenomClient(string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/106.0.0.0 Safari/537.36")
        {
            if (string.IsNullOrEmpty(userAgent)) throw new ArgumentNullException(nameof(userAgent), "User Agent is null or empty.");

            Client = new(HttpHandler)
            {
                DefaultRequestVersion = new Version(2, 0),
                BaseAddress = BaseUri,
                Timeout = TimeSpan.FromMinutes(1)
            };

            Client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", userAgent);

            Client.DefaultRequestHeaders.Accept.Add(new("text/html"));
            Client.DefaultRequestHeaders.Accept.Add(new("application/xhtml+xml"));
            Client.DefaultRequestHeaders.Accept.Add(new("application/xml", 0.9));
            Client.DefaultRequestHeaders.Accept.Add(new("image/avif", 0.9));
            Client.DefaultRequestHeaders.Accept.Add(new("image/webp", 0.9));
            Client.DefaultRequestHeaders.Accept.Add(new("*/*", 0.8));

            Client.DefaultRequestHeaders.AcceptLanguage.Add(new("en-US"));
            Client.DefaultRequestHeaders.AcceptLanguage.Add(new("en", 0.5));

            Client.DefaultRequestHeaders.Referrer = new("https://my.freenom.com/clientarea.php");
            Client.DefaultRequestHeaders.Add("Origin", "https://my.freenom.com");
        }

        /// <summary>
        /// Login to Freeenom with your email and password. This is necessary before running any other methods.
        /// </summary>
        /// <param name="email">Your account's email.</param>
        /// <param name="password">Your account's password.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="FreenomException"></exception>
        public async Task<string> Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email)) throw new ArgumentNullException(nameof(email), "Email address is null or empty.");
            if (string.IsNullOrEmpty(password)) throw new ArgumentNullException(nameof(password), "Password is null or empty.");

            HttpResponseMessage firstRes = await Client.Request("clientarea.php", HttpMethod.Get, target: HttpStatusCode.Redirect);

            string redirect = firstRes.Headers.Location?.OriginalString;
            if (string.IsNullOrEmpty(redirect)) throw new FreenomException("Exception at login: First response is missing a 'Location' header.");

            HttpResponseMessage csrfRes = await Client.Request(redirect, HttpMethod.Get, target: HttpStatusCode.OK);
            string csrfHtml = await csrfRes.Content.ReadAsStringAsync();
            string csrf = Extract.CSRF(csrfHtml);

            if (HttpHandler.CookieContainer.Count == 0) throw new FreenomException("Exception at login: Missing a cookie.");

            FormUrlEncodedContent content = new(new KeyValuePair<string, string>[]
            {
                new KeyValuePair<string, string>("token", csrf),
                new KeyValuePair<string, string>("username", email),
                new KeyValuePair<string, string>("password", password),
                new KeyValuePair<string, string>("rememberme", "on"),
            });

            HttpResponseMessage loginRes = await Client.Request("dologin.php", HttpMethod.Post, content, HttpStatusCode.Found);
            if (loginRes.Headers.Location is null) throw new FreenomException("Exception at login: Login response is missing a 'Location' header.");

            HttpResponseMessage clientRes = await Client.Request("clientarea.php", HttpMethod.Get, target: HttpStatusCode.OK);
            string clientHtml = await clientRes.Content.ReadAsStringAsync();

            return Extract.Name(clientHtml);
        }

        /// <summary>
        /// Log out and invalidate your session cookie. Always clean after yourself!
        /// </summary>
        /// <returns></returns>
        public async Task Logout()
        {
            await Client.Request("logout.php", HttpMethod.Get, target: HttpStatusCode.Found);
        }

        /// <summary>
        /// Get your account's details. This method only returns the most basic values.
        /// </summary>
        /// <returns>An instance of <see cref="AccountInfo"/> with your account details.</returns>
        public async Task<AccountInfo> GetAccountInfo()
        {
            HttpResponseMessage res = await Client.Request("clientarea.php?action=details", HttpMethod.Get, target: HttpStatusCode.OK);
            string resHtml = await res.Content.ReadAsStringAsync();

            return Extract.AccountInfo(resHtml);
        }

        /// <summary>
        /// Get the entire list of domains registered by your account that might be renewable now or in the future.<br></br>
        /// <b>Make sure to filter your domains by <see cref="RenewalDomain.Renewable"/>!</b>
        /// </summary>
        /// <returns>An instance of <see cref="RenewalDomain"/> with your domains.</returns>
        public async Task<RenewalDomain[]> GetRenewals()
        {
            HttpResponseMessage res = await Client.Request("domains.php?a=renewals", HttpMethod.Get, target: HttpStatusCode.OK);
            string resHtml = await res.Content.ReadAsStringAsync();

            return Extract.Renewals(resHtml);
        }

        /// <summary>
        /// Renew
        /// </summary>
        /// <param name="id">The ID of the domain to renew. Get these with <see cref="GetRenewals"/>.</param>
        /// <param name="months">The renewal period in months.</param>
        /// <returns></returns>
        /// <exception cref="FreenomException"></exception>
        public async Task<long> RenewDomain(long id, int months)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id), "Domain ID is out of range.");
            if (months <= 0) throw new ArgumentOutOfRangeException(nameof(id), "Renewal period has to be a positive value.");
            if (months > 12) throw new ArgumentOutOfRangeException(nameof(id), "Renewal period has to be less or equal to 12 months.");

            HttpResponseMessage csrfRes = await Client.Request($"domains.php?a=renewdomain&domain={id}", HttpMethod.Get, target: HttpStatusCode.OK);
            string csrfHtml = await csrfRes.Content.ReadAsStringAsync();
            string csrf = Extract.CSRF(csrfHtml);

            FormUrlEncodedContent content = new(new KeyValuePair<string, string>[]
            {
                new KeyValuePair<string, string>("token", csrf),
                new KeyValuePair<string, string>("renewalid", id.ToString()),
                new KeyValuePair<string, string>($"renewalperiod[{id}]", $"{months}M"),
                new KeyValuePair<string, string>("paymentmethod", "credit"),
            });

            HttpResponseMessage renewRes = await Client.Request("domains.php?submitrenewals=true", HttpMethod.Post, content, HttpStatusCode.Found);

            string redirect = renewRes.Headers.Location?.OriginalString;
            if (string.IsNullOrEmpty(redirect)) throw new FreenomException("Exception at renewing: Renewal response is missing a 'Location' header.");

            HttpResponseMessage finalRes = await Client.Request(redirect, HttpMethod.Get, target: HttpStatusCode.OK);
            string finalHtml = await finalRes.Content.ReadAsStringAsync();

            return Extract.OrderNumber(finalHtml);
        }
    }
}