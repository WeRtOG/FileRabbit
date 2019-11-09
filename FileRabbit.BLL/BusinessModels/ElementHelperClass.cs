using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FileRabbit.ViewModels;

namespace FileRabbit.BLL.BusinessModels
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

        // this method is needed to translate bytes to kylo-, mega-, gigabytes
        public static Tuple<double, ElementVM.Unit> Recount(Tuple<double, ElementVM.Unit> value)
        {
            // if current value is more than 1000, translate to a large unit
            while(value.Item1 >= MaxByteSize)
                value = new Tuple<double, ElementVM.Unit>(value.Item1 / KyloBiteSize, value.Item2 + 1);

            return new Tuple<double, ElementVM.Unit>(Math.Round(value.Item1, 1), value.Item2);
        }

        // this method is needed to define file extension
        public static string DefineFileExtension(string name)
        {
            string result;
            result = name.Substring(name.LastIndexOf('.'));
            return result;
        }

        // this method is needed to define file name
        public static string DefineFileName(string path)
        {
            string result;
            result = path.Substring(path.LastIndexOf("\\") + 1);
            return result;
        }

        // this method is needed to define file type by its extension
        public static ElementVM.FileType DefineFileType(string extension)
        {
            extension = extension.ToLower();

            if (docTypes.Contains(extension))
                return ElementVM.FileType.Document;

            if (imageTypes.Contains(extension))
                return ElementVM.FileType.Image;

            if (videoTypes.Contains(extension))
                return ElementVM.FileType.Video;

            if (audioTypes.Contains(extension))
                return ElementVM.FileType.Audio;

            return ElementVM.FileType.Other;
        }
    }
}
