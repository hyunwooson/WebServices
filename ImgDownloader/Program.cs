using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace ImgDownloader
{
    class Program
    {
        static void Main(string[] args)
        {

            #region downloader
            Dictionary<string,string> urlList = new Dictionary<string, string>();

            string[] urls = new string[] {
                @"http://www.gstatic.com/images/icons/material/apps/weather/2x/wintry_mix_rain_snow_light_color_96dp.png" ,
                @"http://www.gstatic.com/images/icons/material/apps/weather/2x/haze_fog_dust_smoke_light_color_96dp.png" ,
                @"http://www.gstatic.com/images/icons/material/apps/weather/2x/cloudy_light_color_96dp.png" ,
                @"http://www.gstatic.com/images/icons/material/apps/weather/2x/snow_showers_snow_light_color_96dp.png" ,
                @"http://www.gstatic.com/images/icons/material/apps/weather/2x/flurries_light_color_96dp.png" ,
                @"http://www.gstatic.com/images/icons/material/apps/weather/2x/drizzle_light_color_96dp.png" ,
                @"http://www.gstatic.com/images/icons/material/apps/weather/2x/showers_rain_light_color_96dp.png" ,
                @"http://www.gstatic.com/images/icons/material/apps/weather/2x/heavy_rain_light_color_96dp.png" ,
                @"http://www.gstatic.com/images/icons/material/apps/weather/2x/strong_tstorms_light_color_96dp.png" ,
                @"http://www.gstatic.com/images/icons/material/apps/weather/2x/isolated_scattered_tstorms_day_light_color_96dp.png" ,
                @"http://www.gstatic.com/images/icons/material/apps/weather/2x/isolated_scattered_tstorms_night_light_color_96dp.png" ,
                @"http://www.gstatic.com/images/icons/material/apps/weather/2x/scattered_showers_day_light_color_96dp.png" ,
                @"http://www.gstatic.com/images/icons/material/apps/weather/2x/scattered_showers_night_light_color_96dp.png" ,
                @"http://www.gstatic.com/images/icons/material/apps/weather/2x/partly_cloudy_light_color_96dp.png" ,
                @"http://www.gstatic.com/images/icons/material/apps/weather/2x/partly_cloudy_night_light_color_96dp.png" ,
                @"http://www.gstatic.com/images/icons/material/apps/weather/2x/mostly_sunny_light_color_96dp.png" ,
                @"http://www.gstatic.com/images/icons/material/apps/weather/2x/mostly_clear_night_light_color_96dp.png" ,
                @"http://www.gstatic.com/images/icons/material/apps/weather/2x/mostly_cloudy_day_light_color_96dp.png" ,
                @"http://www.gstatic.com/images/icons/material/apps/weather/2x/mostly_cloudy_night_light_color_96dp.png" ,
                @"http://www.gstatic.com/images/icons/material/apps/weather/2x/sunny_light_color_96dp.png" ,
                @"http://www.gstatic.com/images/icons/material/apps/weather/2x/clear_night_light_color_96dp.png" ,
            };

            foreach (var _url in urls)
            {
                urlList.Add(_url.Split('/')[_url.Split('/').Length - 1].Replace("_light_color_96dp",""), _url);
            }

            foreach (var url in urlList)
            {
                using (var client = new WebClient())
                {
                    client.DownloadFile(url.Value, $"{url.Key}.png");
                }
            }
            #endregion

            #region
            //foreach (var file in Directory.GetFiles(Directory.GetCurrentDirectory(), "*.png"))
            //{
            //    //var tempName =file.ToLowerInvariant();
            //    //File.Move(file, tempName);
            //    File.Move(file, file.ToLowerInvariant());
            //}
            #endregion
        }
    }
}
