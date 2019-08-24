using System;
using System.Threading.Tasks;

namespace HimawariSatellitePhoto
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var service = new HimawariService(8);

            while (true)
            {
                service.DownloadFullImage();
                Task.Delay(TimeSpan.FromMinutes(5)).Wait();
            }
            
        }
    }
}