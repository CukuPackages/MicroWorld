using Unity.Mathematics;
using UnityEngine;

namespace Cuku.MicroWorld
{
    [CreateAssetMenu(menuName = nameof(OSM) + "/" + nameof(Source))]
    public class Source : ScriptableObject
    {
        [SerializeField]
        [Tooltip("\".pbf\" file path relative to StreamingAssets.")]
        public string Data = string.Empty;

        [SerializeField]
        [Tooltip("Center Latitude to search the data from.")]
        public double Lat = 0.0f;

        [SerializeField]
        [Tooltip("Center Longitude to search the data from.")]
        public double Lon = 0.0f;

        [SerializeField]
        [Tooltip("Bounding box size in Km.")]
        public float2 Size = new float2(1.0f, 1.0f);

        [SerializeField]
        [Tooltip("Coordinate System used for the Terrain Data: https://epsg.io")]
        public string CoordinateSystem = string.Empty;

        [SerializeField]
        [Tooltip("If there's any shift from the original terrain data.")]
        public float2 TerrainShift = float2.zero;

        [SerializeField]
        [Tooltip("TODO: figure this out on the coordinate conversion step.")]
        public float2 CoordinatesScale = new float2(1.0f, 1.0f);
    }
}