#if UNITY_EDITOR
using UnityEngine;

namespace Cuku.MicroWorld
{
    [CreateAssetMenu(menuName = nameof(MicroWorld) + "/" + nameof(Opening))]
    public class Opening : ScriptableObject
    {
        public Rect UV;
        [Space] public Vector2 Position;
        [Space] public float Width;
        [Space] public float Depth;
    }
} 
#endif