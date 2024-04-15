using UnityEngine;
using Unity.Mathematics;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.Splines;

namespace Cuku.MicroWorld
{
    public static class Utilities
    {
        #region Spline

        public static BezierKnot[] ToKnots(this float3[] points)
        {
            var bezierKnots = new BezierKnot[points.Length];
            for (int i = 0; i < points.Length; i++)
                bezierKnots[i] = new BezierKnot(points[i]);
            return bezierKnots;
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

        public static void AdaptVolumeToSpline(this Transform target, SplineContainer splineContainer)
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

        #endregion
    }
}