#if UNITY_EDITOR
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;
using static BlockBuilding.BlockBuilding;

namespace Cuku.MicroWorld
{
    [RequireComponent(typeof(SplineContainer))]
    [RequireComponent(typeof(BuildingProperties))]
    public class Building : MonoBehaviour
    {
        [SerializeField] public GameObject BuildingObject;
        [SerializeField] public List<GameObject> Parts = new List<GameObject>();

        [SerializeField] public bool IncludeInFilter = true;
        [SerializeField] public BuildingFilter Filter;



        [ContextMenu(nameof(RunAll))]
        public void RunAll()
        {
            Build();
            Merge();
            Clear();
        }

        [ContextMenu(nameof(Build))]
        public void Build()
        {
            Clear();

            var spline = Spline;
            spline.SetOrder(clockwise: true);

            BuildingObject = new GameObject(name);
            var blockBuilding = BuildingObject.AddComponent<BlockBuilding.BlockBuilding>();
            var center = Center();
            blockBuilding.transform.position = new Vector3(center.x, spline.LowestPoint(), center.z);

            blockBuilding.genCollider = false;

            blockBuilding.GenGameObject();

            // Materials
            var renderer = blockBuilding.GetComponent<Renderer>();
            renderer.material = blockBuilding.matFacade = this.FacadeMaterial();
            blockBuilding.matWindow = this.WindowMaterial();
            blockBuilding.matRoof = RoofMaterial();

            // Columns
            var knots = spline.Spline.Knots.RemoveInline();
            var columns = new List<Column>();
            foreach (var knot in knots)
                columns.Add(new Column()
                {
                    pos = new Vector2(knot.Position.x - center.x, knot.Position.z - center.z),
                    blocks = new List<Block>() { new Block() { height = Vector2.one * this.FloorHeight() } }
                });
            blockBuilding.blocks = columns;

            blockBuilding.GenGameObject();

            var floorCount = this.FloorCount();
            foreach (var block in blockBuilding.blocks)
                for (int i = 1; i < floorCount; i++)
                    block.blocks.Add((Block)block.blocks[i - 1].Clone());

            blockBuilding.GenGameObject();

            BuildRoof(this.RoofType(), floorCount);
        }

        [ContextMenu(nameof(Merge))]
        public void Merge() => BuildingObject = BuildingObject.Merge(Parts);

        [ContextMenu(nameof(Finish))]
        public void Finish()
        {
            if (BuildingObject != null && GetComponent<BlockBuilding.BlockBuilding>())
                GameObject.DestroyImmediate(GetComponent<BlockBuilding.BlockBuilding>());

            foreach (var part in Parts)
                GameObject.DestroyImmediate(part);
            Parts.Clear();
        }

        [ContextMenu(nameof(Clear))]
        public void Clear()
        {
            Finish();
            if (BuildingObject != null)
                GameObject.DestroyImmediate(BuildingObject);
        }

        Material RoofMaterial()
            => this.RoofType() == RoofType.Flat ? this.FlatRoofMaterial() : this.NonFlatRoofMaterial();

        void BuildRoof(RoofType roofType, int floorCount)
        {
            var blockBuilding = BlockBuilding;
            var spline = Spline;

            switch (roofType)
            {
                case RoofType.Flat:
                    foreach (var column in blockBuilding.blocks)
                        column.roofAnchor = Vector3.zero;
                    Parts.Add(spline.Points2D().CreatePolyShape(
                        blockBuilding.transform.position.y + blockBuilding.blocks[0].blocks[0].height.y * floorCount,
                        this.FlatRoofMaterial()));
                    return;
                case RoofType.Pyramidal:
                    break;
                case RoofType.Hipped:
                    break;
                case RoofType.Gabled:
                    break;
                default:
                    break;
            }

            BlockBuilding.GenGameObject();
        }

        public BlockBuilding.BlockBuilding BlockBuilding => BuildingObject.GetComponent<BlockBuilding.BlockBuilding>();

        public SplineContainer Spline => GetComponent<SplineContainer>();

        public float3 Center() => Spline.Center();

        public BuildingProperties Properties => GetComponent<BuildingProperties>();
    }
}
#endif