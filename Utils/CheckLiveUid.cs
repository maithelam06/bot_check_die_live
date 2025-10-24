using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckLiveBot
{
    public class CheckLiveUid
    {
        public static async Task<string> CheckLiveAsync(string uid)
        {
            try
            {
                using var httpClient = new HttpClient();

                // Bật HTTP Keep-Alive
                httpClient.DefaultRequestHeaders.Connection.Add("keep-alive");

                // Set headers mặc định
                httpClient.DefaultRequestHeaders.Add("User-Agent",
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) " +
                    "AppleWebKit/537.36 (KHTML, like Gecko) " +
                    "Chrome/74.0.3729.131 Safari/537.36");
                httpClient.DefaultRequestHeaders.Accept.ParseAdd(
                    "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");

                // Gửi request
                var response = await httpClient.GetAsync($"https://graph.facebook.com/{uid}/picture?redirect=false");
                var cRes = await response.Content.ReadAsStringAsync();

                if (!string.IsNullOrEmpty(cRes))
                {
                    if (cRes.Contains("height") && cRes.Contains("width"))
                    {
                        return "live";
                    }
                    return "die";
                }
            }
            catch
            {
                return "error";
            }

            return null;
        }

    }
}
