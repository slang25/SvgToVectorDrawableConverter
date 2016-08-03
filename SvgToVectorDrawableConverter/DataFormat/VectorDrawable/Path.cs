using System;
using System.Xml;
using JetBrains.Annotations;

namespace SvgToVectorDrawableConverter.DataFormat.VectorDrawable
{
    class Path : PathBase
    {
        public Path([NotNull] XmlElement wrappedElement)
            : base(wrappedElement)
        { }

        public override FillType FillType
        {
            get
            {
                switch (GetAttribute(string.Empty))
                {
                    case "even_odd":
                    case "evenOdd":
                        return FillType.even_odd;
                    default:
                        return default(FillType);
                }
            }
            set
            {
                SetAttribute(value, "better");
                switch (value)
                {
                    case FillType.winding:
                        SetAttribute("nonZero", "api24", "nonZero");
                        break;
                    case FillType.even_odd:
                        SetAttribute("evenOdd", "api24", "nonZero");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(value));
                }
            }
        }

        public string FillColor
        {
            get { return GetAttribute<string>(); }
            set
            {
                SetAttribute(value, "android");
                SetAttribute(value, "auto");
                SetAttribute(value, "better");
            }
        }

        public string StrokeColor
        {
            get { return GetAttribute<string>(); }
            set
            {
                SetAttribute(value, "android");
                SetAttribute(value, "auto");
                SetAttribute(value, "better");
            }
        }

        public float StrokeWidth
        {
            get { return GetAttribute<float>(); }
            set
            {
                SetAttribute(value, "android");
                SetAttribute(value, "auto");
                SetAttribute(value, "better");
            }
        }

        public float StrokeAlpha
        {
            get { return GetAttribute(1f); }
            set
            {
                SetAttribute(value, "android", 1);
                SetAttribute(value, "auto", 1);
                SetAttribute(value, "better", 1);
            }
        }

        public float FillAlpha
        {
            get { return GetAttribute(1f); }
            set
            {
                SetAttribute(value, "android", 1);
                SetAttribute(value, "auto", 1);
                SetAttribute(value, "better", 1);
            }
        }

        public double TrimPathStart
        {
            set
            {
                SetAttribute(Convert.ToSingle(value), "android");
                SetAttribute(Convert.ToSingle(value), "auto");
                SetAttribute(Convert.ToSingle(value), "better");
            }
        }

        public double TrimPathEnd
        {
            set
            {
                SetAttribute(Convert.ToSingle(value), "android");
                SetAttribute(Convert.ToSingle(value), "auto");
                SetAttribute(Convert.ToSingle(value), "better");
            }
        }

        public double TrimPathOffset
        {
            set
            {
                SetAttribute(Convert.ToSingle(value), "android");
                SetAttribute(Convert.ToSingle(value), "auto");
                SetAttribute(Convert.ToSingle(value), "better");
            }
        }

        /// <summary>
        /// Sets the linecap for a stroked path: butt, round, square.
        /// </summary>
        public string StrokeLineCap
        {
            set
            {
                SetAttribute(value, "android");
                SetAttribute(value, "auto");
                SetAttribute(value, "better");
            }
        }

        /// <summary>
        /// Sets the lineJoin for a stroked path: miter, round, bevel.
        /// </summary>
        public string StrokeLineJoin
        {
            set
            {
                SetAttribute(value, "android");
                SetAttribute(value, "auto");
                SetAttribute(value, "better");
            }
        }

        public string StrokeMiterLimit
        {
            set
            {
                SetAttribute(value, "android");
                SetAttribute(value, "auto");
                SetAttribute(value, "better");
            }
        }
    }
}
