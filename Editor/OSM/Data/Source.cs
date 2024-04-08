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
        [Tooltip("Center Latitude and Longitude to search the data from.")]
        public Coordinate CenterCoordinates;

        [SerializeField]
        [Tooltip("Bounding box around the " + nameof(CenterCoordinates) + " size in Km.")]
        public float2 Size = new float2(1.0f, 1.0f);
    }
}