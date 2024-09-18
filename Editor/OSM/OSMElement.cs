using UnityEngine;

namespace Cuku.MicroWorld
{
    [CreateAssetMenu(menuName = nameof(MicroWorld) + "/" + nameof(OSMElement))]
    public class OSMElement : ScriptableObject
    {
        public string Key;
        public string Value;
    }
}