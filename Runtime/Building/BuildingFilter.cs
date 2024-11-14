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


        [ContextMenu(nameof(RunAll))]
        public void RunAll()
        {
            Filter();
            Build();
            MergeSingleBuildings();
            MergeAllBuildings();
            Finish();
        }

        [ContextMenu(nameof(Show))]
        public void Show()
        {
            var splineContainer = GetComponent<SplineContainer>();
            foreach (var spline in splineContainer.Splines)
                spline.Closed = true;
            splineContainer.MakeLinear();
        }

        [ContextMenu(nameof(Filter))]
        public void Filter()
        {
            Show();

            var spline = Spline;

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
        }

        [ContextMenu(nameof(Build))]
        public void Build() => Buildings.ForEach(building => building.Build());

        [ContextMenu(nameof(MergeSingleBuildings))]
        public void MergeSingleBuildings() => Buildings.ForEach(building => building.Merge());

        [ContextMenu(nameof(MergeAllBuildings))]
        public void MergeAllBuildings()
        {
            var main = Buildings[0].BuildingObject;
            var parts = new List<GameObject>();
            for (int i = 1; i < Buildings.Count; i++)
                parts.Add(Buildings[i].BuildingObject);
            _ = main.Merge(parts);
        }

        [ContextMenu(nameof(Finish))]
        public void Finish()
        {
            Buildings.ForEach(building => building.Clear());
            Buildings.Clear();
        }

        public SplineContainer Spline => GetComponent<SplineContainer>();

        public BuildingProperties Properties => GetComponent<BuildingProperties>();
    }
}
#endif