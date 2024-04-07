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
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;
using UnityEditor;

namespace Cuku.MicroWorld
{
    public static class OSM
    {
        // Coordinate System use by OSM
        static GeographicCoordinateSystem osmCoordinateSystem = GeographicCoordinateSystem.WGS84;

        public static LatLon[][] ExtractElementsPoints(this Element[] elements, Source source)
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
                return elementsNodes.GetLatLon();
            }
        }

        internal static FileStream LoadData(this Source source)
            => File.OpenRead(Path.Combine(Application.streamingAssetsPath, source.Data));

        internal static float4 ToBox(this Source source)
        {
            var centerLat = source.Lat;
            var centerLon = source.Lon;
            var size = source.Size / 2.0f;
            // Convert size from kilometers to degrees (approximation)
            float deltaLatDegrees = size.y / 111.32f; // 1 degree of latitude is approximately 111.32 km
            float deltaLonDegrees = size.x / (111.32f * (float)Math.Cos(centerLat * Math.PI / 180.0f)); // Adjust for latitude
            return new float4((float)centerLon - deltaLonDegrees,
                (float)centerLat + deltaLatDegrees,
                (float)centerLon + deltaLonDegrees,
                (float)centerLat - deltaLatDegrees);
        }

        internal static LatLon[][] GetLatLon(this List<Node[]> elementsNodes)
        {
            var elementsPoints = new LatLon[elementsNodes.Count][];
            for (int i = 0; i < elementsNodes.Count; i++)
            {
                var elementNodes = elementsNodes[i];
                elementsPoints[i] = new LatLon[elementNodes.Length];
                for (int j = 0; j < elementNodes.Length; j++)
                {
                    var node = elementNodes[j];
                    elementsPoints[i][j] = new LatLon(node.Latitude.Value, node.Longitude.Value);
                }
            }
            return elementsPoints;
        }

        internal static float3[][] ToWorldPoints(this LatLon[][] latLons, Source source)
        {
            var origin = (new double[] { source.Lat, source.Lon }).Transform(source.CoordinateSystem);
            var shift = source.TerrainShift;
            var scale = source.CoordinatesScale;
            //var origin = new double[] { source.Lat, source.Lon };

            var elementsPoints = new float3[latLons.Length][];
            for (int i = 0; i < latLons.Length; i++)
            {
                var elementNodes = latLons[i];
                elementsPoints[i] = new float3[elementNodes.Length];
                for (int j = 0; j < elementNodes.Length; j++)
                {
                    var node = elementNodes[j];
                    var normalizedPoint = (new double[] { node.Lat, node.Lon }).Transform(source.CoordinateSystem);
                    //var normalizedPoint = new double[] { node.Lat, node.Lon };
                    var point = new double[]
                    {
                        normalizedPoint[0] - origin[0],
                        normalizedPoint[1] - origin[1]
                    };
                    elementsPoints[i][j] = new float3((float)point[1] * scale.x + shift.x, 0, (float)point[0] * scale.y + shift.y);
                    //elementsPoints[i][j] = new float3((float)point[1], 0, (float)point[0]) * 100000;
                }
            }
            return elementsPoints;
        }

        internal static double[] Transform(this double[] latLon, string coordinateSystem)
            => new CoordinateTransformationFactory()
            .CreateFromCoordinateSystems(osmCoordinateSystem, ProjectedCoordinateSystem.WebMercator)
            //.CreateFromCoordinateSystems(osmCoordinateSystem, new CoordinateSystemFactory().CreateFromWkt(coordinateSystem))
            .MathTransform.Transform(latLon);

        internal static BezierKnot[] ToKnots(this float3[] points)
        {
            var bezierKnots = new BezierKnot[points.Length];
            for (int i = 0; i < points.Length; i++)
                bezierKnots[i] = new BezierKnot(points[i]);
            return bezierKnots;
        }
    }
}