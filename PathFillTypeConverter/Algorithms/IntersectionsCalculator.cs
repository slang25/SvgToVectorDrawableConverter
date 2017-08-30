using System.Linq;
using PathFillTypeConverter.Data;

namespace PathFillTypeConverter.Algorithms
{
    static class IntersectionsCalculator
    {
        public static void Calculate(Path path)
        {
            foreach (var subpath in path.Subpaths)
            {
                subpath.BuildPolylineApproximations();
                foreach (var segment in subpath.ClosedSegments)
                {
                    segment.Intersections.Clear();
                }
            }

            for (var i = 0; i < path.Subpaths.Count; i++)
            {
                var subpath1 = path.Subpaths[i];
                var segments1 = subpath1.ClosedSegments.ToArray();
                for (var j = 0; j < segments1.Length; j++)
                {
                    var segment1 = segments1[j];
                    CalculateSelfIntersections(segment1);
                    for (var k = j + 1; k < segments1.Length; k++)
                    {
                        CalculateIntersections(segment1, segments1[k], k == j + 1, j == 0 && k == segments1.Length - 1);
                    }
                    foreach (var subpath2 in path.Subpaths.Skip(i + 1))
                    {
                        if (!segment1.PolylineApproximation.BoundingBox.IntersectsWith(subpath2.PolygonApproximation.BoundingBox))
                        {
                            continue;
                        }
                        foreach (var segment2 in subpath2.ClosedSegments)
                        {
                            CalculateIntersections(segment1, segment2, false, false);
                        }
                    }
                }
            }
        }

        private struct LineIterator
        {
            private readonly Polyline _polyline;
            private int _index;

            private LineIterator(Polyline polyline, int index)
            {
                _polyline = polyline;
                _index = index;
            }

            public LineIterator(Polyline polyline)
                : this(polyline, 1)
            { }

            public LineIterator Clone()
            {
                return new LineIterator(_polyline, _index);
            }

            public bool MoveNextLine()
            {
                _index++;
                return _index < _polyline.Points.Count;
            }

            private int NextPartIndex
            {
                get
                {
                    var result = _index / PolylinePart.MaxLineCount;
                    if (_index % PolylinePart.MaxLineCount > 0)
                    {
                        result++;
                    }
                    return result;
                }
            }

            public void SkipCurrentPart()
            {
                _index = NextPartIndex * PolylinePart.MaxLineCount;
            }

            public Point CurrentStartPoint => _polyline.Points[_index - 1];
            public Point CurrentEndPoint => _polyline.Points[_index];
            public PolylinePart CurrentPart => _polyline.Parts[NextPartIndex - 1];
            public bool IsFirstLine => _index == 1;
            public bool IsLastLine => _index == _polyline.Points.Count - 1;
        }

        private static void CalculateSelfIntersections(SegmentBase segment)
        {
            if (!(segment is CubicBezierSegment))
            {
                // only cubic bezier segment can self-intersect
                return;
            }
            var iterator1 = new LineIterator(segment.PolylineApproximation);
            while (iterator1.MoveNextLine())
            {
                var iterator2 = iterator1.Clone();
                if (!iterator2.MoveNextLine())
                {
                    return;
                }
                var skipCurrentPart1 = true;
                while (iterator2.MoveNextLine())
                {
                    if (!iterator1.CurrentPart.BoundingBox.IntersectsWith(iterator2.CurrentPart.BoundingBox))
                    {
                        iterator2.SkipCurrentPart();
                        continue;
                    }
                    skipCurrentPart1 = false;
                    var intersection = CalculateIntersection(iterator1.CurrentStartPoint, iterator1.CurrentEndPoint, iterator2.CurrentStartPoint, iterator2.CurrentEndPoint);
                    if (intersection.HasValue)
                    {
                        segment.Intersections.Add(intersection.Value);
                    }
                }
                if (skipCurrentPart1)
                {
                    iterator1.SkipCurrentPart();
                }
            }
        }

        private static void CalculateIntersections(SegmentBase segment1, SegmentBase segment2, bool skipInner, bool skipOuter)
        {
            if (!segment1.PolylineApproximation.BoundingBox.IntersectsWith(segment2.PolylineApproximation.BoundingBox))
            {
                return;
            }
            var iterator1 = new LineIterator(segment1.PolylineApproximation);
            while (iterator1.MoveNextLine())
            {
                var iterator2 = new LineIterator(segment2.PolylineApproximation);
                var skipCurrentPart1 = true;
                while (iterator2.MoveNextLine())
                {
                    if (!iterator1.CurrentPart.BoundingBox.IntersectsWith(iterator2.CurrentPart.BoundingBox))
                    {
                        iterator2.SkipCurrentPart();
                        continue;
                    }
                    skipCurrentPart1 = false;
                    if ((skipInner && iterator1.IsLastLine && iterator2.IsFirstLine) ||
                        (skipOuter && iterator1.IsFirstLine && iterator2.IsLastLine))
                    {
                        continue;
                    }
                    var intersection = CalculateIntersection(iterator1.CurrentStartPoint, iterator1.CurrentEndPoint, iterator2.CurrentStartPoint, iterator2.CurrentEndPoint);
                    if (intersection.HasValue)
                    {
                        segment1.Intersections.Add(intersection.Value);
                        segment2.Intersections.Add(intersection.Value);
                    }
                }
                if (skipCurrentPart1)
                {
                    iterator1.SkipCurrentPart();
                }
            }
        }

        private static Point? CalculateIntersection(Point p11, Point p12, Point p21, Point p22)
        {
            if (!new Box(p11, p12).IntersectsWith(new Box(p21, p22)))
            {
                return null;
            }
            var s1 = p12 - p11;
            var s2 = p22 - p21;
            var a = p11 - p21;
            var b = s1.X * s2.Y - s2.X * s1.Y;
            var s = (s1.X * a.Y - s1.Y * a.X) / b;
            var t = (s2.X * a.Y - s2.Y * a.X) / b;
            if (s >= 0 && s <= 1 && t >= 0 && t <= 1)
            {
                return new Point(p11.X + t * s1.X, p11.Y + t * s1.Y);
            }
            return null;
        }
    }
}
