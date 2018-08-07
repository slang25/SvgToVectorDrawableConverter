using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using MathNet.Numerics.LinearAlgebra.Double;
using PathFillTypeConverter;
using PathFillTypeConverter.Utils;
using SvgToVectorDrawableConverter.DataFormat.Common;
using SvgToVectorDrawableConverter.DataFormat.Exceptions;
using SvgToVectorDrawableConverter.DataFormat.ScalableVectorGraphics;
using SvgToVectorDrawableConverter.DataFormat.VectorDrawable;
using Group = SvgToVectorDrawableConverter.DataFormat.VectorDrawable.Group;
using VdPath = SvgToVectorDrawableConverter.DataFormat.VectorDrawable.Path;
using SvgPath = SvgToVectorDrawableConverter.DataFormat.ScalableVectorGraphics.Path;
using VdClipPath = SvgToVectorDrawableConverter.DataFormat.VectorDrawable.ClipPath;
using Vector = SvgToVectorDrawableConverter.DataFormat.VectorDrawable.Vector;

namespace SvgToVectorDrawableConverter.DataFormat.Converters.SvgToVector
{
    class SvgToVectorDocumentConverter
    {
        [NotNull]
        private readonly string _blankVectorDrawablePath;
        private readonly bool _fixFillType;

        public SvgToVectorDocumentConverter([NotNull] string blankVectorDrawablePath, bool fixFillType)
        {
            _blankVectorDrawablePath = blankVectorDrawablePath;
            _fixFillType = fixFillType;
        }

        private bool _isFillTypeSupported;
        private bool _isStrokeDasharrayUsed;
        private bool _isGroupOpacityUsed;
        private readonly HashSet<string> _unsupportedElements = new HashSet<string>();

        private void Reset()
        {
            _isFillTypeSupported = true;
            _isStrokeDasharrayUsed = false;
            _isGroupOpacityUsed = false;
            _unsupportedElements.Clear();
        }

        [NotNull]
        public IList<string> Warnings
        {
            get
            {
                var warnings = new List<string>();
                if (!_isFillTypeSupported)
                {
                    warnings.Add("SVG fill-rule and clip-rule are not properly supported on Android. Please, read https://github.com/a-student/SvgToVectorDrawableConverter#not-supported-svg-features. Try specifying the --fix-fill-type option.");
                }
                if (_isStrokeDasharrayUsed)
                {
                    warnings.Add("The stroke-dasharray attribute is not supported.");
                }
                if (_isGroupOpacityUsed)
                {
                    warnings.Add("Group opacity is not supported on Android. Please, apply opacity to path elements instead of a group.");
                }
                if (_unsupportedElements.Count > 0)
                {
                    warnings.Add($"Met unsupported element(s): {string.Join(", ", _unsupportedElements)}.");
                }
                return warnings.AsReadOnly();
            }
        }

        private Dictionary<string, Element> _map;

        private static void FillMap(IDictionary<string, Element> map, Element root)
        {
            if (!string.IsNullOrEmpty(root.Id))
            {
                map[root.Id] = root;
            }
            if (!(root is ElementWithChildren))
            {
                return;
            }
            foreach (var child in ((ElementWithChildren)root).Children)
            {
                FillMap(map, child);
            }
        }

        [NotNull]
        public DocumentWrapper<Vector> Convert([NotNull] DocumentWrapper<Svg> svgDocument)
        {
            Reset();

            _map = new Dictionary<string, Element>();
            FillMap(_map, svgDocument.Root);

            var vectorDocument = VectorDocumentWrapper.CreateFromFile(_blankVectorDrawablePath);

            var viewBox = svgDocument.Root.ViewBox;

            vectorDocument.Root.ViewportWidth = viewBox.Width;
            vectorDocument.Root.ViewportHeight = viewBox.Height;

            vectorDocument.Root.Width = ConvertToDp(svgDocument.Root.Width, viewBox.Width);
            vectorDocument.Root.Height = ConvertToDp(svgDocument.Root.Height, viewBox.Height);

            var style = StyleHelper.MergeStyles(StyleHelper.InitialStyles, svgDocument.Root.Style);

            vectorDocument.Root.Alpha = float.Parse(style["opacity"] ?? "1", CultureInfo.InvariantCulture);

            var group = vectorDocument.Root.Children.Append<Group>();
            group.TranslateX = -viewBox.X;
            group.TranslateY = -viewBox.Y;
            AppendAll(group.Children, svgDocument.Root.Children, style);

            VectorOptimizer.Optimize(vectorDocument.Root);
            return vectorDocument;
        }

        private static string ConvertToDp(string length, double reference)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}dp", UnitConverter.ConvertToPx(length, reference));
        }

        private void InitRecursively(Group outerGroup, Group innerGroup, G g, StringDictionary parentStyle)
        {
            var style = StyleHelper.MergeStyles(parentStyle, g.Style);
            Init(outerGroup, innerGroup, g.Transform, style);
            AppendAll(innerGroup.Children, g.Children, style);

            _isGroupOpacityUsed |= style["opacity"] != null;
        }

        private void AppendAll(ElementCollection elements, ElementCollection children, StringDictionary parentStyle)
        {
            foreach (var child in children)
            {
                if (!IsDisplayed(child))
                {
                    continue;
                }
                if (child is G)
                {
                    var outerGroup = elements.Append<Group>();
                    var innerGroup = outerGroup.Children.Append<Group>();
                    InitRecursively(outerGroup, innerGroup, (G)child, parentStyle);
                    continue;
                }
                if (child is SvgPath)
                {
                    var outerGroup = elements.Append<Group>();
                    var innerGroup = outerGroup.Children.Append<Group>();
                    Init(outerGroup, innerGroup, (SvgPath)child, parentStyle);
                    continue;
                }
                if (child is UnsupportedElement)
                {
                    _unsupportedElements.Add(child.ToString());
                }
            }
        }

        private static bool IsDisplayed(Element element)
        {
            return Styler.GetStyle(element)["display"] != "none";
        }

        private void Init(Group outerGroup, Group innerGroup, SvgPath svgPath, StringDictionary parentStyle)
        {
            var style = StyleHelper.MergeStyles(parentStyle, svgPath.Style);
            Init(outerGroup, innerGroup, svgPath.Transform, style);
            var fillPath = innerGroup.Children.Append<VdPath>();
            var strokePath = fillPath;

            fillPath.PathData = svgPath.D;
            if (style.ContainsKey("fill") && SetFillType(fillPath, style["fill-rule"]))
            {
                strokePath = innerGroup.Children.Append<VdPath>();
                strokePath.PathData = PathDataFixer.Fix(svgPath.D);
            }
            fillPath.PathData = PathDataFixer.Fix(fillPath.PathData);

            foreach (string key in style.Keys)
            {
                var value = style[key];
                switch (key)
                {
                    case "fill":
                        if (value.StartsWith("#"))
                        {
                            fillPath.FillColor = value;
                        }
                        break;
                    case "stroke":
                        if (value.StartsWith("#"))
                        {
                            strokePath.StrokeColor = value;
                        }
                        break;
                    case "stroke-width":
                        strokePath.StrokeWidth = (float)UnitConverter.ConvertToPx(value, 0);
                        break;
                    case "stroke-opacity":
                        strokePath.StrokeAlpha *= float.Parse(value, CultureInfo.InvariantCulture);
                        break;
                    case "fill-opacity":
                        fillPath.FillAlpha *= float.Parse(value, CultureInfo.InvariantCulture);
                        break;
                    case "opacity":
                        strokePath.StrokeAlpha *= float.Parse(value, CultureInfo.InvariantCulture);
                        fillPath.FillAlpha *= float.Parse(value, CultureInfo.InvariantCulture);
                        break;
                    case "stroke-linecap":
                        strokePath.StrokeLineCap = value;
                        break;
                    case "stroke-linejoin":
                        strokePath.StrokeLineJoin = value;
                        break;
                    case "stroke-miterlimit":
                        strokePath.StrokeMiterLimit = value;
                        break;
                    case "stroke-dasharray":
                        _isStrokeDasharrayUsed |= value != "none";
                        break;
                }
            }
        }

        private void Init(Group outerGroup, Group innerGroup, Transform transform, StringDictionary style)
        {
            if (transform is Transform.Matrix matrix)
            {
                if (matrix.A == 0 && matrix.D == 0)
                {
                    innerGroup.Rotation = 90;
                    innerGroup.ScaleX = matrix.B;
                    innerGroup.ScaleY = -matrix.C;
                    innerGroup.TranslateX = matrix.E;
                    innerGroup.TranslateY = matrix.F;
                }
                else if (matrix.A * matrix.C == -matrix.B * matrix.D)
                {
                    innerGroup.Rotation = MathHelper.ToDegrees(Math.Atan(matrix.B / matrix.A));
                    innerGroup.ScaleX = Math.Sign(matrix.A) * Math.Sqrt(MathHelper.Square(matrix.A) + MathHelper.Square(matrix.B));
                    innerGroup.ScaleY = Math.Sign(matrix.D) * Math.Sqrt(MathHelper.Square(matrix.C) + MathHelper.Square(matrix.D));
                    innerGroup.TranslateX = matrix.E;
                    innerGroup.TranslateY = matrix.F;
                }
                else
                {
                    var svd = DenseMatrix.OfArray(new[,] { { matrix.A, matrix.C }, { matrix.B, matrix.D } }).Svd();
                    outerGroup.Rotation = MathHelper.ToDegrees(Math.Atan2(svd.U[1, 0], svd.U[0, 0]));
                    innerGroup.Rotation = MathHelper.ToDegrees(Math.Atan2(svd.VT[1, 0], svd.VT[0, 0]));
                    outerGroup.ScaleX = svd.S[0];
                    outerGroup.ScaleY = svd.S[1] * svd.U.Determinant();
                    innerGroup.ScaleY = svd.VT.Determinant();
                    outerGroup.TranslateX = matrix.E;
                    outerGroup.TranslateY = matrix.F;
                }
            }
            if (transform is Transform.Translate translate)
            {
                innerGroup.TranslateX = translate.Tx;
                innerGroup.TranslateY = translate.Ty;
            }
            if (transform is Transform.Scale scale)
            {
                innerGroup.ScaleX = scale.Sx;
                innerGroup.ScaleY = scale.Sy;
            }
            if (transform is Transform.Rotate rotate)
            {
                innerGroup.Rotation = rotate.Angle;
                innerGroup.PivotX = rotate.Cx;
                innerGroup.PivotY = rotate.Cy;
            }

            var clipPath = style["clip-path"];
            if (!string.IsNullOrEmpty(clipPath) && clipPath != "none")
            {
                var match = Regex.Match(clipPath, @"^url\(#(?<key>.+)\)$");
                if (!match.Success)
                {
                    throw new UnsupportedFormatException("Wrong clip-path attribute value.");
                }
                var key = match.Groups["key"].Value;
                foreach (var x in ClipPathHelper.ExtractPaths((G)_map[key]))
                {
                    var vdClipPath = innerGroup.Children.Append<VdClipPath>();
                    vdClipPath.PathData = x.Path.D;
                    SetFillType(vdClipPath, x.Style["clip-rule"]);
                    vdClipPath.PathData = PathDataFixer.Fix(vdClipPath.PathData);
                }
            }
        }

        private bool SetFillType(PathBase path, string rule)
        {
            var separatePathForStroke = false;
            FillType? fillType = null;
            switch (rule)
            {
                case "nonzero":
                    fillType = FillType.winding;
                    break;
                case "evenodd":
                    fillType = FillType.even_odd;
                    break;
            }
            if (fillType.HasValue)
            {
                if (fillType.Value == FillType.even_odd && _fixFillType)
                {
                    try
                    {
                        path.PathData = PathDataConverter.ConvertFillTypeFromEvenOddToWinding(path.PathData, out separatePathForStroke);
                        fillType = FillType.winding;
                    }
                    catch (Exception e)
                    {
                        throw new FixFillTypeException(e);
                    }
                }

                path.FillType = fillType.Value;
                if (path.FillType != fillType.Value)
                {
                    _isFillTypeSupported = false;
                }
            }
            return separatePathForStroke;
        }
    }
}
