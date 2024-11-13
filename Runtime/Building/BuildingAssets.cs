#if UNITY_EDITOR
using UnityEngine;

namespace Cuku.MicroWorld
{
    [CreateAssetMenu(menuName = nameof(MicroWorld) + "/" + nameof(BuildingAssets))]
    public class BuildingAssets : ScriptableObject
    {
        public Material Facade;
        public Material Window;
        public Material Roof;
    }
}
#endif