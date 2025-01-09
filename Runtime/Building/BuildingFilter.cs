#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

namespace Cuku.MicroWorld
{
    [RequireComponent(typeof(SplineContainer))]
    [RequireComponent(typeof(BuildingProperties))]
    public class BuildingFilter : MonoBehaviour
    {
        [SerializeField] public bool IsGlobal;
        [SerializeField] public List<Building> Buildings = new List<Building>();

        public SplineContainer Spline => GetComponent<SplineContainer>();
        public BuildingProperties Properties => GetComponent<BuildingProperties>();


        [ContextMenu(nameof(Filter))]
        public void Filter()
        {
            Clear();

            var spline = Spline;
            spline.Spline.Closed = true;
            spline.SetTangentMode(TangentMode.Linear);

            var allBuildings = FindObjectsByType<Building>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < allBuildings.Length; i++)
            {
                var building = allBuildings[i];
                if (building.IncludeInFilter && spline.Contains(building.Center()))
                {
                    Buildings.Add(building);
                    building.Filter = this;
                }
            }

            if (!IsGlobal)
                return;

            foreach (var filter in BuildingExtensions.OtherFilters())
                filter.Filter();

            ClearFilters();
        }

        [ContextMenu(nameof(CreateBuildings))]
        public void CreateBuildings()
        {
            ClearFilters();

            if (IsGlobal)
                foreach (var filter in BuildingExtensions.OtherFilters())
                    filter.Buildings.ForEach(building => building.Create());

            Buildings.ForEach(building => building.Create());
        }

        [ContextMenu(nameof(MergeBuildings))]
        public void MergeBuildings()
        {
            ClearFilters();

            if (IsGlobal)
                foreach (var filter in BuildingExtensions.OtherFilters())
                    filter.Buildings.ForEach(building => building.Merge());

            Buildings.ForEach(building => building.Merge());
        }

        [ContextMenu(nameof(CreateAndMerge))]
        public void CreateAndMerge()
        {
            Filter();
            CreateBuildings();
            MergeBuildings();
            Clear();
        }

        [ContextMenu(nameof(Clear))]
        public void Clear()
        {
            Buildings.ForEach(building => building.Clear());
            Buildings.Clear();
        }

        public void ClearFilters()
        {
            var globalFilter = BuildingExtensions.GlobalFilter();

            var filters = BuildingExtensions.OtherFilters();

            foreach (var filter in filters)
                globalFilter.Buildings.RemoveAll(item => filter.Buildings.Contains(item));

            foreach (var currentFilter in filters)
                foreach (var filter in filters)
                    if (!currentFilter.IsGlobal && currentFilter != filter)
                        filter.Buildings.RemoveAll(item => currentFilter.Buildings.Contains(item));
        }
    }
}
#endif