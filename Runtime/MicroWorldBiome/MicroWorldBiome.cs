#if UNITY_EDITOR
using JBooth.MicroVerseCore;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;

namespace Cuku.MicroWorld
{
    [RequireComponent(typeof(SplineContainer))]
    public class MicroWorldBiome : MonoBehaviour
    {
        [SerializeField] GameObject Biome;

        [ContextMenu(nameof(Spawn))]
        void Spawn()
        {
            var microverse = MicroVerse.instance;
            if (microverse == null)
            {
                Debug.LogError($"Can't find {nameof(MicroVerse)}!");
                return;
            }
            if (Biome == null)
            {
                Debug.LogError($"{nameof(Biome)} is empty!");
                return;
            }

            var biomeArea = new GameObject($"{nameof(Biome)} {Biome.name}");
            biomeArea.transform.SetParent(microverse.transform, true);

            var biomeSpline = biomeArea.AddComponent<SplineContainer>();
            var splines = new List<Spline>();
            foreach (var spline in GetComponent<SplineContainer>().Splines)
                splines.Add(new Spline(spline));
            biomeSpline.Splines = splines;

            ShiftKnots(ref biomeSpline, transform.position);

            var splineArea = biomeArea.AddComponent<SplineArea>();
            splineArea.spline = biomeSpline;

            var biomeInstance = PrefabUtility.InstantiatePrefab(Biome, biomeArea.transform) as GameObject;
            biomeInstance.gameObject.name = Biome.name;
            biomeInstance.transform.position = transform.position;

            AdaptVolumeToSpline(splineArea.spline, biomeInstance.transform);

            var filter = biomeInstance.GetComponent<FalloffOverride>().filter;
            filter.filterType = FalloffFilter.FilterType.SplineArea;
            filter.splineArea = splineArea;

            MicroVerse.instance.Invalidate();
        }

        void ShiftKnots(ref SplineContainer splineContainer, Vector3 shift)
        {
            var shiftAmmount = (float3)shift;
            foreach (var spline in splineContainer.Splines)
            {
                var knots = spline.Knots.ToArray();
                for (int i = 0; i < knots.Length; i++)
                    knots[i].Position += shiftAmmount;
                for (int i = 0; i < knots.Length; i++)
                    spline.SetKnot(i, knots[i]);
            }
        }

        void AdaptVolumeToSpline(SplineContainer splineContainer, Transform target)
        {
            var points = new List<float3>();
            foreach (var spline in splineContainer.Splines)
                foreach (var knot in spline.Knots)
                    points.Add(knot.Position);

            // Calculate center
            float3 sum = float3.zero;
            foreach (float3 point in points)
                sum += point;
            float3 center = sum / points.Count;

            // Calculate the dimensions of the bounding box
            var min = new float3(float.MaxValue, float.MaxValue, float.MaxValue);
            var max = new float3(float.MinValue, float.MinValue, float.MinValue);
            foreach (float3 point in points)
            {
                min = math.min(min, point);
                max = math.max(max, point);
            }
            var dimensions = max - min;

            target.position = (Vector3)center;
            target.localScale = new Vector3(dimensions.x, target.localScale.y, dimensions.z);
        }
    }
}
#endif