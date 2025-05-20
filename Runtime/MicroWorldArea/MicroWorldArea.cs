#if UNITY_EDITOR
using JBooth.MicroVerseCore;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;

namespace Cuku.MicroWorld
{
    [RequireComponent(typeof(SplineContainer))]
    public class MicroWorldArea : MonoBehaviour
    {
        [SerializeField] public GameObject Content;
        [SerializeField] public string ContentParent;
        [SerializeField] public float3 ContentVolumeScale = new float3(1f, 1f, 1f);

        [HideInInspector] public GameObject ContentInstance;
        [HideInInspector] public SplineArea ContentArea;

        [ContextMenu(nameof(Spawn))]
        public virtual void Spawn()
        {
            var area = new GameObject(Content.name);
            area.transform.position = transform.position;

            area.transform.SetParent(string.IsNullOrEmpty(ContentParent) ?
                MicroVerse.instance.transform :
                MicroVerse.instance.transform.Find(ContentParent)
                , true);

            var areaSpline = area.AddComponent<SplineContainer>();
            var splines = new List<Spline>();
            foreach (var spline in GetComponent<SplineContainer>().Splines)
                splines.Add(new Spline(spline));
            areaSpline.Splines = splines;

            SplineExtensions.ShiftKnots(ref areaSpline, transform.position);

            ContentArea = area.AddComponent<SplineArea>();
            ContentArea.spline = areaSpline;

            ContentInstance = PrefabUtility.InstantiatePrefab(Content, area.transform) as GameObject;
            ContentInstance.gameObject.name = Content.name;
            ContentInstance.transform.position = transform.position;

            ContentArea.spline.AdaptVolumeToSpline(ContentInstance.transform);
            var position = ContentInstance.transform.position;
            position.x *= ContentVolumeScale.x;
            position.y *= ContentVolumeScale.y;
            position.z *= ContentVolumeScale.z;
            ContentInstance.transform.position = position;
            var scale = ContentInstance.transform.localScale;
            scale.x *= ContentVolumeScale.x;
            scale.y *= ContentVolumeScale.y;
            scale.z *= ContentVolumeScale.z;
            ContentInstance.transform.localScale = scale;
            //UnityEditor.EditorUtility.SetDirty(ContentInstance);
            //var stamp = ContentInstance.GetComponent<HeightStamp>();
            //if (stamp) MicroVerse.instance?.Invalidate(stamp.GetBounds());
            //MicroVerse.instance?.Invalidate();
        }

        public bool IsValid()
        {
            if (MicroVerse.instance == null)
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
            MicroVerse.instance.Invalidate();
        }
    }
}
#endif