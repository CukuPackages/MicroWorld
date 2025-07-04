using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;
using static Cuku.MicroWorld.MicroWorld;

namespace Cuku.MicroWorld
{
    public static class MicroWorldSpline
    {
        public const string CropperLabel = "Cropper";
        public const string IntersectionsParentLabel = "Intersections Parent";
        public const string IntersectionLabel = "Intersection ";

        [MenuItem(nameof(MicroWorld) + "/Spline/Pivot at Center", priority = 1)]
        internal static void PivotAtCenter()
        {
            foreach (var splineContainer in Selection.gameObjects.SelectMany(go => go.GetComponentsInChildren<SplineContainer>())
                    .Where(splineContainer => splineContainer != null))
                splineContainer.PivotAtCenter();
        }

        [MenuItem(nameof(MicroWorld) + "/Spline/Pivot at Start", priority = 2)]
        internal static void PivotAtStart()
        {
            foreach (var splineContainer in Selection.gameObjects.SelectMany(go => go.GetComponentsInChildren<SplineContainer>())
                    .Where(splineContainer => splineContainer != null))
                splineContainer.PivotAtStart();
        }

        [MenuItem(nameof(MicroWorld) + "/Spline/Pivot at World Origin", priority = 3)]
        internal static void PivotAtWorldOrigin()
        {
            foreach (var splineContainer in Selection.gameObjects.SelectMany(go => go.GetComponentsInChildren<SplineContainer>())
                    .Where(splineContainer => splineContainer != null))
                splineContainer.PivotAtWorldOrigin();
        }

        [MenuItem(nameof(MicroWorld) + "/Spline/Crop", priority = 4)]
        internal static void Crop()
        {
            Debug.Log("Cropping Splines...");

            var startTime = DateTime.Now;

            // Find the Cropper spline by name
            var cropperObject = GameObject.Find(CropperLabel);
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
                var knots = spline.Knots.ToList();
                foreach (var knot in knots)
                    if (!cropper.Contains(knot.Position))
                        spline.Remove(knot);

                if (!spline.Knots.Any())
                {
                    UnityEngine.Object.DestroyImmediate(splineContainer.gameObject);
                    cropped++;
                    continue;
                }
                EditorUtility.SetDirty(splineContainer.gameObject);
            }

            var timePassed = DateTime.Now - startTime;
            Debug.Log($"Cropped {cropped} in {(int)timePassed.TotalMinutes:00}:{timePassed.Seconds:00}");
        }

        [MenuItem(nameof(MicroWorld) + "/Spline/Split Intersecting", priority = 5)]
        internal static void SplitIntersecting()
        {
            Debug.Log("Split Intersecting...");

            var startTime = DateTime.Now;

            bool canSplit;
            do
            {
                var splineContainers = Selection.gameObjects
                    .SelectMany(go => go.GetComponentsInChildren<SplineContainer>())
                    .Where(go => go != null)
                    .ToArray();

                if (splineContainers.Length < 2)
                {
                    Debug.LogWarning("Select at least two spline containers to detect intersections!");
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
                                        SplineUtility.SplitSplineOnKnot(containerA, knotIndexA.Value);

                                    // Split splineB at the matching point
                                    var knotIndexB = containerB.FindClosestKnotIndex(pointB);
                                    if (knotIndexB.HasValue)
                                        SplineUtility.SplitSplineOnKnot(containerB, knotIndexB.Value);
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
                            EditorUtility.SetDirty(newGameObject);
                        }
                        canSplit = true;
                        GameObject.DestroyImmediate(container.gameObject);
                    }
            } while (canSplit);

            var timePassed = DateTime.Now - startTime;
            Debug.Log($"{(int)timePassed.TotalMinutes:00}:{timePassed.Seconds:00}");
        }

        [MenuItem(nameof(MicroWorld) + "/Spline/Create Intersections", priority = 6)]
        internal static void CreateIntersections()
        {
            Debug.Log("Create Intersections...");

            var startTime = DateTime.Now;

            var splineContainers = Selection.gameObjects
                .SelectMany(go => go.GetComponentsInChildren<SplineContainer>())
                .Where(splineContainer => splineContainer != null)
                .ToArray();

            if (splineContainers.Length < 2)
            {
                Debug.LogWarning("Please select at least two spline containers to detect intersections.");
                return;
            }

            var intersectionDictionary = new Dictionary<Vector3, HashSet<int>>();

            for (int i = 0; i < splineContainers.Length; i++)
            {
                var splineA = splineContainers[i];
                var pointsA = new[]
                {
                    splineA.Spline[0].Position,
                    splineA.Spline[splineA.Spline.Count - 1].Position
                };

                for (int j = i + 1; j < splineContainers.Length; j++)
                {
                    var splineB = splineContainers[j];
                    var pointsB = new[]
                    {
                        splineB.Spline[0].Position,
                        splineB.Spline[splineB.Spline.Count - 1].Position
                    };

                    foreach (var pointA in pointsA)
                        foreach (var pointB in pointsB)
                            if (Vector3.Distance(pointA, pointB) < Mathf.Epsilon)
                            {
                                if (!intersectionDictionary.ContainsKey(pointA))
                                    intersectionDictionary[pointA] = new HashSet<int>();
                                intersectionDictionary[pointA].Add(i);
                                intersectionDictionary[pointA].Add(j);
                            }
                }
            }

            var intersectionsParent = new GameObject(IntersectionsParentLabel).transform;
            foreach (var intersection in intersectionDictionary)
            {
                var count = intersection.Value.Count;
                if (count >= 3)
                {
                    var intersectionObject = new GameObject($"{IntersectionLabel}{count}");
                    intersectionObject.transform.position = intersection.Key;
                    intersectionObject.transform.SetParent(intersectionsParent);
                    EditorUtility.SetDirty(intersectionObject);
                }
            }

            var timePassed = DateTime.Now - startTime;
            Debug.Log($"{(int)timePassed.TotalMinutes:00}:{timePassed.Seconds:00}");
        }

        [MenuItem(nameof(MicroWorld) + "/Spline/Connect", priority = 7)]
        internal static void Connect()
        {
            GameObject[] selected = Selection.gameObjects;

            if (selected.Length < 2)
            {
                Debug.LogError("Select at least two GameObjects with SplineContainer components.");
                return;
            }

            SplineContainer targetContainer = selected[0].GetComponent<SplineContainer>();
            if (targetContainer == null)
            {
                Debug.LogError("The first selected GameObject must have a SplineContainer component.");
                return;
            }

            Spline targetSpline = targetContainer.Spline;
            Undo.RegisterCompleteObjectUndo(targetContainer, "Connect Splines");

            int totalAdded = 0;

            for (int i = 1; i < selected.Length; i++)
            {
                SplineContainer sourceContainer = selected[i].GetComponent<SplineContainer>();
                if (sourceContainer == null) continue;

                Spline sourceSpline = sourceContainer.Spline;

                for (int j = 0; j < sourceSpline.Count; j++)
                {
                    BezierKnot knot = sourceSpline[j];
                    targetSpline.Add(knot);
                    totalAdded++;
                }
            }

            for (int i = 1; i < selected.Length; i++)
            {
                UnityEngine.Object.DestroyImmediate(selected[i]);
            }

            EditorUtility.SetDirty(selected[0]);
            Debug.Log($"Connected splines. {totalAdded} knots added to {selected[0].name}.");
        }

        [MenuItem(nameof(MicroWorld) + "/Spline/Connect Continuous", priority = 8)]
        internal static void ConnectContinuous()
        {
            Debug.Log("Connect Continuous...");

            var startTime = DateTime.Now;

            var splineContainers = Selection.gameObjects
                .SelectMany(go => go.GetComponentsInChildren<SplineContainer>())
                .Where(splineContainer => splineContainer != null)
                .ToList();

            if (splineContainers.Count < 2)
            {
                Debug.LogWarning("Select at least two spline containers to connect!");
                return;
            }

            // Get intersections
            var intersections = Children(IntersectionsParentLabel)
                .Select(intersection => intersection.position)
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
                        if (baseSpline.Connect(targetSpline, intersections))
                        {
                            UnityEngine.Object.DestroyImmediate(targetSpline.gameObject);
                            splineContainers.RemoveAt(j);
                            j--;
                            count++;
                            merged = true;
                        }
                        EditorUtility.SetDirty(baseSpline.gameObject);
                    }
                }
            } while (merged);

            var timePassed = DateTime.Now - startTime;
            Debug.Log($"Connected {count} splines in ({$"{(int)timePassed.TotalMinutes:00}:{timePassed.Seconds:00}"})");
        }

        [MenuItem(nameof(MicroWorld) + "/Spline/Smooth", priority = 9)]
        internal static void Smooth()
        {
            Debug.Log("Smooth Splines...");

            var startTime = DateTime.Now;

            foreach (var splineContainer in Selection.gameObjects.SelectMany(go => go.GetComponentsInChildren<SplineContainer>())
                    .Where(splineContainer => splineContainer != null))
            {
                splineContainer.SetTangentMode(TangentMode.AutoSmooth);
                EditorUtility.SetDirty(splineContainer.gameObject);
            }

            var timePassed = DateTime.Now - startTime;
            Debug.Log($"{(int)timePassed.TotalMinutes:00}:{timePassed.Seconds:00}");
        }
    }
}
