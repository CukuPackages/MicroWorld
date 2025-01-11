using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;

namespace Cuku.MicroWorld
{
    public static class MicroWorldSpline
    {
        [MenuItem(nameof(MicroWorld) + "/Spline/Center Pivot", priority = 1)]
        internal static void CenterPivot()
        {
            foreach (var splineContainer in Selection.gameObjects.SelectMany(go => go.GetComponentsInChildren<SplineContainer>())
                    .Where(splineContainer => splineContainer != null))
                splineContainer.CenterPivot();
        }

        [MenuItem(nameof(MicroWorld) + "/Spline/Split Intersecting", priority = 2)]
        internal static void SplitIntersecting()
        {
            var splineContainers = Selection.gameObjects
                .SelectMany(go => go.GetComponentsInChildren<SplineContainer>())
                .Where(splineContainer => splineContainer != null)
                .ToList();

            if (splineContainers.Count < 2)
            {
                Debug.LogWarning("Please select at least two spline containers to detect intersections.");
                return;
            }

            foreach (var containerA in splineContainers)
                foreach (var containerB in splineContainers)
                {
                    if (containerA == containerB)
                        continue;

                    var splineA = containerA.Spline;
                    var splineB = containerB.Spline;

                    // Iterate through the inner points of splineA
                    for (int i = 1; i < splineA.Count - 1; i++)
                    {
                        var pointA = splineA[i].Position;

                        // Compare with all points of splineB
                        for (int j = 0; j < splineB.Count; j++)
                        {
                            var pointB = splineB[j].Position;

                            // Check if positions match
                            if (pointA.Equals(pointB))
                            {
                                // Split splineA at the matching point
                                var knotIndexA = containerA.FindClosestKnotIndex(pointA);
                                if (knotIndexA.HasValue)
                                {
                                    SplineUtility.SplitSplineOnKnot(containerA, knotIndexA.Value);
                                }

                                // Split splineB at the matching point
                                var knotIndexB = containerB.FindClosestKnotIndex(pointB);
                                if (knotIndexB.HasValue)
                                {
                                    SplineUtility.SplitSplineOnKnot(containerB, knotIndexB.Value);
                                }
                            }
                        }
                    }
                }

            // Split and move resulting splines to new GameObjects
            foreach (var container in splineContainers)
                if (container.Splines.Count > 1)
                {
                    var parent = container.transform.parent;
                    for (int i = 0; i < container.Splines.Count; i++)
                    {
                        var newGameObject = new GameObject(container.gameObject.name);
                        newGameObject.transform.SetParent(parent, worldPositionStays: false);
                        var newSplineContainer = newGameObject.AddComponent<SplineContainer>();
                        newSplineContainer.Spline = container.Splines[i];
                    }

                    GameObject.DestroyImmediate(container.gameObject);
                }
        }

        [MenuItem(nameof(MicroWorld) + "/Spline/Create Intersections", priority = 3)]
        internal static void CreateIntersections()
        {
            var splineContainers = Selection.gameObjects
                .SelectMany(go => go.GetComponentsInChildren<SplineContainer>())
                .Where(splineContainer => splineContainer != null)
                .ToList();

            if (splineContainers.Count < 2)
            {
                Debug.LogWarning("Please select at least two spline containers to detect intersections.");
                return;
            }

            var intersectionDictionary = new Dictionary<Vector3, HashSet<int>>();

            for (int i = 0; i < splineContainers.Count; i++)
            {
                var splineA = splineContainers[i];
                var pointsA = new[]
                {
                    splineA.Spline[0].Position,
                    splineA.Spline[splineA.Spline.Count - 1].Position
                };

                for (int j = i + 1; j < splineContainers.Count; j++)
                {
                    var splineB = splineContainers[j];
                    var pointsB = new[]
                    {
                        splineB.Spline[0].Position,
                        splineB.Spline[splineB.Spline.Count - 1].Position
                    };

                    foreach (var pointA in pointsA)
                    {
                        foreach (var pointB in pointsB)
                        {
                            if (Vector3.Distance(pointA, pointB) < Mathf.Epsilon)
                            {
                                if (!intersectionDictionary.ContainsKey(pointA))
                                {
                                    intersectionDictionary[pointA] = new HashSet<int>();
                                }
                                intersectionDictionary[pointA].Add(i);
                                intersectionDictionary[pointA].Add(j);
                            }
                        }
                    }
                }
            }

            var intersectionsParent = new GameObject("Intersections").transform;
            foreach (var intersection in intersectionDictionary)
            {
                var count = intersection.Value.Count;
                if (count >= 3)
                {
                    var intersectionObject = new GameObject($"Intersection {count}");
                    intersectionObject.transform.position = intersection.Key;
                    intersectionObject.transform.SetParent(intersectionsParent);
                }
            }
        }

        [MenuItem(nameof(MicroWorld) + "/Spline/Connect Continuous", priority = 4)]
        internal static void ConnectContinuous()
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
                        if (baseSpline.Connect(targetSpline))
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

        [MenuItem(nameof(MicroWorld) + "/Spline/Smooth", priority = 5)]
        internal static void Smooth()
        {
            foreach (var splineContainer in Selection.gameObjects.SelectMany(go => go.GetComponentsInChildren<SplineContainer>())
                    .Where(splineContainer => splineContainer != null))
                splineContainer.SetTangentMode(TangentMode.AutoSmooth);
        }
    }
}
