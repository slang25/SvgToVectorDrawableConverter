using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using CommandLine;
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
            var parseResults = CommandLine.Parser.Default.ParseArguments<Options>(args);
            parseResults.WithParsed(Convert).WithNotParsed(errs => errs.ToList().ForEach(e => PrintError(e.ToString())));
        }

        private static void Convert(Options options)
        {
            var converter = new SvgToVectorDocumentConverter(options.BlankVectorDrawablePath, options.FixFillType);

            foreach (var inputFile in Directory.GetFiles(options.InputDirectory, options.InputMask + ".pdf", SearchOption.AllDirectories))
            {
                Console.Write(".");

                var subpath = PathHelper.Subpath(inputFile, options.InputDirectory);
                var tempFile = PathHelper.GenerateTempFileName("svg");

                try
                {
                    Inkscape.ConvertPdfToSvg(Inkscape.FindAppPath(), inputFile, tempFile);
                    
                    SvgO.OptimizeSvg(options.SvgOPath, tempFile, tempFile);
                    SvgPreprocessor.Preprocess(tempFile, tempFile);

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

        private static void PrintWarnings(string subpath, ICollection<string> warnings)
        {
            if (warnings.Count == 0) return;
            Console.WriteLine();
            Console.Write($"[Warning(s)] {subpath}: ");
            foreach (var warning in warnings)
            {
                Console.WriteLine(warning);
            }
        }

        private static void PrintError(string message)
        {
            Console.WriteLine();
            Console.WriteLine("[Error] " + message);
        }
    }
}
