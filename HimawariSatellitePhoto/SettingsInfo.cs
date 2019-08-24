using System;

namespace HimawariSatellitePhoto
{
    public class ImageInfo
    {
        public DateTime Date { get; set; }
        public string File { get; set; }
    }

    public class SettingsInfo
    {
        public string Dimension { get; set; }
        public string Resolution { get; set; }
        public string Date { get; set; }
        public DateTime DateRaw { get; set; }
    }
}