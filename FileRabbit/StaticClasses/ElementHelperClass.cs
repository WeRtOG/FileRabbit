using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FileRabbit.ViewModels;

namespace FileRabbit.StaticClasses
{ 
    public static class ElementHelperClass
    {
        private const int KyloBiteSize = 1024;
        private const int MaxByteSize = 1000;

        private static List<string> docTypes = new List<string> { ".doc", ".docx", ".xls", ".docm", ".dot",
            ".dotm", ".epub", ".fb2", ".ibooks", ".indd", ".key", ".mobi", ".odt", ".pdf", ".pages", ".pps",
            ".ppsm", ".ppsx", ".ppt", ".pptm", ".pptx", ".pub", ".rtf", ".wpd", ".wps", ".xls", ".xlsb",
            ".xlsm", ".xlsx", ".xlt", ".xltm", ".xltx", ".xps" };
        private static List<string> videoTypes = new List<string> { ".3g2", ".3gp", ".asf", ".avi", ".bin",
            ".dat", ".f4v", ".flv", ".h264", ".m4v", ".mkv", ".mod", ".mov", ".mp4", ".mpeg", ".mpg", ".mts",
            ".rm", ".rmvb", ".ts", ".vcd", ".vid", ".vob", ".webm", ".wmv" };
        private static List<string> audioTypes = new List<string> { ".ac3", ".aif", ".amr", ".aud", ".flac",
            ".iff", ".m3u", ".m3u8", ".m4a", ".m4b", ".m4p", ".m4r", ".mid", ".midi", ".mod", ".mp3", ".mpa",
            ".ogg", ".ra", ".wav", ".wma" };
        private static List<string> imageTypes = new List<string> { ".bmp", ".djvu", ".dng", ".gif", ".gz",
            ".jpeg", ".jpg", ".mng", ".msp", ".png", ".psd", ".pspimage", ".tga", ".thm", ".tif", ".tiff",
            ".xcf", ".ai", ".cdd", ".cdr", ".eps", ".ps", ".svg", ".vsd" };

        public static Tuple<double, ElementViewModel.Unit> Recount(Tuple<double, ElementViewModel.Unit> value)
        {
            while(value.Item1 >= MaxByteSize)
                value = new Tuple<double, ElementViewModel.Unit>(value.Item1 / KyloBiteSize, value.Item2 + 1);

            return new Tuple<double, ElementViewModel.Unit>(Math.Round(value.Item1, 1), value.Item2);
        }

        public static ElementViewModel.FileType DefineFileType(string extension)
        {
            if (docTypes.Contains(extension))
                return ElementViewModel.FileType.Document;

            if (imageTypes.Contains(extension))
                return ElementViewModel.FileType.Image;

            if (videoTypes.Contains(extension))
                return ElementViewModel.FileType.Video;

            if (audioTypes.Contains(extension))
                return ElementViewModel.FileType.Audio;

            return ElementViewModel.FileType.Other;
        }
    }
}
