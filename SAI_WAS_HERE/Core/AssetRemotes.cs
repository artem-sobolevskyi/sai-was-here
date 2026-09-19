using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Threading.Tasks;

namespace SAI_WAS_HERE
{
    internal static class AssetRemotes
    {
        private const string RemotesUrl =
            "https://raw.githubusercontent.com/artem-sobolevskyi/gfn-assets/main/jsons/remotes.json";

        private static JObject _cache;

        public static async Task<string> GetUrlAsync(string key)
        {
            if (_cache == null)
            {
                using (var wc = new WebClient())
                {
                    wc.Headers.Add("User-Agent", "SAI_WAS_HERE");
                    string json = await wc.DownloadStringTaskAsync(RemotesUrl);
                    _cache = JObject.Parse(json);
                }
            }

            return _cache[key]?.ToString();
        }
    }
}
