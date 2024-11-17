#if UNITY_EDITOR
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor.PackageManager.UI;
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
            blockBuilding.matRoof = this.RoofMaterial();

            // Add blocks
            //var knots = spline.Spline.Knots.RemoveInline();
            //var columns = new List<Column>();
            //foreach (var pointPosition in points)
            //    columns.Add(new Column()
            //    {
            //        pos = new Vector2(pointPosition.Position.x - center.x, pointPosition.Position.z - center.z),
            //        blocks = new List<Block>() { new Block() { height = Vector2.one * floorHeight } }
            //    });
            blockBuilding.blocks = AddColumns(spline.Spline.Knots.RemoveInline(), center, this.FloorHeight());

            blockBuilding.GenGameObject();

            // Add floors
            var floorCount = this.FloorCount();
            foreach (var block in blockBuilding.blocks)
                for (int i = 1; i < floorCount; i++)
                    block.blocks.Add((Block)block.blocks[i - 1].Clone());

            blockBuilding.GenGameObject();

            AddRoof(this.RoofType(), floorCount);
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

        List<Column> AddColumns(List<BezierKnot> points, float3 buildingCenter, float floorHeight)
        {
            var columns = new List<Column>();
            for (int i = 0; i < points.Count; i++)
            {
                var point = points[i];

                var block = new Block() { height = Vector2.one * floorHeight };
                var blockWidth = BlockWidth(points, point, i);

                // Add window
                var window = this.Window();
                if (window &&
                    blockWidth >= BuildingExtensions.GlobalFilter().Properties.WindowOpeningWidth * BuildingExtensions.WorldScale)
                    AddOpening(ref block, window, blockWidth);

                columns.Add(new Column()
                {
                    pos = new Vector2(point.Position.x - buildingCenter.x, point.Position.z - buildingCenter.z),
                    blocks = new List<Block>() { block }
                });
            }

            return columns;
        }

        void AddRoof(RoofType roofType, int floorCount)
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
                        blockBuilding.matRoof));
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

        float BlockWidth(List<BezierKnot> points, BezierKnot point, int i)
        {
            var pointPosition = new float2(point.Position.x, point.Position.z);
            var nextPoint = points[(i + 1) % points.Count];
            var nextPointPosition = new float2(nextPoint.Position.x, nextPoint.Position.z);
            return math.distance(pointPosition, nextPointPosition);
        }

        void AddOpening(ref Block block, Opening opening, float blockWidth)
        {
            var worldScale = BuildingExtensions.WorldScale;

            block.stretch = false;
            block.hasWindow = true;
            block.depth = opening.Depth;
            block.uvw = opening.UV;

            var openingWidth = opening.Width * worldScale;
            var size = new float2(
                openingWidth,
                openingWidth * opening.UV.height / opening.UV.width);

            block.t = new Rect()
            {
                x = blockWidth * opening.Position.x - size.x / 2f,
                y = block.height.x * opening.Position.y - size.y / 2f,
                width = size.x,
                height = size.y
            };
        }
    }
}
#endif