using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VGraph.src.dataLayers
{
    public interface BackgroundLayer : IDataLayer
    {
        public bool DrawCenterLines { get; set; }
        public bool DrawGridLines { get; set; }
        public bool DrawBackgroundImage { get; set; }

        public bool SetBackgroundImage(string path);
        public bool ToggleCenterLines();
        public bool ToggleGridLines();
        public bool ToggleBackgroundImage();
    }
}
