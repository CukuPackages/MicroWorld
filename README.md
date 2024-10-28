## Installation
1. Open Git Bash / Terminal at the project root
2. Add submodule
   ```
   git submodule add https://github.com/CukuPackages/Cuku.MicroWorld.git Packages/com.cuku.microworld
   ```

## Dependencies
MicroVerse

MicroSplat

Foliage Renderer

[OsmSharp](https://www.nuget.org/packages/OsmSharp/)

[OsmSharp.Geo](https://www.nuget.org/packages/OsmSharp.Geo/)

## Terrain Data
Use TerraLand to download terrain data and satellite images:

Create main area terrain with high resolution height and texture maps:
- Area Size: 20 km
- Tile Grid: 5
- Heightmap Res: 1024 (adapt so that is not jagged)
- Texture Res: 8192
- Resample Mode: Down
- World Scale: 0.1 or less (for large areas)
- Draw Instanced: False
- Pixel Error: 1
- Splatmap Res: 4096

Create surrounding terrain with enough low resolution height and texture maps:
- Area Size: 200 km
- Tile Grid: 2
- Heightmap Res: 128
- Texture Res: 8192
- Resample Mode: Down
- World Scale: 0.1 (for large areas)
- Draw Instanced: False
- Pixel Error: 1
- Splatmap Res: 4096

## Setup Main Area
- Move the generated Data folder from TPrefabs - InSceneData to the project Assets
- Select terrains parent in Hierarchy
- MicroWorld - Convert Terain to MicroVerse
- HeightStamp textures should be Red (if they're green, restart Unity to refresh the HeightStamps import)
- Adjust HeightStamps Scale.y so that it roughly matches the original terrain

### Setup MicroVerse
1. Select MicroVerse in Hierarchy
2. Adjust HeightMap Resolution if required (also adjust HeightStamp textures resolution)
3. Adjust AlphaMap Resolution if required
4. Detail Resolution: 512
5. Enable Terrain Culling: MicroVerse - Options - Settings - Use Scene Culling
6. MicroVerse - Pixel Error: 1
7. After MicroVerse is setup, remove TerrainData and Layer folders of the original terrain

#### Fix Stamps Prefab Overrides
When instantiating content prefabs from script, e.g. from MVBiome, MicroVerse calls RevisionAllStamps in MicroVerse.cs.
This will set some Stamps properties, which break the direct relation to the source content prefab and so it's desired.
To avoid this, comment out the only call to RevisionAllStamps in MicroVerse.cs.

### Setup MicroSplat
1. Select MicroVerse in Hierarchy
2. Select Convert to MicroSplat

### Setup MicroVerse Preview
1. Select MicroSplat material
2. Features - MicroVersePreview
3. Check Export MicroVerse Preview
4. Select MicroVerse
5. Proxy Renderer Mode: Proxy While Updating / Always Proxy

### Setup Global Texturing
1. Select MicroSplat material
2. Features - Global Texturing
3. Global Tint: Multiply 2X
4. Sampling Mode: Linear

### Apply Tint Maps
1. Select Terrains Parent under MicroVerse
2. MicroWorld - Set Tint Texture To MicroSplat Terrain

### Setup Procedural Texturing
Use Procedural Texturing to immediately texture the terrain based on height, slope, and noise functions:
1. Select MicroSplat material
2. Features
3. Procedural Texturing
4. Check Procedural Texturing

Disable Texturing from MicroVerse:
1. Select all CopyPaste Stamp objects
2. Locate Copy Paste Stamp component
3. Uncheck Apply Texturing

Set World Height Range:
1. Select MicroSplat material
2. Settings
3. Procedural Texture
4. World Height Range: X: 0, Y: terrain heighest point (Tip: use the reference cube)

## Setup Surrounding Area
Convert Surrounding Area terrain into mesh terrain:
1. Create new scene
2. Setup Terrain as above
3. Add HeightStamps to better blend between main and surrounding area (after main HeightStamp)
4. E.g. create HeightStamp same size as MainArea, Mode: Override, Falloff Type: Range
5. Create other HeightStamps to better blend on the edges
6. Apply similar global texturing settings as the main area
7. Convert terrains to mesh with default settings
8. Lower splat maps resolution to the minimum
9. Delete terrain dataterrain data

## Vegetation
### Setup Foliage Renderer
1. Window - FoliageRenderer - Validate Culling Shader
2. GameObject - Create Foliage Renderer
3. Select Foliage Renderer game object - Shader Patcher - Scan - Patch
4. Add Indirect Camera component to the Camera

> [!NOTE]
> If you are using TVE - The Visual Engine, set the TVE ShaderManager to FoliageRenderer Mode.

### Optimization
1. Foliage Renderer - Uncheck Auto Refresh Terrains
2. Window - FoliageRenderer - Disable Force Scene View Refresh
3. Indirect Renderer - Enable Hi Z Occlusion if it helps
4. Indirect Renderer - Increase Max Graphics Buffer Caches

### Adjust Shadows
1. HDRP Global Settings - Max Shadow Distance
2. Main Directional Light - Shadows - Resolution
3. Adjust Shadow Distance for Trees and Details in Terrain Foliage Renderer

### Setup Biome
1. Keep a separate scene with only Vegetation Splines (must not go in build settings)
2. Locate Features - MicroVerse - MVBiome prefab
3. Drag it in the Scene
4. Make sure the the Spline Container - Spline 0 - Closed is CHECKED
5. Draw a closed spline (check unity Spline Package) that represents the Biome Area
6. Add the Biome Prefab to MVBiome component - Biome
7. Right Click MVBiome component - Spawn
8. The Biome will be created and parented under MicroVerse
9. Adjust it as desired

## OSM - Open Street Maps
OSM data can also be used to extract the vegetation areas, roads, buildings, etc.

### Setup OSM
1. Use NuGet to install OsmSharp and OsmSharp.Geo
2. Install MicroWorld
3. Download .osm.pbf data from Geofabrik and add in [LOCAL] or anywhere on the PC, it will not be added in runtime
4. Create Source asset: Right Click in Assets - Create - MicroWorld - OSMSource
5. OSMSource - Select OSM Data to chose the OSM data .pbf file
6. OSMSource - Center Coordinates: can be found from TerraLand settings when generating the terrain
7. Area: the area where to extract data from (real world size, ignore TerraLand World Scale)
8. Use https://www.openstreetmap.org/ to analyze elements to be extracted
9. If Query features doesn't work, try and click on the feature boundary

#### Setup Vegetation
1. Create Vegetation scene to use it to store vegetation biome areas and make it Active
2. Create vegetation Element assets as defined in OSM (e.g. landuse:forest): Right Click in Assets - Create - OSM - Element
3. Create OSM data Extractor asset: Right Click in Assets - Create - MicroWorld - OSMExtractor
4. Set Source asset (created when setting up OSM)
5. Set vegetation Element assets
6. Select OSMExtractor asset in Inspector - Click ⋮ - Extract (it will create a .json file on the same location)
7. Create MicroWorldVegetation / MicroWorldBiome prefab variant (from MicroWorld - Runtime - MicroWorldArea - Prefabs)
8. MicroWorldVegetation / MicroWorldBiome - Content: use one from Cuku.MicroWorld.Content or create a new one
9. Use models from Library for TreeStamps, Biomes, etc.
10. Select both Extractor and Biome / Vegetation
11. MicroWorld - Setup Elements
12. Select Biome Game Object/s in the scene - Click ⋮ - Spawn

#### Reduce Vegetaion
Vegetation Areas can be reduced / filtered around a spline by a defined range.

> [!WARNING]
> Too Much Content to Spawn?!

If there's just too much content to spawn and not enough RAM, you're doomed, but it's worth trying  the following:
1. Enable MicroVerse - Options - Settings - Use Scene Culling
2. Spawn and save the content in batches
3. Don't focus to Scene and Game tabs
4. When building, don't keep MicroVerse scene open

## TVE and Amplify Impostors Workflow
### Source Prefab
1. Keep only LOD0 and LOD1
2. Keep only LOD0 Game Objects
3. Remove renderers from LOD1
4. Set LOD0 Transition (% Screen Size): 10%
5. Set LOD1 Transition (% Screen Size): 0.1%
6. Remove Ccolliders from Game Objects

### Convert Source Prefab to TVE
1. If a pop-up shows select: Keep Converted Material (to reuse existing materials)
2. Convert Collected Data (green box icon after Convert) and move to Library (Library - Asset Type - Publisher)
3. Create a folder with prefab name on the same level to store settings and specific textures to that prefab

### Impostorize
1. Add Amplify Impostor component to Prefab
2. Click the + button before Bake Impostor button to create new Impostor settings and add to the prefab folder
3. Bake Type: Hemi Octahedron (for top down views)
4. Bake Preset: TVE Hemi Octahedron
5. Add Prefab to scene
6. Click Bake Impostor
7. Drag LOD0 object on newly created Impostor object - Material - Copy Settings From Object
8. Apply prefab overrides
