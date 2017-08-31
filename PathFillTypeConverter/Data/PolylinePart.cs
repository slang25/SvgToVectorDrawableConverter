using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace PathFillTypeConverter.Data
{
    [Serializable]
    public class PolylinePart
    {
        public const int MaxLineCount = 50;

        private sealed class PointCollection : IReadOnlyCollection<Point>
        {
            private readonly IReadOnlyList<Point> _points;
            private readonly int _startIndex, _endIndex;

            public PointCollection(IReadOnlyList<Point> points, int startIndex)
            {
                _points = points;
                _startIndex = startIndex;
                _endIndex = Math.Min(_startIndex + MaxLineCount, _points.Count - 1);
            }

            public IEnumerator<Point> GetEnumerator()
            {
                for (var i = _startIndex; i <= _endIndex; i++)
                {
                    yield return _points[i];
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public int Count => _endIndex - _startIndex + 1;
        }

        public Box BoundingBox { get; }

        public PolylinePart([NotNull] IReadOnlyList<Point> points, int startIndex)
        {
            BoundingBox = new Box(new PointCollection(points, startIndex));
        }
    }
}
