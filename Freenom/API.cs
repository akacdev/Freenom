using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Freenom
{
    public static class API
    {
        public const int MaxRetries = 3;
        public const int RetryDelay = 1000 * 3;
        public const int ExtraDelay = 1000;
        public const int PreviewMaxLength = 500;

        public static async Task<HttpResponseMessage> Request
        (
            this HttpClient cl,
            HttpMethod method,
            string path,
            HttpContent content = null,
            HttpStatusCode target = HttpStatusCode.OK)
        {
            int retries = 0;

            HttpResponseMessage res = null;

            while (res is null || !target.HasFlag(res.StatusCode) && retries < MaxRetries)
            {
                HttpRequestMessage req = new(method, path)
                {
                    Content = content
                };

                res = await cl.SendAsync(req);

                if (!target.HasFlag(res.StatusCode) && (int)res.StatusCode >= 500) await Task.Delay(RetryDelay);
                else break;

                retries++;
            }

            if (!target.HasFlag(res.StatusCode))
            {
                string text = await res.Content.ReadAsStringAsync();
                throw new FreenomException($"Received an unexpected response at {path}: {res.StatusCode} => {text[..Math.Min(PreviewMaxLength, text.Length)]}");
            }

            return res;
        }
    }
}