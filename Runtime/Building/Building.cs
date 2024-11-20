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


        [ContextMenu(nameof(Create))]
        public void Create()
        {
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
        public void Merge()
        {
            BuildingObject = BuildingObject.Merge(Parts);

            if (BuildingObject != null && GetComponent<BlockBuilding.BlockBuilding>())
                GameObject.DestroyImmediate(GetComponent<BlockBuilding.BlockBuilding>());

            foreach (var part in Parts)
                GameObject.DestroyImmediate(part);
            Parts.Clear();
        }

        [ContextMenu(nameof(CreateAndMerge))]
        public void CreateAndMerge()
        {
            Create();
            Merge();
        }

        [ContextMenu(nameof(Clear))]
        public void Clear()
        {
            if (BuildingObject != null)
                GameObject.DestroyImmediate(BuildingObject);

            foreach (var part in Parts)
                GameObject.DestroyImmediate(part);
            Parts.Clear();

            Filter = null;
        }

        List<Column> AddColumns(List<BezierKnot> points, float3 buildingCenter, float floorHeight)
        {
            var openingWidth = BuildingExtensions.OpeningWidth;

            var columns = new List<Column>();
            for (int i = 0; i < points.Count; i++)
            {
                var point = points[i];
                var facadePoints = FacadePoints(points, point, i);
                var facadeWidth = math.distance(facadePoints[0], facadePoints[1]);
                var divisions = Mathf.FloorToInt(facadeWidth / openingWidth);
                var columnWidth = facadeWidth / divisions;

                var window = this.Window();

                foreach (var facadePoint in SplitFacade(facadePoints[0], facadePoints[1], divisions))
                    columns.Add(new Column()
                    {
                        pos = new Vector2(facadePoint.x - buildingCenter.x, facadePoint.y - buildingCenter.z),
                        blocks = new List<Block>() { Block(floorHeight, columnWidth, window) }
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

        Block Block(float floorHeight, float width, Opening window = null)
        {
            var block = new Block() { height = Vector2.one * floorHeight };
            if (window)
                AddOpening(ref block, window, width);
            return block;
        }

        void AddOpening(ref Block block, Opening opening, float width)
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
                x = width * opening.Position.x - size.x / 2f,
                y = block.height.x * opening.Position.y - size.y / 2f,
                width = size.x,
                height = size.y
            };
        }

        List<float2> SplitFacade(float2 pointA, float2 pointB, int divisions)
        {
            var points = new List<float2>() { pointA };
            if (divisions <= 0) return points;
            for (int i = 1; i < divisions; i++)
                points.Add(Vector2.Lerp(pointA, pointB, (float)i / divisions));
            return points;
        }

        float2[] FacadePoints(List<BezierKnot> points, BezierKnot point, int i)
        {
            var pointPosition = new float2(point.Position.x, point.Position.z);
            var nextPoint = points[(i + 1) % points.Count];
            var nextPointPosition = new float2(nextPoint.Position.x, nextPoint.Position.z);
            return new float2[] { pointPosition, nextPointPosition };
        }
    }
}
#endif