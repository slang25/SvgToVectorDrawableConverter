using System;
using System.IO;
using CommandLine;
using SvgToVectorDrawableConverter.Utils;

namespace SvgToVectorDrawableConverter
{
    class Options
    {
        [Option('i', Required = true, HelpText = "Input file mask with optional directory path. If directory is not specified current directory is used. Example: '-i *.svg'.")]
        public string InputFileMask { private get; set; }

        public string InputDirectory
        {
            get
            {
                var value = Path.GetDirectoryName(InputFileMask);
                if (string.IsNullOrEmpty(value))
                {
                    value = Environment.CurrentDirectory;
                }
                else
                {
                    value = Path.GetFullPath(value);
                }
                return value;
            }
        }

        public string InputMask
        {
            get
            {
                var value = Path.GetFileNameWithoutExtension(InputFileMask);
                if (string.IsNullOrEmpty(value))
                {
                    value = "*";
                }
                return value;
            }
        }

        private string _outputDirectory;

        [Option('o', HelpText = "Path to the output directory. By default, the output files are put together with the input files.")]
        public string OutputDirectory
        {
            get
            {
                if (string.IsNullOrEmpty(_outputDirectory))
                {
                    return InputDirectory;
                }
                return _outputDirectory;
            }
            set { _outputDirectory = value; }
        }

        [Option("lib", HelpText = "Library name for which to create vector drawable files. Do not specify if you want to convert for Android 5.0+")]
        public string Lib { private get; set; }

        public string BlankVectorDrawablePath => Path.Combine(App.Directory, $"BlankVectorDrawable{(string.IsNullOrEmpty(Lib) ? "" : "." + Lib)}.xml");

        private string _inkscapeAppPath;
        private string _svgoPath;

        [Option("inkscape", HelpText = "Path to the Inkscape app file. Specify this if your Inkscape installation directory differs from the default.")]
        public string InkscapeAppPath
        {
            get
            {
                if (string.IsNullOrEmpty(_inkscapeAppPath))
                {
                    return Inkscape.FindAppPath();
                }
                return _inkscapeAppPath;
            }
            set { _inkscapeAppPath = value; }
        }

        [Option("svgo", HelpText = "Path to the svgo. Specify this if your svgo location differs from the default.")]
        public string SvgOPath
        {
            get
            {
                if (string.IsNullOrEmpty(_svgoPath))
                {
                    return SvgO.FindAppPath();
                }
                return _svgoPath;
            }
            set { _svgoPath = value; }
        }

        [Option("fix-fill-type", HelpText = "Experimental.")]
        public bool FixFillType { get; set; }
        
        private static string GetGithub()
        {
            return "If you have any problems with the converter, please create an issue on GitHub (https://github.com/a-student/SvgToVectorDrawableConverter/issues/new), explain the reproducing steps, and add link to the SVG file (link is optional but highly recommended)."
                + Environment.NewLine;
        }
    }
}
