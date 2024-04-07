#if UNITY_EDITOR
using JBooth.MicroVerseCore;
using System.Collections.Generic;
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

            Utilities.ShiftSplineKnots(ref biomeSpline, transform.position);

            var splineArea = biomeArea.AddComponent<SplineArea>();
            splineArea.spline = biomeSpline;

            var biomeInstance = PrefabUtility.InstantiatePrefab(Biome, biomeArea.transform) as GameObject;
            biomeInstance.gameObject.name = Biome.name;
            biomeInstance.transform.position = transform.position;

            biomeInstance.transform.AdaptVolumeToSpline(splineArea.spline);

            var filter = biomeInstance.GetComponent<FalloffOverride>().filter;
            filter.filterType = FalloffFilter.FilterType.SplineArea;
            filter.splineArea = splineArea;

            MicroVerse.instance.Invalidate();
        }
    }
}
#endif