using System.Linq;
using UnityEngine;

namespace Cuku.MicroWorld
{
    public static class BuildingExtensions
    {
        public static BuildingFilter GlobalFilter()
            => GameObject.FindObjectsByType<BuildingFilter>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                    .First(filter => filter.IsGlobal);

        public static float FloorHeight(this Building building)
        {
            var worldScale = GlobalFilter().Properties.WorldScale;

            if (building.Properties.OverrideFloorHeight)
                return building.Properties.FloorHeight * worldScale;
            else if (building.Filter && building.Filter.Properties.OverrideFloorHeight)
                return building.Filter.Properties.FloorHeight * worldScale;
            return GlobalFilter().Properties.FloorHeight * worldScale;
        }

        public static int FloorCount(this Building building)
        {
            if (building.Properties.OverrideFloorCount)
                return building.Properties.FloorCount;
            else if (building.Filter && building.Filter.Properties.OverrideFloorCount)
                return building.Filter.Properties.FloorCount;
            return GlobalFilter().Properties.FloorCount;
        }

        public static RoofType RoofType(this Building building)
        {
            if (building.Properties.OverrideRoofType)
                return building.Properties.RoofType;
            else if (building.Filter && building.Filter.Properties.OverrideRoofType)
                return building.Filter.Properties.RoofType;
            return GlobalFilter().Properties.RoofType;
        }

        public static Material FacadeMaterial(this Building building)
        {
            if (building.Properties.OverrideFacadeMaterial)
                return building.Properties.FacadeMaterial;
            else if (building.Filter && building.Filter.Properties.OverrideFacadeMaterial)
                return building.Filter.Properties.FacadeMaterial;
            return GlobalFilter().Properties.FacadeMaterial;
        }

        public static Material WindowMaterial(this Building building)
        {
            if (building.Properties.OverrideWindowMaterial)
                return building.Properties.WindowMaterial;
            else if (building.Filter && building.Filter.Properties.OverrideWindowMaterial)
                return building.Filter.Properties.WindowMaterial;
            return GlobalFilter().Properties.WindowMaterial;
        }

        public static Material FlatRoofMaterial(this Building building)
        {
            if (building.Properties.OverrideFlatRoofMaterial)
                return building.Properties.FlatRoofMaterial;
            else if (building.Filter && building.Filter.Properties.OverrideFlatRoofMaterial)
                return building.Filter.Properties.FlatRoofMaterial;
            return GlobalFilter().Properties.FlatRoofMaterial;
        }

        public static Material NonFlatRoofMaterial(this Building building)
        {
            if (building.Properties.OverrideNonFlatRoofMaterial)
                return building.Properties.NonFlatRoofMaterial;
            else if (building.Filter && building.Filter.Properties.OverrideNonFlatRoofMaterial)
                return building.Filter.Properties.NonFlatRoofMaterial;
            return GlobalFilter().Properties.NonFlatRoofMaterial;
        }
    }
}