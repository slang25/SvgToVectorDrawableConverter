using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using JetBrains.Annotations;
using SvgToVectorDrawableConverter.DataFormat.Converters.SvgToVector;
using SvgToVectorDrawableConverter.DataFormat.Exceptions;
using SvgToVectorDrawableConverter.DataFormat.ScalableVectorGraphics;
using SvgToVectorDrawableConverter.Utils;
using Path = System.IO.Path;

namespace SvgToVectorDrawableConverter
{
    public class Program
    {
        public static void Main(string[] args)
        {
            new Program(args, Console.Out, Console.Error).RunAsync().Wait();
        }

        private readonly string[] _args;
        private readonly TextWriter _outWriter;
        private readonly TextWriter _errorWriter;

        public Program([NotNull] string[] args, [NotNull] TextWriter outWriter, [NotNull] TextWriter errorWriter)
        {
            _args = new string[args.Length];
            args.CopyTo(_args, 0);
            _outWriter = outWriter;
            _errorWriter = errorWriter;
        }

        public async Task RunAsync()
        {
            try
            {
                var options = new Options();
                if (CommandLine.Parser.Default.ParseArguments(_args, options))
                {
                    Convert(options);
                    if (!options.NoUpdateCheck)
                    {
                        using (var updateChecker = new UpdateChecker())
                        {
                            await updateChecker.CheckForUpdateAsync();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                PrintError(e.ToString());
            }
        }

        private void Convert(Options options)
        {
            var converter = new SvgToVectorDocumentConverter(options.BlankVectorDrawablePath, options.FixFillType);

            foreach (var inputFile in Directory.GetFiles(options.InputDirectory, options.InputMask + ".svg", SearchOption.AllDirectories))
            {
                _outWriter.Write(".");

                var subpath = PathHelper.Subpath(inputFile, options.InputDirectory);
                var tempFile = PathHelper.GenerateTempFileName("svg");

                try
                {
                    SvgPreprocessor.Preprocess(inputFile, tempFile);
                    Inkscape.SimplifySvgSync(options.InkscapeAppPath, tempFile, tempFile);

                    var svgDocument = SvgDocumentWrapper.CreateFromFile(tempFile);
                    var outputDocument = converter.Convert(svgDocument).WrappedDocument;
                    PrintWarnings(subpath, converter.Warnings);

                    var outputFile = Path.Combine(options.OutputDirectory, subpath);
                    outputFile = Path.ChangeExtension(outputFile, "xml");
                    outputFile = PathHelper.NormalizeFileName(outputFile);

                    Directory.CreateDirectory(Path.GetDirectoryName(outputFile));
                    var settings = new XmlWriterSettings
                    {
                        Encoding = new UTF8Encoding(false),
                        Indent = true,
                        IndentChars = new string(' ', 4),
                        NewLineOnAttributes = true
                    };
                    using (var writer = XmlWriter.Create(outputFile, settings))
                    {
                        outputDocument.Save(writer);
                    }
                }
                catch (FixFillTypeException e)
                {
                    PrintError($"{subpath}: Failure due to the --fix-fill-type option. {e.InnerException.Message}");
                }
                catch (Exception e)
                {
                    PrintError($"{subpath}: {e.Message}");
                }

                File.Delete(tempFile);
            }
        }

        private void PrintWarnings(string subpath, ICollection<string> warnings)
        {
            if (warnings.Count == 0) return;
            _outWriter.WriteLine();
            _outWriter.Write($"[Warning(s)] {subpath}: ");
            foreach (var warning in warnings)
            {
                _outWriter.WriteLine(warning);
            }
        }

        private void PrintError(string message)
        {
            _errorWriter.WriteLine();
            _errorWriter.WriteLine("[Error] " + message);
        }
    }
}
