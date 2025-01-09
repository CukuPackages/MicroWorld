using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

namespace Cuku.MicroWorld
{
    public static class MicroWorldSpline
    {
        [MenuItem(nameof(MicroWorld) + "/Spline/Center Pivot", priority = 1)]
        internal static void CenterPivot()
        {
            foreach (var splineContainer in Selection.gameObjects.SelectMany(go => go.GetComponentsInChildren<SplineContainer>())
                .Where(splineContainer => splineContainer != null).ToArray())
            {
                var shift = (float3)splineContainer.transform.position;
                splineContainer.transform.position = Vector3.zero;
                var knots = splineContainer.Spline.Knots.ToArray();
                for (int i = 0; i < knots.Length; i++)
                {
                    var knot = knots[i];
                    knot.Position += shift;
                    splineContainer.Spline.SetKnot(i, knot);
                }
            }
        }

        [MenuItem(nameof(MicroWorld) + "/Spline/Smooth", priority = 2)]
        internal static void Smooth()
        {
            foreach (var splineContainer in Selection.gameObjects.SelectMany(go => go.GetComponentsInChildren<SplineContainer>())
                    .Where(splineContainer => splineContainer != null).ToArray())
                splineContainer.SetTangentMode(TangentMode.AutoSmooth);
        }

        [MenuItem(nameof(MicroWorld) + "/Spline/Connect Continuous", priority = 3)]
        internal static void Connect()
        {
            var splineContainers = Selection.gameObjects
                .SelectMany(go => go.GetComponentsInChildren<SplineContainer>())
                .Where(splineContainer => splineContainer != null)
                .ToList();

            if (splineContainers.Count < 2)
            {
                Debug.LogWarning("Please select at least two spline containers to connect.");
                return;
            }

            int mergeCount = 0;
            bool merged;
            do
            {
                merged = false;
                for (int i = 0; i < splineContainers.Count; i++)
                {
                    var baseSpline = splineContainers[i];
                    for (int j = i + 1; j < splineContainers.Count; j++)
                    {
                        var targetSpline = splineContainers[j];
                        if (TryConnectSplines(baseSpline, targetSpline))
                        {
                            Object.DestroyImmediate(targetSpline.gameObject);
                            splineContainers.RemoveAt(j);
                            j--; // Adjust index after removal
                            mergeCount++;
                            merged = true;
                        }
                    }
                }
            } while (merged);

            Debug.Log($"Merged {mergeCount} splines.");
        }

        private static bool TryConnectSplines(SplineContainer baseSplineContainer, SplineContainer targetSplineContainer)
        {
            var baseSpline = baseSplineContainer.Spline;
            var targetSpline = targetSplineContainer.Spline;

            var baseFirst = baseSpline[0].Position;
            var baseLast = baseSpline[baseSpline.Count - 1].Position;

            var targetFirst = targetSpline[0].Position;
            var targetLast = targetSpline[targetSpline.Count - 1].Position;

            // Case 1: base last connects to target first
            if (Vector3.Distance(new Vector3(baseLast.x, 0, baseLast.z), new Vector3(targetFirst.x, 0, targetFirst.z)) < Mathf.Epsilon)
            {
                Vector3 connectionPoint = (baseLast + targetFirst) / 2;
                baseSpline[baseSpline.Count - 1] = new BezierKnot(connectionPoint);
                foreach (var knot in targetSpline.Skip(1))
                    baseSpline.Add(knot);
                return true;
            }
            // Case 2: base first connects to target last
            else if (Vector3.Distance(new Vector3(baseFirst.x, 0, baseFirst.z), new Vector3(targetLast.x, 0, targetLast.z)) < Mathf.Epsilon)
            {
                Vector3 connectionPoint = (baseFirst + targetLast) / 2;
                baseSpline[0] = new BezierKnot(connectionPoint);
                for (int i = targetSpline.Count - 2; i >= 0; i--)
                    baseSpline.Insert(0, targetSpline[i]);
                return true;
            }
            // Case 3: base last connects to target last (reverse target spline)
            else if (Vector3.Distance(new Vector3(baseLast.x, 0, baseLast.z), new Vector3(targetLast.x, 0, targetLast.z)) < Mathf.Epsilon)
            {
                Vector3 connectionPoint = (baseLast + targetLast) / 2;
                baseSpline[baseSpline.Count - 1] = new BezierKnot(connectionPoint);
                for (int i = targetSpline.Count - 2; i >= 0; i--)
                    baseSpline.Add(targetSpline[i]);
                return true;
            }
            // Case 4: base first connects to target first (reverse target spline)
            else if (Vector3.Distance(new Vector3(baseFirst.x, 0, baseFirst.z), new Vector3(targetFirst.x, 0, targetFirst.z)) < Mathf.Epsilon)
            {
                Vector3 connectionPoint = (baseFirst + targetFirst) / 2;
                baseSpline[0] = new BezierKnot(connectionPoint);
                for (int i = 1; i < targetSpline.Count; i++)
                    baseSpline.Insert(0, targetSpline[i]);
                return true;
            }

            return false;
        }
    }
}
