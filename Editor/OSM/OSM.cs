using UnityEngine;
using System;
using Unity.Mathematics;
using System.IO;
using OsmSharp.Complete;
using OsmSharp.Streams;
using System.Linq;
using OsmSharp;
using System.Collections.Generic;
using UnityEngine.Splines;
using UnityEditor;

namespace Cuku.MicroWorld
{
    public static class OSM
    {
        public static Coordinate[][] ExtractElementsPoints(this Element[] elements, Source source)
        {
            using (var fileStream = source.LoadData())
            {
                var box = source.ToBox();
                var streamSource = new PBFOsmStreamSource(fileStream).FilterBox(box.x, box.y, box.z, box.w);

                var elementsNodes = new List<Node[]>();
                foreach (var element in elements)
                {
                    var filtered = from osmGeo in streamSource
                                   where osmGeo.Type == OsmGeoType.Node ||
                                        (osmGeo.Type == OsmGeoType.Way && osmGeo.Tags != null && osmGeo.Tags.Contains(element.Key, element.Value))
                                   select osmGeo;

                    var completes = filtered.ToComplete();
                    var ways = from osmGeo in completes
                               where osmGeo.Type == OsmGeoType.Way
                               select osmGeo;

                    IEnumerable<CompleteWay> completeWays = ways.Cast<CompleteWay>();
                    foreach (CompleteWay way in completeWays)
                        elementsNodes.Add(way.Nodes);
                }
                return elementsNodes.GetCoordinates();
            }
        }

        internal static FileStream LoadData(this Source source)
            => File.OpenRead(Path.Combine(Application.streamingAssetsPath, source.Data));

        internal static float4 ToBox(this Source source)
        {
            var centerLat = source.CenterCoordinates.Lat;
            var centerLon = source.CenterCoordinates.Lon;
            var size = source.Size / 2.0f;
            // Convert size from kilometers to degrees (approximation)
            float deltaLatDegrees = size.y / 111.32f; // 1 degree of latitude is approximately 111.32 km
            float deltaLonDegrees = size.x / (111.32f * (float)Math.Cos(centerLat * Math.PI / 180.0f)); // Adjust for latitude
            return new float4((float)centerLon - deltaLonDegrees,
                (float)centerLat + deltaLatDegrees,
                (float)centerLon + deltaLonDegrees,
                (float)centerLat - deltaLatDegrees);
        }

        internal static Coordinate[][] GetCoordinates(this List<Node[]> elementsNodes)
        {
            var elementsPoints = new Coordinate[elementsNodes.Count][];
            for (int i = 0; i < elementsNodes.Count; i++)
            {
                var elementNodes = elementsNodes[i];
                elementsPoints[i] = new Coordinate[elementNodes.Length];
                for (int j = 0; j < elementNodes.Length; j++)
                {
                    var node = elementNodes[j];
                    elementsPoints[i][j] = new Coordinate(node.Latitude.Value, node.Longitude.Value);
                }
            }
            return elementsPoints;
        }

        internal static float3[][] ToWorldPoints(this Coordinate[][] elements, Tile[] tiles)
        {
            var worldElement = new float3[elements.Length][];
            for (int i = 0; i < elements.Length; i++)
            {
                var elementPoints = elements[i];
                worldElement[i] = new float3[elementPoints.Length];
                for (int j = 0; j < elementPoints.Length; j++)
                    worldElement[i][j] = elementPoints[j].ToTerrainPosition(tiles);
            }
            return worldElement;
        }

        static Vector3 ToTerrainPosition(this Coordinate coordinate, Tile[] tiles)
        {
            (Tile tile, Terrain terrain) = coordinate.FindTileTerrainPair(tiles);
            double minLat = tile.BottomRight.Lat;
            double maxLat = tile.TopLeft.Lat;
            double minLon = tile.TopLeft.Lon;
            double maxLon = tile.BottomRight.Lon;
            // Calculate relative position with respect to the terrain's bounding box
            double relativeX = (coordinate.Lon - minLon) / (maxLon - minLon);
            double relativeZ = (coordinate.Lat - minLat) / (maxLat - minLat);
            // Convert to Unity terrain position
            Vector3 terrainSize = terrain.terrainData.size;
            Vector3 terrainPosition = terrain.transform.position;
            double posX = relativeX * terrainSize.x + terrainPosition.x;
            double posZ = relativeZ * terrainSize.z + terrainPosition.z;
            // Y-position based on actual terrain height
            double posY = terrain.SampleHeight(new Vector3((float)posX, 0, (float)posZ)) + terrainPosition.y;
            return new Vector3((float)posX, (float)posY, (float)posZ);
        }

        internal static (Tile tile, Terrain terrain) FindTileTerrainPair(this Coordinate coordinate, Tile[] tiles)
        {
            foreach (var tile in tiles)
            {
                if (coordinate.Lat <= tile.TopLeft.Lat &&
                    coordinate.Lat >= tile.BottomRight.Lat &&
                    coordinate.Lon >= tile.TopLeft.Lon &&
                    coordinate.Lon <= tile.BottomRight.Lon)
                    return (tile,
                        GameObject.FindObjectsByType<Terrain>(FindObjectsSortMode.None)
                        .FirstOrDefault(terrain => terrain.name.Contains(tile.Name)));
            }
            return default;
        }

        internal static BezierKnot[] ToKnots(this float3[] points)
        {
            var bezierKnots = new BezierKnot[points.Length];
            for (int i = 0; i < points.Length; i++)
                bezierKnots[i] = new BezierKnot(points[i]);
            return bezierKnots;
        }


    }
}