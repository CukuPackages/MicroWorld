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

                // If signed points is negative, points are in clockwise order
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
            // Calculate the dimensions of the bounding box
            var min = new float3(float.MaxValue, float.MaxValue, float.MaxValue);
            var max = new float3(float.MinValue, float.MinValue, float.MinValue);
            var points = splineContainer.Points();
            for (int i = 0; i < points.Count; i++)
            {
                var point = points[i];
                min = math.min(min, point);
                max = math.max(max, point);
            }
            var dimensions = max - min;
            target.position = (Vector3)splineContainer.Center();
            target.localScale = new Vector3(dimensions.x, target.localScale.y, dimensions.z);
        }

        /// <summary>
        /// Get all <see cref="SplineContainer"/> points.
        /// </summary>
        public static List<float3> Points(this SplineContainer splineContainer)
        {
            var points = new List<float3>();
            foreach (var spline in splineContainer.Splines)
                foreach (var knot in spline.Knots)
                    points.Add(knot.Position);
            return points;
        }

        /// <summary>
        /// Get all <see cref="SplineContainer"/> points in (X, Z) plane.
        /// </summary>
        public static List<Vector2> Points2D(this SplineContainer splineContainer)
        {
            var points = new List<Vector2>();
            foreach (var spline in splineContainer.Splines)
                foreach (var knot in spline.Knots)
                    points.Add(new Vector2(knot.Position.x, knot.Position.z));
            return points;
        }

        /// <summary>
        /// Get <see cref="SplineContainer"/> center of all points.
        /// </summary>
        public static float3 Center(this SplineContainer splineContainer)
        {
            var points = splineContainer.Points();
            var sum = float3.zero;
            foreach (float3 point in points)
                sum += point;
            return sum / points.Count;
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

        public static BezierKnot[] ToKnots(this float3[] points, bool closed = false)
        {
            var length = points.Length - Convert.ToInt32(closed);
            var knots = new BezierKnot[length];
            for (int i = 0; i < length; i++)
                knots[i] = new BezierKnot(points[i]);
            return knots;
        }

        /// <summary>
        /// Remove redundant points within a straight line.
        /// </summary>
        public static List<BezierKnot> RemoveInline(this IEnumerable<BezierKnot> knots, float tolerance = 0.1f)
        {
            var points = knots.ToArray();
            if (points.Length < 3)
                return new List<BezierKnot>(points); // Not enough points to simplify

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
    }
}