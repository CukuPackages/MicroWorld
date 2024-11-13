#if UNITY_EDITOR
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;
using static BlockBuilding.BlockBuilding;

namespace Cuku.MicroWorld
{
    [RequireComponent(typeof(SplineContainer))]
    public class Building : MonoBehaviour
    {
        [SerializeField] public BuildingAssets assets;
        [SerializeField] public GameObject building;
        [SerializeField] public List<GameObject> parts = new List<GameObject>();


        [ContextMenu(nameof(Clear))]
        public void Clear()
        {
            Finish();
            if (building != null)
                GameObject.DestroyImmediate(building);
        }

        [ContextMenu(nameof(Build))]
        public void Build()
        {
            Clear();

            var spline = GetComponent<SplineContainer>();
            spline.SetOrder(clockwise: true);

            building = new GameObject(assets.name);
            var blockBuilding = building.AddComponent<BlockBuilding.BlockBuilding>();
            var center = Center();
            blockBuilding.transform.position = new Vector3(center.x, spline.LowestPoint(), center.z);

            blockBuilding.genCollider = false;

            blockBuilding.GenGameObject();

            // Materials
            var renderer = blockBuilding.GetComponent<Renderer>();
            renderer.material = blockBuilding.matFacade = assets.Facade;
            blockBuilding.matWindow = assets.Window;
            blockBuilding.matRoof = assets.Roof;

            // Columns
            var knots = spline.Spline.Knots.RemoveInline();
            var columns = new List<Column>();
            foreach (var knot in knots)
                columns.Add(new Column()
                {
                    pos = new Vector2(knot.Position.x - center.x, knot.Position.z - center.z),
                    blocks = new List<Block>() { new Block() }
                });
            blockBuilding.blocks = columns;

            foreach (var column in columns)
                column.roofAnchor = Vector3.zero;

            blockBuilding.GenGameObject();

            // Add flat roof
            parts.Add(spline.Points().CreatePolyShape(blockBuilding.transform.position.y + columns[0].blocks[0].height.y, assets.Roof));
        }

        [ContextMenu(nameof(Merge))]
        public void Merge() => building = building.Merge(parts);

        [ContextMenu(nameof(Finish))]
        public void Finish()
        {
            if (building != null && GetComponent<BlockBuilding.BlockBuilding>())
                GameObject.DestroyImmediate(GetComponent<BlockBuilding.BlockBuilding>());

            foreach (var part in parts)
                GameObject.DestroyImmediate(part);
            parts.Clear();
        }

        public float3 Center() => GetComponent<SplineContainer>().Center();
    }
}
#endif