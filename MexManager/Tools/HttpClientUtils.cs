using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace MexManager.Tools
{
    public static class HttpClientUtils
    {
        public static async Task DownloadFileTaskAsync(this HttpClient client, Uri uri, string FileName)
        {
            using (var s = await client.GetStreamAsync(uri))
            {
                using (var fs = new FileStream(FileName, FileMode.Create))
                {
                    await s.CopyToAsync(fs);
                }
            }
        }
    }
}
