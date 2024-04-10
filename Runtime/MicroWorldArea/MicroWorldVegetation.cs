#if UNITY_EDITOR
using JBooth.MicroVerseCore;
using UnityEngine;
using UnityEngine.Splines;

namespace Cuku.MicroWorld
{
    [RequireComponent(typeof(SplineContainer))]
    public class MicroWorldVegetation : MicroWorldArea
    {
        [ContextMenu(nameof(Spawn))]
        public override void Spawn()
        {
            if (!IsValid()) return;

            base.Spawn();

            SetFilter(ContentInstance.GetComponent<TreeStamp>().filterSet.falloffFilter);
        }
    }
}
#endif