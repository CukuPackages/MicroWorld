#if UNITY_EDITOR
using UnityEngine;

namespace Cuku.MicroWorld
{
    public class BuildingProperties : MonoBehaviour
    {
        [SerializeField] public float WorldScale;

        [SerializeField] public bool OverrideFloorHeight;
        [SerializeField] public float FloorHeight;

        [SerializeField] public bool OverrideFloorCount;
        [SerializeField] public int FloorCount;

        [SerializeField] public bool OverrideRoofType;
        [SerializeField] public RoofType RoofType;

        [SerializeField] public bool OverrideFacadeMaterial;
        [SerializeField] public Material FacadeMaterial;

        [SerializeField] public bool OverrideWindowMaterial;
        [SerializeField] public Material WindowMaterial;

        [SerializeField] public bool OverrideFlatRoofMaterial;
        [SerializeField] public Material FlatRoofMaterial;

        [SerializeField] public bool OverrideNonFlatRoofMaterial;
        [SerializeField] public Material NonFlatRoofMaterial;
    }

    /// <summary>
    /// https://wiki.openstreetmap.org/wiki/Key:roof:shape
    /// </summary>
    public enum RoofType
    {
        Flat,
        Pyramidal,
        Hipped,
        Gabled
    }
}
#endif