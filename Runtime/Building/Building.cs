using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using static BlockBuilding.BlockBuilding;

namespace Cuku.MicroWorld
{
    [RequireComponent(typeof(SplineContainer))]
    public class Building : MonoBehaviour
    {
        [SerializeField] BuildingAssets assets;
        [SerializeField] GameObject building;
        [SerializeField] List<GameObject> parts = new List<GameObject>();


        [ContextMenu(nameof(Build))]
        public void Build()
        {
            var spline = GetComponent<SplineContainer>();
            spline.SetOrder(clockwise: true);

            building = new GameObject(assets.name, new Type[] { typeof(BlockBuilding.BlockBuilding) });
            var blockBuilding = building.GetComponent<BlockBuilding.BlockBuilding>();
            var center = spline.Center();
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

        [ContextMenu(nameof(Combine))]
        public void Combine() => building.Combine(parts);

        [ContextMenu(nameof(Clean))]
        public void Clean()
        {
            GameObject.DestroyImmediate(GetComponent<BlockBuilding.BlockBuilding>());
            building = null;
            foreach (var part in parts)
                GameObject.DestroyImmediate(part);
            parts.Clear();
        }
    }
}