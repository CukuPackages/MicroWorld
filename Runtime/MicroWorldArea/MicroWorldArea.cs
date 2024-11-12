#if UNITY_EDITOR
using JBooth.MicroVerseCore;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;

namespace Cuku.MicroWorld
{
    [RequireComponent(typeof(SplineContainer))]
    public class MicroWorldArea : MonoBehaviour
    {
        [SerializeField] public GameObject Content;

        [HideInInspector] public GameObject ContentInstance;
        [HideInInspector] public SplineArea ContentArea;

        public virtual void Spawn()
        {
            var area = new GameObject(Content.name);
            area.transform.SetParent(GameObject.FindObjectOfType<MicroVerse>(true).transform, true);

            var biomeSpline = area.AddComponent<SplineContainer>();
            var splines = new List<Spline>();
            foreach (var spline in GetComponent<SplineContainer>().Splines)
                splines.Add(new Spline(spline));
            biomeSpline.Splines = splines;

            SplineUtilities.ShiftKnots(ref biomeSpline, transform.position);

            ContentArea = area.AddComponent<SplineArea>();
            ContentArea.spline = biomeSpline;

            ContentInstance = PrefabUtility.InstantiatePrefab(Content, area.transform) as GameObject;
            ContentInstance.gameObject.name = Content.name;
            ContentInstance.transform.position = transform.position;

            ContentArea.spline.AdaptVolumeToSpline(ContentInstance.transform);
        }

        public bool IsValid()
        {
            if (GameObject.FindObjectOfType<MicroVerse>(true) == null)
            {
                Debug.LogError($"Can't find {nameof(MicroVerse)}!");
                return false;
            }
            if (Content == null)
            {
                Debug.LogError($"{nameof(Content)} is empty!");
                return false;
            }
            return true;
        }

        public void SetFilter(FalloffFilter filter)
        {
            filter.filterType = FalloffFilter.FilterType.SplineArea;
            filter.splineArea = ContentArea;
            GameObject.FindObjectOfType<MicroVerse>(true).Invalidate();
        }
    }
}
#endif