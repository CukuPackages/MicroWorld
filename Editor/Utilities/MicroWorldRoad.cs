using JBooth.MicroVerseCore;
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
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
            if (roadAssets == null || roadAssets.RoadSystem == null)
            {
                Debug.LogError("RoadAssets or RoadSystem prefab is missing.");
                return;
            }

            var roadSystem = PrefabUtility.InstantiatePrefab(
                roadAssets.RoadSystem, MicroVerse.instance.transform) as GameObject;

            var roadSplines = Children(RoadsParentLabel);

            for (int i = 0; i < roadSplines.Length; i++)
            {
                var originalRoad = roadSplines[i];

                if (originalRoad.GetComponent<SplineContainer>() == null)
                {
                    Debug.LogWarning($"Skipping object {originalRoad.name}: no SplineContainer found.");
                    continue;
                }

                var roadClone = GameObject.Instantiate(originalRoad).gameObject;
                roadClone.name = originalRoad.name + "_Clone";

                var roadInstance = (PrefabUtility.InstantiatePrefab(
                    roadAssets.GetRoad("2"), roadSystem.transform) as GameObject).GetComponent<Road>();

                roadInstance.defaultChoiceData.roadPrefab = roadInstance.config.entries[0].prefab;

                var splineContainer = roadInstance.gameObject.CopySplineContainerFrom(roadClone);

                if (splineContainer == null || splineContainer.Splines.Count == 0)
                {
                    Debug.LogError($"SplineContainer copy failed or is empty for {originalRoad.name}");
                    GameObject.DestroyImmediate(roadInstance.gameObject);
                    GameObject.DestroyImmediate(roadClone);
                    continue;
                }

                splineContainer.SetTangentMode(TangentMode.AutoSmooth);
                roadInstance.splineContainer.PivotAtStart();

                GameObject.DestroyImmediate(roadInstance.gameObject.GetComponent<MeshRenderer>());
                GameObject.DestroyImmediate(roadInstance.gameObject.GetComponent<MeshFilter>());

                EditorUtility.SetDirty(roadInstance.gameObject);
                EditorUtility.SetDirty(splineContainer);
                EditorUtility.SetDirty(roadSystem);

                GameObject.DestroyImmediate(roadClone);
            }

            roadSystem.GetComponent<RoadSystem>().ReGenerateRoads();
            EditorSceneManager.MarkSceneDirty(roadSystem.scene);
        }
    }
}
