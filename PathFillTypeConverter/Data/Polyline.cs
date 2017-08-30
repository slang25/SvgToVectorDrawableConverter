using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using PathFillTypeConverter.Extensions;

namespace PathFillTypeConverter.Data
{
    [Serializable]
    public class Polyline
    {
        public IReadOnlyList<Point> Points { get; }
        public Box BoundingBox { get; }

        public IReadOnlyList<PolylinePart> Parts { get; }

        public Polyline([NotNull] IEnumerable<Point> points)
        {
            Points = points.ToReadOnlyList();
            BoundingBox = new Box(Points);
            Parts = BuildParts();
        }

        private IReadOnlyList<PolylinePart> BuildParts()
        {
            var lineCount = Points.Count - 1;
            var result = new List<PolylinePart>(lineCount / PolylinePart.MaxLineCount + 1);
            for (var i = 0; i < lineCount; i += PolylinePart.MaxLineCount)
            {
                result.Add(new PolylinePart(Points, i));
            }
            return result.AsReadOnly();
        }
    }
}
