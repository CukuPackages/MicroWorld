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

        public static BezierKnot[] ToBezierKnots(this float3[] points)
        {
            var bezierKnots = new BezierKnot[points.Length];
            for (int i = 0; i < points.Length; i++)
                bezierKnots[i] = new BezierKnot(points[i]);
            return bezierKnots;
        }

        public static void ShiftSplineKnots(ref SplineContainer splineContainer, Vector3 shift)
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
            Terrain[] terrains = Terrain.activeTerrains;
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
            var points = new List<float3>();
            foreach (var spline in splineContainer.Splines)
                foreach (var knot in spline.Knots)
                    points.Add(knot.Position);

            // Calculate center
            float3 sum = float3.zero;
            foreach (float3 point in points)
                sum += point;
            float3 center = sum / points.Count;

            // Calculate the dimensions of the bounding box
            var min = new float3(float.MaxValue, float.MaxValue, float.MaxValue);
            var max = new float3(float.MinValue, float.MinValue, float.MinValue);
            foreach (float3 point in points)
            {
                min = math.min(min, point);
                max = math.max(max, point);
            }
            var dimensions = max - min;

            target.position = (Vector3)center;
            target.localScale = new Vector3(dimensions.x, target.localScale.y, dimensions.z);
        }

        #endregion
    }
}