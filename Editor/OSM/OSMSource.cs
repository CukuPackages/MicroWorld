using Unity.Mathematics;
using UnityEngine;

namespace Cuku.MicroWorld
{
    [CreateAssetMenu(menuName = nameof(MicroWorld) + "/" + nameof(OSMSource))]
    public class OSMSource : ScriptableObject
    {
        [SerializeField]
        public string Data = string.Empty;

        [SerializeField]
        [Tooltip("Center Latitude and Longitude to search the data from.")]
        public Coordinate CenterCoordinates;

        [SerializeField]
        [Tooltip("Bounding box around the " + nameof(CenterCoordinates) + " in Unity Units.")]
        public float2 Area = new float2(1.0f, 1.0f);
    }
}