using UnityEngine;
using System;
using Unity.Mathematics;
using System.IO;
using OsmSharp.Complete;
using OsmSharp.Streams;
using System.Linq;
using OsmSharp;
using System.Collections.Generic;
using UnityEditor;

namespace Cuku.MicroWorld
{
    public static class OSM
    {
        public static Coordinate[][] Extract(this OSMElement[] elements, OSMSource source)
        {
            using (var fileStream = File.OpenRead(source.Data))
            {
                var box = source.ToBoundingBox();
                var stream = new PBFOsmStreamSource(fileStream).FilterBox(box.x, box.y, box.z, box.w);
                // Extract elemenst
                var osmGeos = (from osmGeo in stream
                               from element in elements
                               where osmGeo.Type == OsmGeoType.Node ||
                                     (osmGeo.Type == OsmGeoType.Way && osmGeo.Tags != null && osmGeo.Tags.Contains(element.Key, element.Value)) ||
                                     (osmGeo.Type == OsmGeoType.Relation && osmGeo.Tags != null && osmGeo.Tags.Contains(element.Key, element.Value))
                               select osmGeo).ToList();
                // Extract outer way members
                var outerWayMembers = osmGeos.Where(osmGeo => osmGeo.Type == OsmGeoType.Relation)
                    .SelectMany(osmGeo => (osmGeo as Relation)?.Members
                    .Where(member => member.Type == OsmGeoType.Way && member.Role == "outer")
                    .Select(member => member.Id)).ToList();
                var outerWays = (from osmGeo in stream
                                 where osmGeo.Type == OsmGeoType.Way && outerWayMembers.Contains(osmGeo.Id.Value)
                                 select osmGeo).ToList();
                osmGeos.AddRange(outerWays);
                // Extract ways
                var completes = osmGeos.ToComplete();
                var ways = from osmGeo in completes
                           where osmGeo.Type == OsmGeoType.Way
                           select osmGeo;
                var completeWays = ways.Cast<CompleteWay>();
                // Extract way nodes
                var geoElements = new List<Node[]>();
                foreach (CompleteWay way in completeWays)
                    geoElements.Add(way.Nodes);
                return geoElements.GetCoordinates();
            }
        }

        internal static Coordinate[][] GetCoordinates(this List<Node[]> elements)
        {
            var coordinates = new Coordinate[elements.Count][];
            for (int i = 0; i < elements.Count; i++)
            {
                var points = elements[i];
                coordinates[i] = new Coordinate[points.Length];
                for (int j = 0; j < points.Length; j++)
                {
                    var point = points[j];
                    coordinates[i][j] = new Coordinate(point.Latitude.Value, point.Longitude.Value);
                }
            }
            return coordinates;
        }

        internal static float4 ToBoundingBox(this OSMSource source)
        {
            var centerLat = source.CenterCoordinates.Lat;
            var centerLon = source.CenterCoordinates.Lon;
            var size = source.Area / 2.0f;
            // Convert size from kilometers to degrees (approximation)
            float deltaLatDegrees = size.y / 111.32f; // 1 degree of latitude is approximately 111.32 km
            float deltaLonDegrees = size.x / (111.32f * (float)Math.Cos(centerLat * Math.PI / 180.0f)); // Adjust for latitude
            return new float4((float)centerLon - deltaLonDegrees,
                (float)centerLat + deltaLatDegrees,
                (float)centerLon + deltaLonDegrees,
                (float)centerLat - deltaLatDegrees);
        }

        internal static float3[][] ToWorldPoints(this Coordinate[][] element, ref Tile[] tiles)
        {
            var worldElement = new float3[element.Length][];
            for (int i = 0; i < element.Length; i++)
            {
                var points = element[i];
                worldElement[i] = new float3[points.Length];
                for (int j = 0; j < points.Length; j++)
                    worldElement[i][j] = points[j].ToTerrainPosition(ref tiles);
            }
            return worldElement;
        }

        internal static float3 ToTerrainPosition(this Coordinate coordinate, ref Tile[] tiles)
        {
            (Tile tile, Terrain terrain) = coordinate.FindTileTerrainPair(ref tiles);
            var minLat = tile.BottomRight.Lat;
            var maxLat = tile.TopLeft.Lat;
            var minLon = tile.TopLeft.Lon;
            var maxLon = tile.BottomRight.Lon;
            // Calculate relative position with respect to the terrain's bounding box
            var relativeX = (coordinate.Lon - minLon) / (maxLon - minLon);
            var relativeZ = (coordinate.Lat - minLat) / (maxLat - minLat);
            // Convert to Unity terrain position
            var terrainSize = terrain.terrainData.size;
            var terrainPosition = terrain.transform.position;
            var posX = relativeX * terrainSize.x + terrainPosition.x;
            var posZ = relativeZ * terrainSize.z + terrainPosition.z;
            // Y-position based on actual terrain height
            double posY = terrain.SampleHeight(new Vector3((float)posX, 0, (float)posZ)) + terrainPosition.y;
            return new float3((float)posX, (float)posY, (float)posZ);
        }

        internal static (Tile tile, Terrain terrain) FindTileTerrainPair(this Coordinate coordinate, ref Tile[] tiles)
        {
            foreach (var tile in tiles)
            {
                if (coordinate.Lat <= tile.TopLeft.Lat &&
                    coordinate.Lat >= tile.BottomRight.Lat &&
                    coordinate.Lon >= tile.TopLeft.Lon &&
                    coordinate.Lon <= tile.BottomRight.Lon)
                    return (tile,
                        GameObject.FindObjectsByType<Terrain>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                        .FirstOrDefault(terrain => terrain.name.Contains(tile.Name)));
            }
            return default;
        }
    }
}