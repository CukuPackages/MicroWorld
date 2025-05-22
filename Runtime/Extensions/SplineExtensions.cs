using UnityEngine;
using Unity.Mathematics;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.Splines;
using System;
using Freya;

namespace Cuku.MicroWorld
{
    public static class SplineExtensions
    {
        public static void SetTangentMode(this SplineContainer splineContainer, TangentMode tangentMode)
        {
            foreach (var spline in splineContainer.Splines)
            {
                List<BezierKnot> knots = new List<BezierKnot>(spline.Knots);
                spline.Clear();
                foreach (var knot in knots)
                    spline.Add(knot, tangentMode);
            }
        }

        public static void ShiftKnots(ref SplineContainer splineContainer, Vector3 shift)
        {
            var shiftAmmount = (float3)shift;
            foreach (var spline in splineContainer.Splines)
            {
                var knots = spline.Knots.ToArray();
                for (int i = 0; i < knots.Length; i++)
                    knots[i].Position += shiftAmmount;
                for (int i = 0; i < knots.Length; i++)
                    spline.SetKnot(i, knots[i]);
            }
        }

        public static void SetOrder(this SplineContainer splineContainer, bool clockwise)
        {
            if (splineContainer == null)
                throw new ArgumentNullException(nameof(splineContainer));

            foreach (var spline in splineContainer.Splines)
            {
                float signedArea = 0f;
                for (int i = 0; i < spline.Count; i++)
                {
                    var current = spline[i].Position;
                    var next = spline[(i + 1) % spline.Count].Position;

                    signedArea += (next.x - current.x) * (next.z + current.z);
                }

                // If signed vectorPoints is negative, vectorPoints are in clockwise order
                if (signedArea < 0 && clockwise)
                    return;

                spline.Knots = spline.Knots.Reverse();
            }
        }

        public static void SnapSplineToTerrain(ref SplineContainer splineContainer)
        {
            var terrains = Terrain.activeTerrains;
            foreach (var spline in splineContainer.Splines)
            {
                var knots = spline.Knots.ToArray();
                for (int i = 0; i < knots.Length; i++)
                {
                    var pointPosition = knots[i].Position;
                    pointPosition.y = 10000.0f;
                    RaycastHit hit;
                    for (int j = 0; j < terrains.Length; j++)
                    {
                        var ray = new Ray(pointPosition, Vector3.down);
                        if (terrains[j].GetComponent<Collider>().Raycast(ray, out hit, Mathf.Infinity))
                        {
                            pointPosition.y = hit.point.y;
                            break;
                        }
                    }
                    knots[i].Position = pointPosition;
                }
                for (int i = 0; i < knots.Length; i++)
                    spline.SetKnot(i, knots[i]);
            }
        }

        public static void AdaptVolumeToSpline(this SplineContainer splineContainer, Transform target)
        {
            var minMax = splineContainer.MinMax();
            var dimensions = minMax.max - minMax.min;
            target.localScale = new Vector3(dimensions.x, dimensions.y, dimensions.z);
        }


        public static List<float3> Points(this SplineContainer splineContainer)
        {
            var points = new List<float3>();
            foreach (var spline in splineContainer.Splines)
                foreach (var knot in spline.Knots)
                    points.Add(knot.Position);
            return points;
        }

        public static List<Vector2> Points2D(this SplineContainer splineContainer)
        {
            var points = new List<Vector2>();
            foreach (var spline in splineContainer.Splines)
                foreach (var knot in spline.Knots)
                    points.Add(new Vector2(knot.Position.x, knot.Position.z));
            return points;
        }

        public static float3[] ToFloat3(this Vector3[] vectorPoints)
        {
            var floatPoints = new float3[vectorPoints.Length];
            for (int i = 0; i < vectorPoints.Length; i++)
            {
                var vectorPoint = vectorPoints[i];
                floatPoints[i] = new float3(vectorPoint.x, vectorPoint.y, vectorPoint.z);
            }
            return floatPoints;
        }

        public static (float3 min, float3 max) MinMax(this SplineContainer splineContainer)
        {
            var min = new float3(float.MaxValue, float.MaxValue, float.MaxValue);
            var max = new float3(float.MinValue, float.MinValue, float.MinValue);
            var points = splineContainer.Points();
            for (int i = 0; i < points.Count; i++)
            {
                var point = points[i];
                min = math.min(min, point);
                max = math.max(max, point);
            }

            return (min, max);
        }

        public static float3 Center(this SplineContainer splineContainer)
        {
            var minMax = splineContainer.MinMax();
            return (minMax.min + minMax.max) * 0.5f;
        }

        public static void PivotAtCenter(this SplineContainer splineContainer)
            =>  MovePivot(splineContainer, splineContainer.Center());

        public static void PivotAtStart(this SplineContainer container)
            => MovePivot(container, container.transform.TransformPoint(container.Splines[0].Knots.First().Position));

        public static void MovePivot(this SplineContainer container, Vector3 position)
        {
            var shift = position - container.transform.position;
            ShiftKnots(ref container, -shift);
            container.transform.position = position;
            UnityEditor.Undo.RecordObject(container, "Move Spline Pivot");
            UnityEditor.EditorUtility.SetDirty(container);
        }

        public static void Scale(this SplineContainer splineContainer, float factor)
        {
            for (int i = 0; i < splineContainer.Splines.Count; i++)
            {
                var spline = splineContainer[i];
                var knotCount = spline.Count;

                for (int j = 0; j < knotCount; j++)
                {
                    var knot = spline[j];

                    knot.Position *= factor;
                    knot.TangentIn *= factor;
                    knot.TangentOut *= factor;

                    spline[j] = knot;
                }
            }
        }

        public static float Area(this SplineContainer splineContainer)
        {
            var points = splineContainer.Points();
            int count = points.Count;
            float area = 0f;
            for (int i = 0; i < count; i++)
            {
                float3 a = points[i];
                float3 b = points[(i + 1) % count];
                area += (b.x - a.x) * (b.z * a.z);
            }

            return math.abs(area) * 0.5f;
        }

        public static bool Contains(this SplineContainer splineContainer, float3 point)
            => new Polygon(splineContainer.Points2D()).Contains(new Vector2(point.x, point.z));

        public static float LowestPoint(this SplineContainer splineContainer)
        {
            var lowest = float.MaxValue;
            foreach (var knot in splineContainer.Spline)
                if (knot.Position.y < lowest)
                    lowest = knot.Position.y;
            return lowest;
        }

        public static bool Connect(this SplineContainer baseSplineContainer, SplineContainer targetSplineContainer, float3[] skipPoints)
        {
            var baseSpline = baseSplineContainer.Spline;
            var targetSpline = targetSplineContainer.Spline;

            float3 baseFirst = baseSpline[0].Position;
            float3 baseLast = baseSpline[baseSpline.Count - 1].Position;

            float3 targetFirst = targetSpline[0].Position;
            float3 targetLast = targetSpline[targetSpline.Count - 1].Position;

            bool Skip(float3 point)
            {
                for (var i = 0; i < skipPoints.Length; i++)
                    if (math.distance(skipPoints[i], point) < math.EPSILON)
                        return true;
                return false;
            }

            // Case 1: base last connects to target first
            if (!Skip(baseLast) && !Skip(targetFirst) &&
                math.distance(baseLast, targetFirst) < math.EPSILON)
            {
                float3 connectionPoint = (baseLast + targetFirst) / 2;
                baseSpline[baseSpline.Count - 1] = new BezierKnot(connectionPoint);
                foreach (var knot in targetSpline.Skip(1))
                    baseSpline.Add(knot);
                return true;
            }

            // Case 2: base first connects to target last
            if (!Skip(baseFirst) && !Skip(targetLast) &&
                math.distance(baseFirst, targetLast) < math.EPSILON)
            {
                float3 connectionPoint = (baseFirst + targetLast) / 2;
                baseSpline[0] = new BezierKnot(connectionPoint);
                for (int i = targetSpline.Count - 2; i >= 0; i--)
                    baseSpline.Insert(0, targetSpline[i]);
                return true;
            }

            // Case 3: base last connects to target last (reverse target spline)
            if (!Skip(baseLast) && !Skip(targetLast) &&
                math.distance(baseLast, targetLast) < math.EPSILON)
            {
                float3 connectionPoint = (baseLast + targetLast) / 2;
                baseSpline[baseSpline.Count - 1] = new BezierKnot(connectionPoint);
                for (int i = targetSpline.Count - 2; i >= 0; i--)
                    baseSpline.Add(targetSpline[i]);
                return true;
            }

            // Case 4: base first connects to target first (reverse target spline)
            if (!Skip(baseFirst) && !Skip(targetFirst) &&
                math.distance(baseFirst, targetFirst) < math.EPSILON)
            {
                float3 connectionPoint = (baseFirst + targetFirst) / 2;
                baseSpline[0] = new BezierKnot(connectionPoint);
                for (int i = 1; i < targetSpline.Count; i++)
                    baseSpline.Insert(0, targetSpline[i]);
                return true;
            }

            return false;
        }

        public static BezierKnot[] ToKnots(this List<float3> points, bool closed = false)
        {
            var length = points.Count - Convert.ToInt32(closed);
            var knots = new BezierKnot[length];
            for (int i = 0; i < length; i++)
                knots[i] = new BezierKnot(points[i]);
            return knots;
        }

        public static List<BezierKnot> RemoveInline(this IEnumerable<BezierKnot> knots, float tolerance = 0.1f)
        {
            var points = knots.ToArray();
            if (points.Length < 3)
                return new List<BezierKnot>(points); // Not enough vectorPoints to simplify

            List<BezierKnot> cleanedKnots = new List<BezierKnot>();

            for (int i = 0; i < points.Length; i++)
            {
                BezierKnot prevKnot = points[(i - 1 + points.Length) % points.Length];
                BezierKnot currentKnot = points[i];
                BezierKnot nextKnot = points[(i + 1) % points.Length];

                // Calculate the direction vectors (consider only x and z)
                Vector2 dirPrevToCurrent = new Vector2(currentKnot.Position.x - prevKnot.Position.x, currentKnot.Position.z - prevKnot.Position.z).normalized;
                Vector2 dirCurrentToNext = new Vector2(nextKnot.Position.x - currentKnot.Position.x, nextKnot.Position.z - currentKnot.Position.z).normalized;

                // Local function to check if directions are approximately equal
                bool ApproximatelyEqual(Vector2 v1, Vector2 v2, float tolerance)
                    => Mathf.Abs(v1.x - v2.x) < tolerance && Mathf.Abs(v1.y - v2.y) < tolerance;

                // If directions are not almost equal, keep the knot
                if (!ApproximatelyEqual(dirPrevToCurrent, dirCurrentToNext, tolerance))
                    cleanedKnots.Add(currentKnot);
            }

            return cleanedKnots;
        }

        public static SplineKnotIndex? FindClosestKnotIndex(this SplineContainer container, Vector3 point)
        {
            var spline = container.Spline;
            float minDistance = float.MaxValue;
            SplineKnotIndex? closestIndex = null;

            for (int i = 0; i < spline.Count; i++)
            {
                float distance = Vector3.Distance(spline[i].Position, point);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestIndex = new SplineKnotIndex(0, i); // 0 as the spline index for single-spline containers
                }
            }

            return closestIndex;
        }

        public static SplineContainer CopySplineContainerFrom(this GameObject targetObject, GameObject sourceObject)
        {
            var source = sourceObject.GetComponent<SplineContainer>();

            var target = targetObject.GetComponent<SplineContainer>();
            if (target == null)
                target = targetObject.AddComponent<SplineContainer>();

            while (target.Splines.Count > 0)
                target.RemoveSplineAt(0);

            for (int i = 0; i < source.Splines.Count; i++)
            {
                var sourceSpline = source.Splines[i];
                var newSpline = new Spline();

                foreach (var knot in sourceSpline)
                {
                    var newKnot = new BezierKnot
                    {
                        Position = knot.Position,
                        TangentIn = knot.TangentIn,
                        TangentOut = knot.TangentOut,
                        Rotation = knot.Rotation
                    };
                    newSpline.Add(newKnot);
                }

                newSpline.Closed = sourceSpline.Closed;
                target.AddSpline(newSpline);
            }

            return target;
        }
    }
}