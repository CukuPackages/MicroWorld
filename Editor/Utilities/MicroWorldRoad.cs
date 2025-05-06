using JBooth.MicroVerseCore;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;
using static Cuku.MicroWorld.MicroWorld;
using static Cuku.MicroWorld.MicroWorldSpline;

namespace Cuku.MicroWorld
{
    public static class MicroWorldRoad
    {
        const string RoadsParentLabel = "Roads";

        [MenuItem(nameof(MicroWorld) + "/Road/Setup Splines", priority = 1)]
        internal static void SetupSplines()
        {
            Debug.Log("Setup Intersections and Roads Splines...");

            var startTime = DateTime.Now;

            Crop();
            SplitIntersecting();
            CreateIntersections();
            ConnectContinuous();

            var timePassed = DateTime.Now - startTime;
            Debug.Log($"Setup Intersection and Road Splines in {(int)timePassed.TotalMinutes:00}:{timePassed.Seconds:00}");
        }

        [MenuItem(nameof(MicroWorld) + "/Road/Setup Roads", priority = 2)]
        internal static void SetupRoads()
        {
            Debug.Log("Setup Roads...");

            var roadAssets = Array.Find(Selection.objects, obj => obj is ScriptableObject) as RoadAssets;

            var roadSystem = PrefabUtility.InstantiatePrefab(
                    roadAssets.RoadSystem, MicroVerse.instance.transform) as GameObject;

            //var intersectionPositions = Children(IntersectionsParentLabel);
            //var intersections = new Intersection[intersectionPositions.Length];
            //for (int i = 0; i < intersectionPositions.Length; i++)
            //{
            //    var intersection = intersectionPositions[i];
            //    var intersectionInstance = PrefabUtility.InstantiatePrefab(
            //        roadAssets.GetIntersection(intersection.name.Split(IntersectionLabel)[1]),
            //        roadSystem.transform) as GameObject;
            //    intersectionInstance.transform.position = intersection.position;
            //    intersections[i] = intersectionInstance.GetComponent<Intersection>();
            //}

            var roadSplines = Children(RoadsParentLabel);
            for (int i = 0; i < roadSplines.Length; i++)
            {
                var road = roadSplines[i];
                var roadInstance = (PrefabUtility.InstantiatePrefab(
                    roadAssets.GetRoad("2"),
                    roadSystem.transform) as GameObject).GetComponent<Road>();
                roadInstance.defaultChoiceData.roadPrefab = roadInstance.config.entries[0].prefab;
                var splineContainer = roadInstance.gameObject.CopySplineContainerFrom(road.gameObject);
                splineContainer.SetTangentMode(TangentMode.AutoSmooth);
                roadInstance.splineContainer.PivotAtStart();
                GameObject.DestroyImmediate(roadInstance.gameObject.GetComponent<MeshRenderer>());
                GameObject.DestroyImmediate(roadInstance.gameObject.GetComponent<MeshFilter>());
            }

            roadSystem.GetComponent<RoadSystem>().ReGenerateRoads();
        }
    }
}
