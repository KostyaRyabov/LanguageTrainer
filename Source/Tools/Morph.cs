using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;


/* 
 * ################################################################################################
 * 
 * Source  : https://gist.github.com/badamczewski/06d9c86e6d78fc79905f943cb3545f51
 * 
 *              Some parts of the code have been removed or rewritten
 *           
 * ################################################################################################
 */


namespace LanguageTrainer.Source.Tools
{
    public static class Morph
    {
        public static bool Collapse(PathGeometry sourceGeometry, double progress)
        {
            int count = sourceGeometry.Figures.Count;
            for (int i = 0; i < sourceGeometry.Figures.Count; i++)
            {
                count -= MorphCollapse(sourceGeometry.Figures[i], progress);
            }

            if (count <= 0) return true;

            return false;
        }

        private static bool DoFiguresOverlap(PathFigureCollection figures, int index0, int index1, int index2)
        {
            if (index2 < figures.Count && index0 >= 0)
            {
                PathGeometry g0 = new PathGeometry(new[] { figures[index2] });
                PathGeometry g1 = new PathGeometry(new[] { figures[index1] });
                PathGeometry g2 = new PathGeometry(new[] { figures[index0] });
                var result0 = g0.FillContainsWithDetail(g1);
                var result1 = g0.FillContainsWithDetail(g2);

                return
                    (result0 == IntersectionDetail.FullyContains ||
                        result0 == IntersectionDetail.FullyInside) &&
                    (result1 == IntersectionDetail.FullyContains ||
                        result1 == IntersectionDetail.FullyInside);
            }

            return false;
        }

        private static bool DoFiguresOverlap(PathFigureCollection figures, int index0, int index1)
        {
            if (index1 < figures.Count && index0 >= 0)
            {
                PathGeometry g1 = new PathGeometry(new[] { figures[index1] });
                PathGeometry g2 = new PathGeometry(new[] { figures[index0] });
                var result = g1.FillContainsWithDetail(g2);
                return result == IntersectionDetail.FullyContains || result == IntersectionDetail.FullyInside;
            }
            return false;
        }

        private static void CollapseFigure(PathFigure figure)
        {
            PointCollection points = ((PolyLineSegment)figure.Segments[0]).Points;
            var centroid = GetCentroid(points, points.Count);

            for (int p = 0; p < points.Count; p++)
            {
                points[p] = centroid;
            }

            figure.StartPoint = centroid;
        }

        public static List<PathGeometry> ToCache(PathGeometry source, PathGeometry target, double speed) => ToCache(source, target, speed, new PowerEase());

        public static List<PathGeometry> ToCache(PathGeometry source, PathGeometry target, double speed, EasingFunctionBase easeFunction)
        {
            int steps = (int)(1 / speed);
            double p = speed;
            List<PathGeometry> cache = new List<PathGeometry>(steps + 1);

            for (int i = 0; i < steps; i++)
            {
                var clone = source.Clone();
                var easeP = easeFunction.Ease(p);

                To(clone, target, easeP);

                p += speed;

                cache.Add(clone);
            }

            cache.Add(target.Clone());

            return cache;
        }

        public static void To(PathGeometry source, PathGeometry target, double progress)
        {
            if (source.Figures.Count < target.Figures.Count)
            {
                var toAdd = target.Figures.Count - source.Figures.Count;

                if (source.Figures.Count == 0)
                {
                    source.Figures = target.Figures.Clone();
                    return;
                }
                else
                {
                    var last = source.Figures.Last();

                    for (int i = 0; i < toAdd; i++)
                    {
                        source.Figures.Add(last.Clone());
                    }
                }
            }
            else if (source.Figures.Count > target.Figures.Count)
            {
                var toAdd = source.Figures.Count - target.Figures.Count;
                var lastIndex = target.Figures.Count - 1;

                if (target.Figures.Count == 0)
                {
                    source.Figures.Clear();
                    return;
                }

                for (int i = 0; i < toAdd; i++)
                {
                    var clone = target.Figures[lastIndex].Clone();

                    if (lastIndex > 0)
                    {
                        if (DoFiguresOverlap(target.Figures, lastIndex - 1, lastIndex))
                        {
                            if (lastIndex - 2 > 0)
                            {
                                if (DoFiguresOverlap(target.Figures, lastIndex - 2, lastIndex - 1, lastIndex))
                                {
                                    clone = target.Figures[lastIndex - 3].Clone();
                                }
                                else
                                {
                                    clone = target.Figures[lastIndex - 2].Clone();
                                }
                            }
                            else
                            {
                                CollapseFigure(clone);
                            }
                        }
                    }
                    else
                    {
                        CollapseFigure(clone);
                    }

                    target.Figures.Add(clone);
                }
            }

            int[] map = new int[source.Figures.Count];
            for (int i = 0; i < map.Length; i++)
                map[i] = -1;

            for (int i = 0; i < source.Figures.Count; i++)
            {
                double closest = double.MaxValue;
                int closestIndex = -1;

                for (int j = 0; j < target.Figures.Count; j++)
                {
                    if (map.Contains(j))
                        continue;

                    var len = Point.Subtract(source.Figures[i].StartPoint, target.Figures[j].StartPoint).LengthSquared;
                    if (len < closest)
                    {
                        closest = len;
                        closestIndex = j;
                    }
                }

                map[i] = closestIndex;
            }

            for (int i = 0; i < source.Figures.Count; i++)
            {
                MorphFigure(source.Figures[i], target.Figures[map[i]], progress);
            }
        }

        public static void MorphFigure(PathFigure source, PathFigure target, double progress)
        {
            PolyLineSegment sourceSegment = (PolyLineSegment)source.Segments[0];
            PolyLineSegment targetSegment = (PolyLineSegment)target.Segments[0];

            if (sourceSegment.Points.Count < targetSegment.Points.Count)
            {
                //
                // Add points to segment.
                //
                var toAdd = targetSegment.Points.Count - sourceSegment.Points.Count;
                for (int i = 0; i < toAdd; i++)
                {
                    sourceSegment.Points.Add(source.StartPoint);
                }
            }
            else if (sourceSegment.Points.Count > targetSegment.Points.Count)
            {
                //
                // Add points to segment.
                //
                var toAdd = sourceSegment.Points.Count - targetSegment.Points.Count;
                for (int i = 0; i < toAdd; i++)
                {
                    targetSegment.Points.Add(target.StartPoint);
                }
            }

            //
            // Interpolate from source to target.
            //
            if (progress >= 1)
            {
                for (int i = 0; i < sourceSegment.Points.Count; i++)
                {
                    var toX = targetSegment.Points[i].X;
                    var toY = targetSegment.Points[i].Y;
                    sourceSegment.Points[i] = new Point(toX, toY);
                }
                source.StartPoint = new Point(target.StartPoint.X, target.StartPoint.Y);
            }
            else
            {
                for (int i = 0; i < sourceSegment.Points.Count; i++)
                {
                    var fromX = sourceSegment.Points[i].X;
                    var toX = targetSegment.Points[i].X;

                    var fromY = sourceSegment.Points[i].Y;
                    var toY = targetSegment.Points[i].Y;

                    if (fromX != toX || fromY != toY)
                    {
                        var x = Interpolate(fromX, toX, progress);
                        var y = Interpolate(fromY, toY, progress);
                        sourceSegment.Points[i] = new Point(x, y);
                    }
                }

                if (source.StartPoint.X != target.StartPoint.X ||
                    source.StartPoint.Y != target.StartPoint.Y)
                {
                    var newX = Interpolate(source.StartPoint.X, target.StartPoint.X, progress);
                    var newY = Interpolate(source.StartPoint.Y, target.StartPoint.Y, progress);
                    source.StartPoint = new Point(newX, newY);
                }
            }
        }

        public static int MorphCollapse(PathFigure source, double progress)
        {
            PolyLineSegment sourceSegment = (PolyLineSegment)source.Segments[0];

            //
            // Find Centroid
            //
            var centroid = GetCentroid(sourceSegment.Points, sourceSegment.Points.Count);
            for (int i = 0; i < sourceSegment.Points.Count; i++)
            {
                var fromX = sourceSegment.Points[i].X;
                var toX = centroid.X;

                var fromY = sourceSegment.Points[i].Y;
                var toY = centroid.Y;

                var x = Interpolate(fromX, toX, progress);
                var y = Interpolate(fromY, toY, progress);

                sourceSegment.Points[i] = new Point(x, y);
            }

            var newX = Interpolate(source.StartPoint.X, centroid.X, progress);
            var newY = Interpolate(source.StartPoint.Y, centroid.Y, progress);

            source.StartPoint = new Point(newX, newY);

            if (centroid.X - newX < 0.005)
            {
                return 1;
            }

            return 0;
        }

        public static Point GetCentroid(PointCollection nodes, int count)
        {
            double x = 0, y = 0, area = 0, k;
            Point a, b;

            if (count > 0)
            {
                b = nodes[count - 1];
            }

            for (int i = 0; i < count; i++)
            {
                a = nodes[i];

                k = a.Y * b.X - a.X * b.Y;
                area += k;
                x += (a.X + b.X) * k;
                y += (a.Y + b.Y) * k;

                b = a;
            }
            area *= 3;

            return (area == 0) ? new Point() : new Point(x /= area, y /= area);
        }

        public static double Interpolate(double from, double to, double progress)
        {
            return from + (to - from) * progress;
        }
    }
}
