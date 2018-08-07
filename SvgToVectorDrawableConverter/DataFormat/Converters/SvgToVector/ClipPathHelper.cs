﻿using System.Collections.Generic;
using System.Collections.Specialized;
using SvgToVectorDrawableConverter.DataFormat.Common;
using SvgToVectorDrawableConverter.DataFormat.ScalableVectorGraphics;

namespace SvgToVectorDrawableConverter.DataFormat.Converters.SvgToVector
{
    class PathWithStyle
    {
        public Path Path { get; }
        public StringDictionary Style { get; }

        public PathWithStyle(Path path, StringDictionary style)
        {
            Path = path;
            Style = style;
        }
    }

    static class ClipPathHelper
    {
        public static IEnumerable<PathWithStyle> ExtractPaths(G clipPath)
        {
            return ExtractPaths(clipPath, StyleHelper.InitialStyles);
        }

        private static IEnumerable<PathWithStyle> ExtractPaths(ElementWithChildren element, StringDictionary parentStyle)
        {
            var style = StyleHelper.MergeStyles(parentStyle, Styler.GetStyle(element));

            foreach (var child in element.Children)
            {
                if (child is Path path)
                {
                    yield return new PathWithStyle(path, StyleHelper.MergeStyles(style, path.Style));
                }
                if (child is ElementWithChildren)
                {
                    foreach (var x in ExtractPaths((ElementWithChildren)child, style))
                    {
                        yield return x;
                    }
                }
            }
        }
    }
}
