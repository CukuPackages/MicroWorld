#if UNITY_EDITOR
using JBooth.MicroVerseCore;
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
            var area = new GameObject(Content.name + " Area");
            area.transform.position = transform.position;

            area.transform.SetParent(string.IsNullOrEmpty(ContentParent) ?
                MicroVerse.instance.transform :
                MicroVerse.instance.transform.Find(ContentParent)
                , true);

            ContentArea = area.AddComponent<SplineArea>();
            ContentArea.spline = area.CopySplineContainerFrom(gameObject);

            ContentInstance = PrefabUtility.InstantiatePrefab(Content, area.transform) as GameObject;
            ContentInstance.gameObject.name = Content.name;

            ContentArea.spline.AdaptVolumeToSpline(ContentInstance.transform);
            var scale = ContentInstance.transform.localScale;
            scale.x *= ContentVolumeScale.x;
            scale.y *= ContentVolumeScale.y;
            scale.z *= ContentVolumeScale.z;

            foreach (var stamp in area.GetComponentsInChildren<HeightStamp>())
                SetFilter(stamp.GetFilterSet().falloffFilter);
            foreach (var stamp in area.GetComponentsInChildren<TreeStamp>())
                SetFilter(stamp.filterSet.falloffFilter);
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

        void SetFilter(FalloffFilter filter)
        {
            filter.filterType = FalloffFilter.FilterType.SplineArea;
            filter.splineArea = ContentArea;
            MicroVerse.instance.Invalidate();
        }
    }
}
#endif