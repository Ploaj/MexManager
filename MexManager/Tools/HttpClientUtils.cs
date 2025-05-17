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
            using Stream s = await client.GetStreamAsync(uri);
            using FileStream fs = new(FileName, FileMode.Create);
            await s.CopyToAsync(fs);
        }
    }
}
