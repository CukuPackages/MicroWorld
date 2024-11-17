using System.Linq;
using UnityEngine;

namespace Cuku.MicroWorld
{
    public static class BuildingExtensions
    {
        public static BuildingFilter GlobalFilter()
            => GameObject.FindObjectsByType<BuildingFilter>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                    .First(filter => filter.IsGlobal);

        public static float WorldScale => GlobalFilter().Properties.WorldScale;

        public static float FloorHeight(this Building building)
        {
            if (building.Properties.OverrideFloorHeight)
                return WorldScale * Random.Range(building.Properties.FloorHeight.x, building.Properties.FloorHeight.y);
            else if (building.Filter && building.Filter.Properties.OverrideFloorHeight)
                return WorldScale * Random.Range(building.Filter.Properties.FloorHeight.x, building.Filter.Properties.FloorHeight.y);
            return WorldScale * Random.Range(GlobalFilter().Properties.FloorHeight.x, GlobalFilter().Properties.FloorHeight.y);
        }

        public static int FloorCount(this Building building)
        {
            if (building.Properties.OverrideFloorCount)
                return Random.Range(building.Properties.FloorCount.x, building.Properties.FloorCount.y);
            else if (building.Filter && building.Filter.Properties.OverrideFloorCount)
                return Random.Range(building.Filter.Properties.FloorCount.x, building.Filter.Properties.FloorCount.y);
            return Random.Range(GlobalFilter().Properties.FloorCount.x, GlobalFilter().Properties.FloorCount.y);
        }

        public static Opening Door(this Building building)
        {
            if (building.Properties.OverrideDoor)
                return building.Properties.Door[Random.Range(0, building.Properties.Door.Length)];
            else if (building.Filter && building.Filter.Properties.OverrideDoor)
                return building.Filter.Properties.Door[Random.Range(0, building.Filter.Properties.Door.Length)];
            return GlobalFilter().Properties.Door[Random.Range(0, GlobalFilter().Properties.Door.Length)];
        }

        public static Opening Window(this Building building)
        {
            if (building.Properties.OverrideWindow)
            {
                var window = building.Properties.Window;
                return (window != null && window.Length > 0) ? window[Random.Range(0, window.Length)] : null;
            }
            else if (building.Filter && building.Filter.Properties.OverrideWindow)
            {
                var window = building.Filter.Properties.Window;
                return (window != null && window.Length > 0) ? window[Random.Range(0, window.Length)] : null;
            }

            var globalWindow = GlobalFilter().Properties.Window;
            return (globalWindow != null && globalWindow.Length > 0) ? globalWindow[Random.Range(0, globalWindow.Length)] : null;
        }

        public static RoofType RoofType(this Building building)
        {
            if (building.Properties.OverrideRoofType)
                return building.Properties.RoofType[Random.Range(0, building.Properties.RoofType.Length)];
            else if (building.Filter && building.Filter.Properties.OverrideRoofType)
                return building.Filter.Properties.RoofType[Random.Range(0, building.Filter.Properties.RoofType.Length)];
            return GlobalFilter().Properties.RoofType[Random.Range(0, GlobalFilter().Properties.RoofType.Length)];
        }

        public static Material FacadeMaterial(this Building building)
        {
            if (building.Properties.OverrideFacadeMaterial)
                return building.Properties.FacadeMaterial[Random.Range(0, building.Properties.FacadeMaterial.Length)];
            else if (building.Filter && building.Filter.Properties.OverrideFacadeMaterial)
                return building.Filter.Properties.FacadeMaterial[Random.Range(0, building.Filter.Properties.FacadeMaterial.Length)];
            return GlobalFilter().Properties.FacadeMaterial[Random.Range(0, GlobalFilter().Properties.FacadeMaterial.Length)];
        }

        public static Material WindowMaterial(this Building building)
        {
            if (building.Properties.OverrideOpeningMaterial)
                return building.Properties.OpeningMaterial[Random.Range(0, building.Properties.OpeningMaterial.Length)];
            else if (building.Filter && building.Filter.Properties.OverrideOpeningMaterial)
                return building.Filter.Properties.OpeningMaterial[Random.Range(0, building.Filter.Properties.OpeningMaterial.Length)];
            return GlobalFilter().Properties.OpeningMaterial[Random.Range(0, GlobalFilter().Properties.OpeningMaterial.Length)];
        }

        public static Material RoofMaterial(this Building building)
        {
            if (building.Properties.OverrideRoofMaterial)
                return building.Properties.RoofMaterial[Random.Range(0, building.Properties.RoofMaterial.Length)];
            else if (building.Filter && building.Filter.Properties.OverrideRoofMaterial)
                return building.Filter.Properties.RoofMaterial[Random.Range(0, building.Filter.Properties.RoofMaterial.Length)];
            return GlobalFilter().Properties.RoofMaterial[Random.Range(0, GlobalFilter().Properties.RoofMaterial.Length)];
        }
    }
}