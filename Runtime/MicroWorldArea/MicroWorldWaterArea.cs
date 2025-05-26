#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;

namespace Cuku.MicroWorld
{
    [RequireComponent(typeof(SplineContainer))]
    public class MicroWorldWaterArea : MonoBehaviour
    {
        [SerializeField] public GameObject WaterArea;
        [SerializeField] public string Parent;
        [SerializeField] public bool SnapToTerrain;

        [ContextMenu(nameof(Spawn))]
        public void Spawn()
        {
            var instance = PrefabUtility.InstantiatePrefab(WaterArea) as GameObject;
            if (!string.IsNullOrEmpty(Parent))
                instance.transform.SetParent(GameObject.Find(Parent).transform);
            instance.transform.position = transform.position;

            var splineContainer = gameObject.GetComponent<SplineContainer>();
            var ram = instance.GetComponent<LakePolygon>();
            ram.snapToTerrain = !SnapToTerrain;
            foreach (var spline in splineContainer.Splines)
                foreach (var knots in spline.Knots)
                    ram.AddPoint(knots.Position);
            ram.GeneratePolygon();
        }
    }
}
#endif