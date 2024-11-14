using UnityEngine;
using System.Collections.Generic;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using Cuku.Utilities;

namespace Cuku.MicroWorld
{
    public static class MeshExtensions
    {
        public static GameObject CreatePolyShape(this List<Vector2> points, float height, Material material)
        {
            var pbMesh = new GameObject("Poly Shape").AddComponent<ProBuilderMesh>();
            var polyShape = pbMesh.gameObject.AddComponent<PolyShape>();

            var controlPoints = new List<Vector3>();
            foreach (var point in points)
                controlPoints.Add(new Vector3(point.x, height, point.y));

            polyShape.SetControlPoints(controlPoints);

            AppendElements.CreateShapeFromPolygon(polyShape);

            polyShape.GetComponent<MeshRenderer>().sharedMaterial = material;

            return pbMesh.Bake();
        }

        public static GameObject Merge(this GameObject mainObject, List<GameObject> gameObjects)
        {
            if (gameObjects.Count == 0)
                return mainObject;
            var pbMeshes = new List<ProBuilderMesh> { mainObject.ToProBuilderMesh() };
            pbMeshes.AddRange(gameObjects.ToProBuilderMeshes());
            return CombineMeshes.Combine(pbMeshes, pbMeshes[0])[0].Bake();
        }

        public static GameObject Bake(this ProBuilderMesh pbMesh)
        {
            var baked = new GameObject(pbMesh.name);
            baked.transform.position = pbMesh.transform.position;
            baked.transform.rotation = pbMesh.transform.rotation;
            baked.transform.localScale = pbMesh.transform.localScale;

            var mesh = new Mesh();
            MeshUtility.Compile(pbMesh, mesh);

            var meshFilter = baked.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = mesh;
            MeshUtility.CollapseSharedVertices(meshFilter.sharedMesh);
            pbMesh.GetComponent<MeshRenderer>().CopyTo(baked);

            GameObject.DestroyImmediate(pbMesh.gameObject);

            return baked;
        }

        public static void Clean(this ProBuilderMesh pbMesh)
        {
            pbMesh.ToMesh();
            pbMesh.Refresh();
        }

        public static List<ProBuilderMesh> ToProBuilderMeshes(this List<GameObject> gameObjects)
        {
            var pbMeshes = new List<ProBuilderMesh>();
            foreach (GameObject gameObject in gameObjects)
                pbMeshes.Add(gameObject.ToProBuilderMesh());
            return pbMeshes;
        }

        public static ProBuilderMesh ToProBuilderMesh(this GameObject gameObject)
        {
            new MeshImporter(gameObject).Import();
            var pbMesh = gameObject.GetComponent<ProBuilderMesh>();
            pbMesh.Clean();
            return pbMesh;
        }
    }
}