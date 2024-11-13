using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.Splines;

namespace Cuku.MicroWorld
{
    [RequireComponent(typeof(SplineContainer))]
    public class BuildingFilter : MonoBehaviour
    {
        [SerializeField] List<Building> buildings = new List<Building>();


        [ContextMenu(nameof(RunAll))]
        public void RunAll()
        {
            Filter();
            Build();
            MergeSingleBuildings();
            MergeAllBuildings();
            Finish();
        }

        [ContextMenu(nameof(Set))]
        public void Set()
        {
            var splineContainer = GetComponent<SplineContainer>();
            foreach (var spline in splineContainer.Splines)
                spline.Closed = true;
            splineContainer.MakeLinear();
        }

        [ContextMenu(nameof(Filter))]
        public void Filter()
        {
            Set();

            var spline = GetComponent<SplineContainer>();

            var allBuildings = FindObjectsByType<Building>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < allBuildings.Length; i++)
                if (spline.Contains(allBuildings[i].Center()))
                    buildings.Add(allBuildings[i]);
        }

        [ContextMenu(nameof(Build))]
        public void Build() => buildings.ForEach(building => building.Build());

        [ContextMenu(nameof(MergeSingleBuildings))]
        public void MergeSingleBuildings() => buildings.ForEach(building => building.Merge());

        [ContextMenu(nameof(MergeAllBuildings))]
        public void MergeAllBuildings()
        {
            var main = buildings[0].building;
            var parts = new List<GameObject>();
            for (int i = 1; i < buildings.Count; i++)
                parts.Add(buildings[i].building);
            main.Merge(parts);
        }

        [ContextMenu(nameof(Finish))]
        public void Finish()
        {
            buildings.ForEach(building => building.Clear());
            buildings.Clear();
        }
    }
}
#endif