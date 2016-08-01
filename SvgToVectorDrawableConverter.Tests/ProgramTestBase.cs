using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SvgToVectorDrawableConverter.Utils;

namespace SvgToVectorDrawableConverter.Tests
{
    public abstract class ProgramTestBase
    {
        public abstract string InputDirectory { get; }
        public abstract string ExpectedDirectory { get; }
        public abstract string TempDirectory { get; }

        protected async Task RunTest(string lib, bool fixFillType, [CallerMemberName] string testMethodName = null)
        {
            var inputFileMask = Path.Combine(InputDirectory, "*");
            var expectedDirectory = Path.Combine(ExpectedDirectory, testMethodName);
            var outputDirectory = !Directory.Exists(expectedDirectory) ? expectedDirectory : Path.Combine(TempDirectory, testMethodName);

            var args = new List<string>
            {
                "-i", inputFileMask,
                "-o", outputDirectory,
                "--no-update-check"
            };
            if (lib != null)
            {
                args.Add("--lib");
                args.Add(lib);
            }
            if (fixFillType)
            {
                args.Add("--fix-fill-type");
            }

            if (Directory.Exists(outputDirectory))
            {
                Directory.Delete(outputDirectory, true);
            }
            Directory.CreateDirectory(outputDirectory);

            using (var outputFile = File.CreateText(Path.Combine(outputDirectory, "output.txt")))
            {
                await new Program(args.ToArray(), outputFile, outputFile).RunAsync();
            }

            if (outputDirectory != expectedDirectory)
            {
                CompareDirectories(outputDirectory, expectedDirectory);
            }
        }

        private static void CompareDirectories(string outputDirectory, string expectedDirectory)
        {
            var outputSubfiles = GetAllSubfiles(outputDirectory);
            CollectionAssert.AreEquivalent(GetAllSubfiles(expectedDirectory), outputSubfiles);
            foreach (var outputSubfile in outputSubfiles)
            {
                var actual = File.ReadAllLines(Path.Combine(outputDirectory, outputSubfile));
                var expected = File.ReadAllLines(Path.Combine(expectedDirectory, outputSubfile));
                CollectionAssert.AreEqual(expected, actual);
            }
        }

        private static string[] GetAllSubfiles(string path)
        {
            return Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories)
                .Select(x => PathHelper.Subpath(x, path).ToLower())
                .ToArray();
        }
    }
}
