using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cuku.MicroWorld
{
    [CreateAssetMenu(menuName = nameof(MicroWorld) + "/" + nameof(RoadAssets))]
    public class RoadAssets : ScriptableObject
    {
        [SerializeField]
        public GameObject RoadSystem;

        [SerializeField]
        List<IntersectionAsset> Intersections;

        [SerializeField]
        List<RoadAsset> Roads;

        public GameObject GetIntersection(string connections)
        {
            foreach (var intersections in Intersections)
                if (intersections.Connections == connections)
                    return intersections.Assets[UnityEngine.Random.Range(0, intersections.Assets.Length)];
            return null;
        }

        public GameObject GetRoad(string type)
        {
            foreach (var roads in Roads)
                if (roads.Type == type)
                    return roads.Assets[UnityEngine.Random.Range(0, roads.Assets.Length)];
            return null;
        }
    }

    [Serializable]
    public struct IntersectionAsset
    {
        public string Connections;
        public GameObject[] Assets;
    }

    [Serializable]
    public struct RoadAsset
    {
        public string Type;
        public GameObject[] Assets;
    }
}