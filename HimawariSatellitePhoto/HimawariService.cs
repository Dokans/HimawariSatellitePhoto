using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;

namespace HimawariSatellitePhoto
{
    public class HimawariService
    {
        private const string BaseUrl = "http://himawari8-dl.nict.go.jp/himawari8/img/D531106/";
        private static readonly string InfoUrl = $"{BaseUrl}latest.json";
        private readonly int _dimension;
        private DateTime LastImagesDate = DateTime.MinValue;

        public HimawariService(int dimension = 4)
        {
            _dimension = dimension;
        }

        private SettingsInfo GetImageInfo()
        {
            Console.WriteLine("Getting last image info");
            SettingsInfo ret = null;
            using (var wc = new WebClient())
            {
                try
                {
                    var jsonInfo = wc.DownloadString(InfoUrl);
                    var imgInfo = JsonConvert.DeserializeObject<ImageInfo>(jsonInfo);
                    ret = new SettingsInfo
                    {
                        Date = $"{imgInfo.Date:yyyy}/{imgInfo.Date:MM}/{imgInfo.Date:dd}/{imgInfo.Date:HHmmss}",
                        DateRaw = imgInfo.Date,
                        Dimension = string.Format("{0:D}d", _dimension),
                        Resolution = 550.ToString()
                    };
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Got info\nLast image from {imgInfo.Date:G}");
                    Console.ResetColor();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    //throw;
                }
            }

            return ret;
        }

        public void DownloadFullImage()
        {
            var settings = GetImageInfo();

            if (settings.DateRaw <= this.LastImagesDate)
            {
                Console.WriteLine($"{DateTime.Now:t} no new images. Last from {this.LastImagesDate:t}");
                return;
            }

            this.LastImagesDate = settings.DateRaw;
            var url = $"{BaseUrl}{settings.Dimension}/550/{settings.Date}";
            var images = new List<Image<Rgba32>>();
            var imgCounts = _dimension * _dimension;
            Console.WriteLine($"Starting downloading images at {DateTime.Now:t} \n");

            using (var wc = new WebClient())
            {
                for (var i = 0; i < _dimension; i++)
                for (var j = 0; j < _dimension; j++)
                {
                    Console.Write($"\rDownloading image {i * _dimension + j + 1} out of {imgCounts}");
                    var imgUrl = $"{url}_{i}_{j}.png";
                    var data = wc.DownloadData(imgUrl);
                    
                    images.Add(Image.Load(data));
                }
            }

            using (var img = new Image<Rgba32>(_dimension * 550, _dimension * 550))
            {
                for (var i = 0; i < _dimension; i++)
                for (var j = 0; j < _dimension; j++)
                {
                    var srcImg = images[j + i * _dimension];
                    img.Mutate(o =>
                        o.DrawImage(srcImg, new Point(i * 550, j * 550), 1f)
                    );
                }
                if (!Directory.Exists("Images"))
                {
                    Directory.CreateDirectory("Images");
                }

                var path = Path.Combine("Images", $"earth_{settings.DateRaw:yyyy_MM_dd_HH_mm}.png");
                img.Save(path);
            }
            Console.WriteLine($"\nFinished downloading images at {DateTime.Now:t}");
        }
    }
}