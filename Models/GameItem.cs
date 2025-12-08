using System;
using System.Windows.Media;

namespace NewLauncher.Models
{
    public class GameItem
    {
        public string Title { get; set; } = string.Empty;
        public string ExePath { get; set; } = string.Empty;
        public ImageSource? Icon { get; set; }
        public ImageSource? BackgroundImage { get; set; }
        public string PlayTime { get; set; } = "0h";
        public DateTime LastPlayed { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
