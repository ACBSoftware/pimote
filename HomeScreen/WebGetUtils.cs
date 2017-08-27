using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace HomeScreen
{
    static class WebGetUtils
    {
        static HttpClient client = new HttpClient();
        public static async Task<string> DownloadAStringAsync(String URL)
        {
            string ReturnValue = "";
            try
            {
                var client = new System.Net.Http.HttpClient();
                ReturnValue = await client.GetStringAsync(new Uri(URL));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error " + ex.ToString());
            }
            return ReturnValue;
        }

        public static async Task<Byte[]> GetImageBytes(String URL)
        {
            Byte[] BytesOfImage = null;
            HttpResponseMessage response = await client.GetAsync(URL);
            if (response.IsSuccessStatusCode)
            {
                BytesOfImage = await response.Content.ReadAsByteArrayAsync();
            }
            return BytesOfImage;
        }

    }
}
