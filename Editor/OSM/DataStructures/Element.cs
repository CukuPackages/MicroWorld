using UnityEngine;

namespace Cuku.MicroWorld
{
    [CreateAssetMenu(menuName = nameof(OSM) + "/" + nameof(Element))]
    public class Element : ScriptableObject
    {
        public string Key;
        public string Value;
    }
}