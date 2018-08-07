﻿using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace SvgToVectorDrawableConverter.Utils
{
    static class Inkscape
    {
        public static string FindAppPath()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var paths = new[]
                {
                    "/Applications/Inkscape.app/Contents/Resources/bin/inkscape", // OS X
                    "/usr/bin/inkscape", // Linux
                    "/usr/local/bin/inkscape", // Homebrew
                    "/opt/local/bin/inkscape", // MacPorts
                };

                foreach (var path in paths)
                {
                    if (File.Exists(path)) return path;
                }
            }
            else
            {
                // Windows
                return (string)Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\App Paths\inkscape.exe", null, null);
            }
            throw new ApplicationException("Inkscape app was not found. Please download it from https://inkscape.org/en/download and install it on your system.");
        }
        
        public static void ConvertPdfToSvg(string appPath, string inputPath, string outputPath)
        {
            if (!File.Exists(appPath))
            {
                throw new ApplicationException($"Inkscape app was not found in the path '{appPath}'. Please download it from https://inkscape.org/en/download and install it on your system.");
            }

            var arguments = $"-f \"{inputPath}\" -D -l \"{outputPath}\"";
            using (var process = Process.Start(new ProcessStartInfo(appPath, arguments) { WorkingDirectory = Path.GetDirectoryName(appPath), CreateNoWindow = true, RedirectStandardOutput = true }))
            {
                process.WaitForExit();
            }
        }
    }
}
