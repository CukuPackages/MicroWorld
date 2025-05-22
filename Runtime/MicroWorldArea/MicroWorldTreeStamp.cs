#if UNITY_EDITOR
using JBooth.MicroVerseCore;
using UnityEngine;
using UnityEngine.Splines;

namespace Cuku.MicroWorld
{
    [RequireComponent(typeof(SplineContainer))]
    public class MicroWorldTreeStamp : MicroWorldArea
    {
        [SerializeField] float Density = 2;


        [ContextMenu(nameof(Spawn))]
        public override void Spawn()
        {
            if (!IsValid()) return;

            base.Spawn();

            var treeStamp = ContentInstance.GetComponent<TreeStamp>();
            treeStamp.density = Density;
        }
    }
}
#endif