using ModdingManager.classes.extentions;
using ModdingManager.classes.gfx;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Tga;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using TeximpNet;
using TeximpNet.Compression;
using TeximpNet.DDS;
namespace ModdingManager.managers.utils
{
    public class ModManager
    {
        public static string Directory;
        public static bool IsDebugRuning;
        public static string GameDirectory;
    }
}
