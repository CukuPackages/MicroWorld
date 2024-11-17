#if UNITY_EDITOR
using UnityEngine;

namespace Cuku.MicroWorld
{
    public class BuildingProperties : MonoBehaviour
    {
        [SerializeField] public float WorldScale;
        [SerializeField] public float WindowOpeningWidth;

        [SerializeField] public bool OverrideFloorHeight;
        [SerializeField] public Vector2 FloorHeight;

        [SerializeField] public bool OverrideFloorCount;
        [SerializeField] public Vector2Int FloorCount;

        [SerializeField] public bool OverrideDoor;
        [SerializeField] public Opening[] Door;

        [SerializeField] public bool OverrideWindow;
        [SerializeField] public Opening[] Window;

        [SerializeField] public bool OverrideRoofType;
        [SerializeField] public RoofType[] RoofType;

        [SerializeField] public bool OverrideFacadeMaterial;
        [SerializeField] public Material[] FacadeMaterial;

        [SerializeField] public bool OverrideOpeningMaterial;
        [SerializeField] public Material[] OpeningMaterial;

        [SerializeField] public bool OverrideRoofMaterial;
        [SerializeField] public Material[] RoofMaterial;
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