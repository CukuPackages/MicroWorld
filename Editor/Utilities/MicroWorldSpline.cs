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

        [MenuItem(nameof(MicroWorld) + "/Spline/Crop", priority = 2)]
        internal static void Crop()
        {
            Debug.Log("Cropping Splines...");

            var startTime = System.DateTime.Now;

            // Find the Cropper spline by name
            var cropperObject = GameObject.Find("Cropper");
            if (cropperObject == null)
            {
                Debug.LogError("Cropper object not found. Make sure there is a GameObject named 'Cropper' in the scene.");
                return;
            }

            var cropper = cropperObject.GetComponent<SplineContainer>();
            if (cropper == null)
            {
                Debug.LogError("Cropper object does not have a SplineContainer component.");
                return;
            }

            int cropped = 0;
            foreach (var splineContainer in Selection.gameObjects
                         .SelectMany(go => go.GetComponentsInChildren<SplineContainer>())
                         .Where(splineContainer => splineContainer != null))
            {
                var spline = splineContainer.Splines[0];
                var knots = spline.Knots.ToList(); // Copy to avoid modifying the collection during iteration
                foreach (var knot in knots)
                    if (!cropper.Contains(knot.Position))
                        spline.Remove(knot); // Remove the knot if it's outside

                // Destroy the GameObject if the spline has no points left
                if (!spline.Knots.Any())
                {
                    Object.DestroyImmediate(splineContainer.gameObject);
                    cropped++;
                }
            }

            var timePassed = System.DateTime.Now - startTime;
            Debug.Log($"Cropped {cropped} in {(int)timePassed.TotalMinutes:00}:{timePassed.Seconds:00}");
        }


        [MenuItem(nameof(MicroWorld) + "/Spline/Split Intersecting", priority = 3)]
        internal static void SplitIntersecting()
        {
            Debug.Log("Split Intersecting...");

            var startTime = System.DateTime.Now;

            bool canSplit;
            do
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

                canSplit = false;

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
                        canSplit = true;
                        GameObject.DestroyImmediate(container.gameObject);
                    }
            } while (canSplit);

            var timePassed = System.DateTime.Now - startTime;
            Debug.Log($"{(int)timePassed.TotalMinutes:00}:{timePassed.Seconds:00}");
        }

        [MenuItem(nameof(MicroWorld) + "/Spline/Create Intersections", priority = 4)]
        internal static void CreateIntersections()
        {
            Debug.Log("Create Intersections...");

            var startTime = System.DateTime.Now;

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

            var timePassed = System.DateTime.Now - startTime;
            Debug.Log($"{(int)timePassed.TotalMinutes:00}:{timePassed.Seconds:00}");
        }

        [MenuItem(nameof(MicroWorld) + "/Spline/Connect Continuous", priority = 5)]
        internal static void ConnectContinuous()
        {
            Debug.Log("Connect Continuous...");

            var startTime = System.DateTime.Now;

            var splineContainers = Selection.gameObjects
                .SelectMany(go => go.GetComponentsInChildren<SplineContainer>())
                .Where(splineContainer => splineContainer != null)
                .ToList();

            if (splineContainers.Count < 2)
            {
                Debug.LogWarning("Please select at least two spline containers to connect.");
                return;
            }

            // Get intersection positions
            var intersectionPositions = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None)
                .Where(intersection => intersection.name.Contains("Intersection "))
                .Select(intersection => intersection.transform.position)
                .ToArray()
                .ToFloat3();

            int count = 0;
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
                        if (baseSpline.Connect(targetSpline, intersectionPositions))
                        {
                            Object.DestroyImmediate(targetSpline.gameObject);
                            splineContainers.RemoveAt(j);
                            j--; // Adjust index after removal
                            count++;
                            merged = true;
                        }
                    }
                }
            } while (merged);

            var timePassed = System.DateTime.Now - startTime;
            Debug.Log($"Connected {count} splines in ({$"{(int)timePassed.TotalMinutes:00}:{timePassed.Seconds:00}"})");
        }

        [MenuItem(nameof(MicroWorld) + "/Spline/Smooth", priority = 6)]
        internal static void Smooth()
        {
            Debug.Log("Smooth Splines...");

            var startTime = System.DateTime.Now;

            foreach (var splineContainer in Selection.gameObjects.SelectMany(go => go.GetComponentsInChildren<SplineContainer>())
                    .Where(splineContainer => splineContainer != null))
                splineContainer.SetTangentMode(TangentMode.AutoSmooth);

            var timePassed = System.DateTime.Now - startTime;
            Debug.Log($"{(int)timePassed.TotalMinutes:00}:{timePassed.Seconds:00}");
        }
    }
}
