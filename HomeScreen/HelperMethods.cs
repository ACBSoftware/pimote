using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media.Imaging;

namespace HomeScreen
{
    public static class HelperMethods
    {
        public static async Task<string> GetCurrentTempAsync(String CitySTForGoogleSearch)
        {
            
            String DisplayTemp = "";
            String GoogleQuery = CitySTForGoogleSearch.Replace(" ","+") + "+" + "Temperature";
            String HTMLFromGoogle = await WebGetUtils.DownloadAStringAsync("https://www.google.com/search?q=" + GoogleQuery);

            //Looking for: <span class="wob_t" style="display:inline">70°F</span>
            Int32 IndexOfTemp = HTMLFromGoogle.IndexOf("class=\"wob_t\""); 
            if (IndexOfTemp > -1)
            {
                HTMLFromGoogle = HTMLFromGoogle.Substring(IndexOfTemp + 37);
                IndexOfTemp = HTMLFromGoogle.IndexOf("</span>");
                if (IndexOfTemp > -1)
                {
                    DisplayTemp = HTMLFromGoogle.Substring(0, IndexOfTemp);
                }
            }
            return DisplayTemp;
        }

        public static async Task<BitmapImage> GetBlueIrisImage(String BlueIrisUrl, String CameraKey)
        {
            BitmapImage image = new BitmapImage();
            Byte[] imageBytes = await WebGetUtils.GetImageBytes(BlueIrisUrl + CameraKey + "?q=20"); //Quality 20 since it's such a small screen...
            using (var ms = new System.IO.MemoryStream(imageBytes))
            {
                await image.SetSourceAsync(ms.AsRandomAccessStream());
            }
            return image; 
        }
        public static string SerializeObject(Object TheObject)
        {
            String Result = "";
            DataContractJsonSerializer serReq = new DataContractJsonSerializer(TheObject.GetType());
            using (System.IO.MemoryStream stream1 = new System.IO.MemoryStream())
            {
                serReq.WriteObject(stream1, TheObject);
                stream1.Position = 0;
                using (System.IO.StreamReader sr = new System.IO.StreamReader(stream1))
                {
                    Result = sr.ReadToEnd();
                }
            }
            return Result;
        }

        public static T DeSerializeObject<T>(string json)
        {
            DataContractJsonSerializer deserializer = new DataContractJsonSerializer(typeof(T));
            using (MemoryStream stream = new MemoryStream(Encoding.Unicode.GetBytes(json)))
            {
                T result = (T)deserializer.ReadObject(stream);
                return result;
            }
        }
     
    }
}
