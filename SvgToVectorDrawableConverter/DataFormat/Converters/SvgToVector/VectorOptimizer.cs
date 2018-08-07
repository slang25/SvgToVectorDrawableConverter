using System.Linq;
using JetBrains.Annotations;
using SvgToVectorDrawableConverter.DataFormat.Common;
using SvgToVectorDrawableConverter.DataFormat.VectorDrawable;

namespace SvgToVectorDrawableConverter.DataFormat.Converters.SvgToVector
{
    internal static class VectorOptimizer
    {
        public static void Optimize([NotNull] Vector root)
        {
            ResetIneffectiveAttributesRecursively(root.Children);
            RemoveInvisiblePaths(root.Children);
            RemoveEmptyGroups(root.Children);
            EliminateUselessGroupNesting(root.Children);
        }

        private static void ResetIneffectiveAttributesRecursively(ElementCollection elements)
        {
            foreach (var element in elements)
            {
                switch (element)
                {
                    case ElementWithChildren elementWithChild:
                        ResetIneffectiveAttributesRecursively(elementWithChild.Children);
                        break;
                    case Path leafElement:
                        ResetIneffectiveAttributes(leafElement);
                        break;
                }
            }
        }

        private static void ResetIneffectiveAttributes(Path path)
        {
            if (string.IsNullOrEmpty(path.FillColor) || path.FillAlpha == 0)
            {
                path.FillColor = null;
                path.FillAlpha = 1;
                path.FillType = FillType.winding;
            }
            if (string.IsNullOrEmpty(path.StrokeColor) || path.StrokeAlpha == 0 || path.StrokeWidth == 0)
            {
                path.StrokeColor = null;
                path.StrokeAlpha = 1;
                path.StrokeWidth = 0;
            }
        }

        private static void RemoveInvisiblePaths(ElementCollection elements)
        {
            for (var i = 0; i < elements.Count; i++)
            {
                var element = elements[i];
                if (element is Path)
                {
                    if (!IsPathVisible((Path)element))
                    {
                        elements.RemoveAt(i--);
                    }
                    continue;
                }
                if (element is ClipPath)
                {
                    if (string.IsNullOrEmpty(((ClipPath)element).PathData))
                    {
                        elements.RemoveAt(i--);
                    }
                    continue;
                }
                if (element is ElementWithChildren)
                {
                    RemoveInvisiblePaths(((ElementWithChildren)element).Children);
                }
            }
        }

        private static bool IsPathVisible(Path path)
        {
            if (string.IsNullOrEmpty(path.PathData))
            {
                return false;
            }
            if (!string.IsNullOrEmpty(path.FillColor) && path.FillAlpha > 0)
            {
                return true;
            }
            if (!string.IsNullOrEmpty(path.StrokeColor) && path.StrokeAlpha > 0 && path.StrokeWidth > 0)
            {
                return true;
            }
            return false;
        }

        private static void RemoveEmptyGroups(ElementCollection elements)
        {
            for (var i = 0; i < elements.Count; i++)
            {
                if (!(elements[i] is ElementWithChildren element)) continue;
                RemoveEmptyGroups(element.Children);
                if (element.Children.All(x => x is ClipPath)) elements.RemoveAt(i--);
            }
        }

        private static void EliminateUselessGroupNesting(ElementCollection elements)
        {
            for (var i = 0; i < elements.Count; i++)
            {
                if (!(elements[i] is Group group)) continue;
                EliminateUselessGroupNesting(group.Children);
                if (IsUselessGroup(group))
                {
                    elements.RemoveAt(i);
                    var count = group.Children.Count;
                    while (group.Children.Count > 0)
                    {
                        elements.MoveTo(i, group.Children[group.Children.Count - 1]);
                    }
                    i += count - 1;
                }
            }
        }

        private static bool IsUselessGroup(Group group)
        {
            if (group.Rotation != 0)
            {
                return false;
            }
            if (group.PivotX != 0 || group.PivotY != 0)
            {
                return false;
            }
            if (group.ScaleX != 1 || group.ScaleY != 1)
            {
                return false;
            }
            if (group.TranslateX != 0 || group.TranslateY != 0)
            {
                return false;
            }
            if (group.Children.Any(x => x is ClipPath))
            {
                return false;
            }
            return true;
        }
    }
}
