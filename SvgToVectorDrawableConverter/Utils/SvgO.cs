using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace SvgToVectorDrawableConverter.Utils
{
    static class SvgO
    {
        public static string FindAppPath()
        {
            var paths = new[] // This needs a tidy up
                {
                    "node.exe",
                    "node",
                };

            foreach (var path in paths)
            {
                if (ExistsOnPath(path)) return path;
            }
            throw new ApplicationException("node was not found, please ensure it is installed and available on the path.");
        }

        private static bool ExistsOnPath(string fileName)
        {
            return GetFullPath(fileName) != null;
        }

        private static string GetFullPath(string fileName)
        {
            if (File.Exists(fileName))
                return Path.GetFullPath(fileName);

            var values = Environment.GetEnvironmentVariable("PATH");
            var splitChar = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ';' : ':';
            foreach (var path in values.Split(splitChar))
            {
                var fullPath = Path.Combine(path, fileName);
                if (File.Exists(fullPath))
                    return fullPath;
            }
            return null;
        }

        public static void OptimizeSvg(string appPath, string inputPath, string outputPath)
        {
            var workingDir = new FileInfo(GetFullPath(appPath)).Directory.FullName;
            const string svgoModulePath = "./node_modules/svgo/bin/svgo";
            var svgoFullPath = Path.Combine(workingDir, svgoModulePath);
            if (!File.Exists(svgoFullPath))
                throw new ApplicationException("svgo was not found, please ensure you have installed it globally with `npm install -g svgo`.");

            var arguments = $"{svgoModulePath} -i \"{inputPath}\" -o \"{outputPath}\"";
            using (var process = Process.Start(new ProcessStartInfo(appPath, arguments) { WorkingDirectory = workingDir, CreateNoWindow = true, RedirectStandardOutput = true }))
            {
                process.WaitForExit();
            }
        }
    }
}
