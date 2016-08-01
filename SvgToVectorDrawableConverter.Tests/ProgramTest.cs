using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SvgToVectorDrawableConverter.Tests
{
    [TestClass]
    public class ProgramTest : ProgramTestBase
    {
        private readonly string _baseDirectory = GetBaseDirectory();

        private static string GetBaseDirectory()
        {
            string result;
            var currentDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());
            while (true)
            {
                result = Path.Combine(currentDirectory.FullName, "test-data");
                if (Directory.Exists(result)) break;
                currentDirectory = currentDirectory.Parent;
                if (currentDirectory == null) throw new Exception("test-data folder not found");
            }
            return result;
        }

        public override string InputDirectory => Path.Combine(_baseDirectory, "input");
        public override string ExpectedDirectory => Path.Combine(_baseDirectory, "expected");
        public override string TempDirectory => Path.Combine(_baseDirectory, "temp");

        [TestMethod, Ignore]
        public void DeleteTemp()
        {
            Directory.Delete(TempDirectory, true);
        }

        [TestMethod]
        public async Task Default()
        {
            await RunTest(null, false);
        }

        [TestMethod]
        public async Task FixFillType()
        {
            await RunTest(null, true);
        }

        [TestMethod]
        public async Task BetterVectorDrawable()
        {
            await RunTest("BetterVectorDrawable", false);
        }

        [TestMethod]
        public async Task BetterVectorDrawable_FixFillType()
        {
            await RunTest("BetterVectorDrawable", true);
        }

        [TestMethod]
        public async Task ResAuto()
        {
            await RunTest("res-auto", false);
        }

        [TestMethod]
        public async Task ResAuto_FixFillType()
        {
            await RunTest("res-auto", true);
        }
    }
}
