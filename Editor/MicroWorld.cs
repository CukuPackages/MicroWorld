using JBooth.MicroSplat;
using JBooth.MicroVerseCore;
using JBooth.FoliageRendering;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Splines;
using Unity.Mathematics;

#if TERRALAND
using TerraUnity.TerraLand;
#endif

namespace Cuku.MicroWorld
{
    public static class MicroWorld
    {
        #region Paths

        internal static string MicroVerseTerrainDataPath(this TerrainData terrainData)
             => Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(AssetDatabase.GetAssetPath(terrainData))), nameof(MicroVerse));

        #endregion

        #region Terrain

        /// <summary>
        /// Select TerrainData assets from the Project and it will create Terrain GameObjects in the scene.
        /// </summary>
        [MenuItem(nameof(MicroWorld) + "/Create Terrain From Terrain Data", priority = 1)]
        internal static void CreateTerrainFromTerrainData()
        {
            var terrainDataAssets = Array.FindAll(Selection.objects, obj => obj is TerrainData)
                .Select(obj => obj as TerrainData).ToArray();

            if (terrainDataAssets == null || terrainDataAssets.Length == 0)
            {
                Debug.LogError("No terrain data assets selected!");
                return;
            }

            // Create parent GameObject to hold all terrains
            GameObject parentTerrain = new GameObject("Terrain");

            // Determine grid size based on the number of terrain data assets
            int gridSizeX = Mathf.CeilToInt(Mathf.Sqrt(terrainDataAssets.Length));
            int gridSizeZ = gridSizeX;

            // Calculate total width and depth of the grid
            float totalWidth = gridSizeX * terrainDataAssets[0].size.x;
            float totalDepth = totalWidth;

            // Calculate starting position of the grid to center it
            float startX = -totalWidth / 2f;
            float startZ = totalDepth / 2f - terrainDataAssets[0].size.z;

            // Create terrains in grid
            Terrain[,] terrains = new Terrain[gridSizeX, gridSizeZ];
            for (int z = 0; z < gridSizeZ; z++)
            {
                for (int x = 0; x < gridSizeX; x++)
                {
                    int index = x * gridSizeZ + z;
                    if (index >= terrainDataAssets.Length)
                        continue;

                    TerrainData terrainData = terrainDataAssets[index];

                    // Create a new terrain GameObject
                    GameObject terrainObject = Terrain.CreateTerrainGameObject(terrainData);

                    // Determine shift based on terrain data size
                    float shiftX = terrainData.size.x;
                    float shiftZ = terrainData.size.z;

                    // Calculate position based on grid and shift values
                    float posX = startX + x * shiftX;
                    float posZ = startZ - z * shiftZ; // Start from -X and +Z, only increment in positive Z direction
                    terrainObject.transform.position = new Vector3(posX, 0, posZ);

                    // Rename the terrain object based on terrain data name without "D"
                    terrainObject.name = terrainData.name.Replace("D", "");

                    // Set the parent of the terrain object
                    terrainObject.transform.SetParent(parentTerrain.transform);

                    // Store terrain in the array
                    terrains[x, z] = terrainObject.GetComponent<Terrain>();
                }
            }

            // Set neighbors for each terrain
            for (int z = 0; z < gridSizeZ; z++)
            {
                for (int x = 0; x < gridSizeX; x++)
                {
                    Terrain left = x > 0 ? terrains[x - 1, z] : null;
                    Terrain right = x < gridSizeX - 1 ? terrains[x + 1, z] : null;
                    Terrain top = z > 0 ? terrains[x, z - 1] : null;
                    Terrain bottom = z < gridSizeZ - 1 ? terrains[x, z + 1] : null;
                    terrains[x, z].SetNeighbors(left, top, right, bottom);
                }
            }

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        #endregion

        #region MicroVerse

        /// <summary>
        /// Select terrain or terrains parent GameObject in the Scene and it will setup MicroVerse.
        /// </summary>
        [MenuItem(nameof(MicroWorld) + "/Convert Terrain To MicroVerse", priority = 100)]
        internal static void ConvertTerrainToMicroVerse()
        {
            Debug.Log("Converting Terrain to MicroVerse...");
            var terrains = Selection.gameObjects.SelectMany(go => go.GetComponentsInChildren<Terrain>()).ToArray();
            if (terrains.Length == 0)
            {
                Debug.LogError($"Select Objects with '{nameof(Terrain)}' component!");
                return;
            }

            ConvertTerrainDataToHeightmap();

            var microverse = new GameObject(nameof(MicroVerse), typeof(MicroVerse)).transform;
            var terrainParent = new GameObject(nameof(Terrain)).transform;
            terrainParent.SetParent(microverse);
            var heightStampParent = new GameObject(nameof(HeightStamp)).transform;
            heightStampParent.SetParent(microverse);

            // Construct new directory for terrain data asset
            string terrainDirectory = Path.GetDirectoryName(AssetDatabase.GetAssetPath(terrains.First().terrainData));
            string newTerrainDirectory = terrains.First().terrainData.MicroVerseTerrainDataPath();
            if (Directory.Exists(newTerrainDirectory))
                Debug.Log("Terrain data is already extracted");
            else
            {
                DuplicateDirectory(terrainDirectory, newTerrainDirectory);
#if TERRALAND
                ExtractTerrainTilesInfo();
#else
                UnityEngine.Debug.LogError("TerraLand was not found!");
#endif
            }

            string[] assetPaths = AssetDatabase.FindAssets("", new string[] { newTerrainDirectory });
            foreach (string path in assetPaths)
                AssetDatabase.ImportAsset(AssetDatabase.GUIDToAssetPath(path));
            AssetDatabase.Refresh();

            string heightmapDirectory = Path.Combine(Path.GetDirectoryName(terrainDirectory), nameof(HeightStamp));

            var terrainDataSize = terrains[0].terrainData.size;
            var heightStampSize = new Vector3(terrainDataSize.x, 100, terrainDataSize.z);
            var heightStampShift = new Vector3(heightStampSize.x / 2f, 0, heightStampSize.z / 2f);

            for (int i = 0; i < terrains.Length; i++)
            {
                var terrain = terrains[i];
                var terrainName = terrain.terrainData.name;

                string newTerrainDataPath = Path.Combine(newTerrainDirectory, $"{terrainName}.asset");
                var newTerrainData = AssetDatabase.LoadAssetAtPath<TerrainData>(newTerrainDataPath);

                // Create terrain
                var newTerrain = Terrain.CreateTerrainGameObject(newTerrainData).transform;
                newTerrain.name = terrain.name;
                newTerrain.SetParent(terrainParent);
                var terrainPosition = new Vector3(terrain.transform.position.x, 0, terrain.transform.position.z);
                newTerrain.position = terrainPosition;

                // Create height stamp
                var heightStamp = new GameObject(terrainName, typeof(HeightStamp)).transform;
                heightStamp.SetParent(heightStampParent);
                heightStamp.position = terrainPosition + heightStampShift;
                heightStamp.localScale = heightStampSize;
                heightStamp.GetComponent<HeightStamp>().stamp = AssetDatabase.LoadAssetAtPath<Texture2D>(Path.Combine(heightmapDirectory, $"{terrainName}.tif"));
            }

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

#if TERRALAND
        /// <summary>
        /// Select terrain or terrains parent GameObject in the Scene and extract the data to a JSON file for further processing.
        /// </summary>
        [MenuItem(nameof(MicroWorld) + "/Extract Terrain Tiles Info", priority = 101)]
        internal static void ExtractTerrainTilesInfo()
        {
            Debug.Log("Extractng Terrain Tiles Info...");
            var tiles = Selection.gameObjects.SelectMany(go => go.GetComponentsInChildren<TTileInfo>()).ToArray();
            var data = new Tile[tiles.Length];
            for (int i = 0; i < data.Length; i++)
            {
                var tile = tiles[i];
                data[i] = new Tile(name: tile.name,
                    topLeft: new Coordinate(tile.TopLeftLatitude, tile.TopLeftLongitude),
                    bottomRight: new Coordinate(tile.BottomRightLatitude, tile.BottomRightLongitude));
            }
            string newTerrainDirectory = tiles.First().GetComponent<Terrain>().terrainData.MicroVerseTerrainDataPath();
            if (!Directory.Exists(newTerrainDirectory))
                Directory.CreateDirectory(newTerrainDirectory);
            string filePath = Path.Combine(newTerrainDirectory, nameof(Tile) + ".json");
            File.WriteAllText(filePath, JsonConvert.SerializeObject(data));
            AssetDatabase.Refresh();
        }
#endif

        /// <summary>
        /// Converts TerrainData to Heightmaps and saves in a parallel location.
        /// </summary>
        internal static void ConvertTerrainDataToHeightmap()
        {
            Debug.Log("Converting Terrain Data to Heightmap...");
            var terrains = Selection.gameObjects.SelectMany(go => go.GetComponentsInChildren<Terrain>()).ToArray();
            var terrainsData = terrains.Select(t => t.terrainData).ToArray();
            if (terrainsData.Length == 0 || terrainsData.Length == 0)
            {
                Debug.LogError($"Select Objects with '{nameof(Terrain)}' component!");
                return;
            }

            var terrainDirectory = Path.GetDirectoryName(AssetDatabase.GetAssetPath(terrainsData[0]));
            var heightmapDirectory = Path.Combine(Path.GetDirectoryName(terrainDirectory), nameof(HeightStamp));
            if (Directory.Exists(heightmapDirectory))
            {
                Debug.Log($"Heightmaps are already extracted at {heightmapDirectory}");
                return;
            }

            // Collect heights from all terrains
            List<float[,]> allHeights = new List<float[,]>();
            foreach (var terrainData in terrainsData)
            {
                int heightmapResolution = terrainData.heightmapResolution;
                float[,] heights = terrainData.GetHeights(0, 0, heightmapResolution, heightmapResolution);
                allHeights.Add(heights);
            }

            // Calculate overall minHeight and maxHeight
            float minHeight = float.MaxValue;
            float maxHeight = float.MinValue;
            foreach (var heights in allHeights)
            {
                float minFloat = heights.Cast<float>().Min();
                float maxFloat = heights.Cast<float>().Max();
                minHeight = Mathf.Min(minHeight, minFloat);
                maxHeight = Mathf.Max(maxHeight, maxFloat);
            }

            var assetPaths = new string[terrainsData.Length];
            int resolution = 2049;
            for (int i = 0; i < terrainsData.Length; i++)
            {
                TerrainData terrainData = terrainsData[i];

                string exportFileName = terrainData.name + ".tif";
                var path = Path.Combine(heightmapDirectory, exportFileName);
                assetPaths[i] = path;
                if (File.Exists(path))
                    continue;

                // Create the Heightmaps directory if it doesn't exist
                if (!Directory.Exists(heightmapDirectory))
                    Directory.CreateDirectory(heightmapDirectory);

                int heightmapResolution = terrainData.heightmapResolution;

                float[,] heights = terrainData.GetHeights(0, 0, heightmapResolution, heightmapResolution);

                // Normalize heights to fit within 0-1 range
                float heightRange = maxHeight - minHeight;
                for (int y = 0; y < heights.GetLength(0); y++)
                    for (int x = 0; x < heights.GetLength(1); x++)
                        heights[y, x] = (heights[y, x] - minHeight) / heightRange;

                // Create a 16-bit heightmap array
                ushort[] heightmapData = new ushort[heightmapResolution * heightmapResolution];

                // Convert heights to 16-bit format
                for (int y = 0; y < heightmapResolution; y++)
                    for (int x = 0; x < heightmapResolution; x++)
                        heightmapData[y * heightmapResolution + x] = (ushort)(heights[y, x] * ushort.MaxValue);

                if (i == 0)
                    resolution = (int)Mathf.Sqrt(heightmapData.Length);
                SaveHeightmapToFile(heightmapData, path);
            }
            AssetDatabase.Refresh();

            foreach (var asset in assetPaths)
            {
                TextureImporter importer = AssetImporter.GetAtPath(asset) as TextureImporter;
                var defaultPlatform = importer.GetDefaultPlatformTextureSettings();
                importer.sRGBTexture = false;
                importer.wrapMode = TextureWrapMode.Clamp;
                defaultPlatform.maxTextureSize = resolution - 1;
                defaultPlatform.format = TextureImporterFormat.R16;
                importer.SetPlatformTextureSettings(defaultPlatform);
            }
            AssetDatabase.Refresh();
        }

        internal static void SaveHeightmapToFile(ushort[] heightmapData, string filePath)
        {
            // Create a Texture2D from heightmap data
            int resolution = (int)Mathf.Sqrt(heightmapData.Length);
            Texture2D texture = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false);
            Color32[] colors = new Color32[heightmapData.Length];
            for (int i = 0; i < heightmapData.Length; i++)
                colors[i] = new Color32((byte)(heightmapData[i] >> 8), (byte)(heightmapData[i] & 0xFF), 0, 255);
            texture.SetPixels32(colors);
            // Encode the texture to a TIFF file
            byte[] tiffBytes = texture.EncodeToTGA();
            File.WriteAllBytes(filePath, tiffBytes);
            Debug.Log($"Saved Heightmap: {filePath}");
            UnityEngine.Object.DestroyImmediate(texture);
        }

        internal static void DuplicateDirectory(string sourceDirPath, string destinationDirPath)
        {
            if (!Directory.Exists(destinationDirPath))
                Directory.CreateDirectory(destinationDirPath);

            // Get the files in the source directory
            string[] files = Directory.GetFiles(sourceDirPath);

            // Copy each file to the destination directory
            foreach (string file in files)
            {
                string fileName = Path.GetFileName(file);
                string destinationFilePath = Path.Combine(destinationDirPath, fileName);
                File.Copy(file, destinationFilePath, false);
            }

            // Get the subdirectories in the source directory
            string[] subDirectories = Directory.GetDirectories(sourceDirPath);

            // Recursively duplicate each subdirectory
            foreach (string subDirectory in subDirectories)
            {
                string directoryName = Path.GetFileName(subDirectory);
                string destinationSubDirPath = Path.Combine(destinationDirPath, directoryName);
                DuplicateDirectory(subDirectory, destinationSubDirPath);
            }
        }

        #endregion

        #region MicroSplat

        /// <summary>
        /// Select terrain or terrain parent GameObjects in Scene and it will set the texture
        /// (located in a parallel location) as the Tint Map for MicroSplat Global Texturing.
        /// </summary>
        [MenuItem(nameof(MicroWorld) + "/Set Tint Texture To MicroSplat Terrain", priority = 200)]
        internal static void SetTintTextureToMicroSplatTerrain()
        {
            var terrains = Selection.gameObjects.SelectMany(go => go.GetComponentsInChildren<MicroSplatTerrain>()).ToArray();
            if (terrains.Length == 0)
            {
                Debug.LogError($"Select Objects with '{nameof(MicroSplatTerrain)}' component!");
                return;
            }

            foreach (var msTerrain in terrains)
            {
                var terrainData = msTerrain.terrain.terrainData;
                if (terrainData == null)
                {
                    Debug.LogError($"{msTerrain.name} has invalid Terrain Data");
                    continue;
                }
                var textureAbsolutePath = $"{Path.Combine(Directory.GetParent(Directory.GetParent(AssetDatabase.GetAssetPath(terrainData)).FullName).FullName, nameof(Texture), $"{terrainData.name.Replace("D", "T")}.jpg")}";
                msTerrain.tintMapOverride = AssetDatabase.LoadAssetAtPath<Texture2D>(textureAbsolutePath.Substring(textureAbsolutePath.IndexOf("Assets")));
            }

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        #endregion

        #region Elements

        /// <summary>
        /// Select 1 <see cref="OSMExtractor"/> asset and 1 <see cref="MicroWorldArea"/> asset from the Project,
        /// and it will set them up in the scene.
        /// </summary>
        [MenuItem(nameof(MicroWorld) + "/Setup Elements", priority = 300)]
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
                Path.Combine(GameObject.FindFirstObjectByType<Terrain>(FindObjectsInactive.Include).terrainData.MicroVerseTerrainDataPath(), nameof(Tile) + ".json")));
            foreach (var element in elements.ToWorldPoints(tiles))
            {
                var splineContainer = (PrefabUtility.InstantiatePrefab(prefab, parent: elementsParent) as GameObject)
                    .GetComponent<SplineContainer>();
                var spline = new Spline(element.ToKnots());
                spline.SetTangentMode(TangentMode.Linear);
                spline.Closed = splineContainer.Spline.Closed;
                splineContainer.Spline = spline;
                Utilities.SnapSplineToTerrain(ref splineContainer);
            }
        }

        [MenuItem(nameof(MicroWorld) + "/Center Spline Pivot", priority = 302)]
        internal static void CenterSplinePivot()
        {
            foreach (var splineContainer in Selection.gameObjects.Where(go => go.GetComponent<SplineContainer>())
                .Select(go => go.GetComponent<SplineContainer>()).ToArray())
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

        [MenuItem(nameof(MicroWorld) + "/Filter Elements by Spline", priority = 301)]
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
                Debug.LogError($"Select 1 {nameof(SplineContainer)} to use as filter: {e}");
                return;
            }
            // Area splines
            var areas = Selection.gameObjects.Where(go => go.GetComponent<MicroWorldArea>())
                .Select(go => go.GetComponent<SplineContainer>()).ToArray();
            if (areas.Length < 1)
            {
                Debug.LogError($"Select at least 1 {nameof(MicroWorldArea)} to filter!");
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

        #endregion

        #region Vegetation

        [MenuItem(nameof(MicroWorld) + "/Apply Foliage Renderer Settings", priority = 400)]
        static void ApplyFoliageRendererSettings()
        {
            if (!IndirectRenderer.hasInstance && !TerrainFoliageRenderer.hasInstance)
            {
                Debug.LogError($"Create Foliage Renderer first!");
                return;
            }
            // Setup Indirect Renderer
            IndirectRenderer.instance.cullingCamera = Camera.main;
            IndirectRenderer.instance.hiZOcclusion = IndirectRenderer.ModeToggle.On;

            TerrainFoliageRenderer settings = default;
            try
            {
                settings = Array.FindAll(Selection.objects, obj => obj is GameObject)
                    .Select(obj => obj as GameObject).ToArray()[0].GetComponent<TerrainFoliageRenderer>();
            }
            catch (Exception e)
            {
                Debug.LogError($"Select one prefab with {nameof(TerrainFoliageRenderer)} component: " + e.Message);
            }

            var instance = TerrainFoliageRenderer.instance;
            instance.treeOptions = settings.treeOptions;
            instance.detailOptions = settings.detailOptions;

            // Immediately refreshes all tree and detail instances (regardless of distance)
            foreach (var tfp in GameObject.FindObjectsOfType<TerrainFoliageProvider>())
            {
                tfp.forceRefreshOnEnable = true;
                // Match terrain settings to Foliage Renderer
                tfp.Terrain.treeDistance = settings.treeOptions.maxDrawDistance;
                tfp.Terrain.detailObjectDistance = settings.detailOptions.maxDrawDistance;
                tfp.Terrain.detailObjectDensity = settings.detailOptions.density;
            }
        }

        #endregion
    }
}
