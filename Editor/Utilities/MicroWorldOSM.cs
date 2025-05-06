using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;
using static Cuku.MicroWorld.MicroWorld;

namespace Cuku.MicroWorld
{
    public static class MicroWorldOSM
    {
        /// <summary>
        /// Select 1 <see cref="OSMExtractor"/> asset and 1 <see cref="MicroWorldArea"/> asset from the Project,
        /// and it will set them up in the scene.
        /// </summary>
        [MenuItem(nameof(MicroWorld) + "/OSM/Setup Elements", priority = 1)]
        internal static void SetupElements()
        {
            var dataAssets = Array.FindAll(Selection.objects, obj => obj is OSMExtractor)
                .Select(obj => obj as OSMExtractor).ToArray();
            if (dataAssets.Length != 1)
            {
                Debug.LogError($"Select exactly 1 {nameof(OSMExtractor)} file!");
                return;
            }

            var dataAsset = dataAssets[0];
            var data = string.Empty;
            try
            {
                data = File.ReadAllText(dataAsset.DataPath());
            }
            catch (Exception e)
            {
                Debug.LogError("Can't extract data: " + e.Message);
            }

            Coordinate[][] elements = default;
            try
            {
                elements = JsonConvert.DeserializeObject<Coordinate[][]>(data);
            }
            catch (Exception e)
            {
                Debug.LogError("Extracted data is invalid: " + e.Message);
            }

            GameObject prefab = default;
            try
            {
                prefab = Array.FindAll(Selection.objects, obj => obj is GameObject)
                    .Select(obj => obj as GameObject).ToArray()[0];
            }
            catch (Exception e)
            {
                Debug.LogError($"Select one prefab with {nameof(SplineContainer)} component: " + e.Message);
            }
            if (!prefab.GetComponent<SplineContainer>())
            {
                Debug.LogError($"Prefab is missing {nameof(SplineContainer)} component!");
                return;
            }

            var elementsParent = new GameObject(Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(dataAsset))).transform;
            var tiles = JsonConvert.DeserializeObject<Tile[]>(File.ReadAllText(
                Path.Combine(MicroVerseTerrainDataPath(GameObject.FindFirstObjectByType<Terrain>(FindObjectsInactive.Include).terrainData), nameof(Tile) + ".json")));
            foreach (var element in elements.ToWorldPoints(ref tiles))
            {
                var splineContainer = (PrefabUtility.InstantiatePrefab(prefab, parent: elementsParent) as GameObject)
                    .GetComponent<SplineContainer>();
                var closed = splineContainer.Spline.Closed;
                var spline = new Spline(element.ToKnots(closed));
                spline.SetTangentMode(TangentMode.Linear);
                spline.Closed = closed;
                splineContainer.Spline = spline;
                SplineExtensions.SnapSplineToTerrain(ref splineContainer);
            }
            Debug.Log($"{nameof(SetupElements)}: {elements.Length}");
        }

        [MenuItem(nameof(MicroWorld) + "/OSM/Filter Elements by Spline", priority = 2)]
        internal static void FilterElements()
        {
            SplineContainer targetSpline = default;
            try
            {
                targetSpline = Selection.gameObjects.FirstOrDefault(go => !go.GetComponent<MicroWorldArea>())
                    .GetComponent<SplineContainer>();
            }
            catch (Exception e)
            {
                Debug.LogError($"Select 1 {nameof(SplineContainer)} to use as Filter: {e}");
                return;
            }
            // Area splines
            var areas = Selection.gameObjects.Where(go => go.GetComponent<MicroWorldArea>())
                .Select(go => go.GetComponent<SplineContainer>()).ToArray();
            if (areas.Length < 1)
            {
                Debug.LogError($"Select at least 1 {nameof(MicroWorldArea)} to Filter!");
                return;
            }
            for (var i = 0; i < areas.Length; i++)
            {
                var area = areas[i];
                var include = false;
                var points = area.Spline.Knots;
                foreach (var point in points)
                {
                    float t;
                    SplineUtility.GetNearestPoint(targetSpline.Spline, point.Position, out _, out t);
                    var pos = SplineUtility.EvaluatePosition(targetSpline.Spline, t);
                    if (math.distance(SplineUtility.EvaluatePosition(targetSpline.Spline, t), point.Position) <= 3000)
                    {
                        include = true;
                        continue;
                    }
                }
                if (!include)
                {
                    Undo.RecordObject(area.gameObject, nameof(FilterElements));
                    area.gameObject.SetActive(false);
                    EditorUtility.SetDirty(area.gameObject);
                }
            }
        }
    }
}
