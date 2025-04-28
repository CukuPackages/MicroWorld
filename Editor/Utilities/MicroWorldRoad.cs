using JBooth.MicroVerseCore;
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Cuku.MicroWorld
{
    public static class MicroWorldRoad
    {
        [MenuItem(nameof(MicroWorld) + "/Road/Setup Splines", priority = 1)]
        internal static void SetupSplines()
        {
            Debug.Log("Setup Intersections and Roads Splines...");

            var startTime = DateTime.Now;

            MicroWorldSpline.Crop();
            MicroWorldSpline.SplitIntersecting();
            MicroWorldSpline.CreateIntersections();
            MicroWorldSpline.ConnectContinuous();
            MicroWorldSpline.Smooth();

            var timePassed = DateTime.Now - startTime;
            Debug.Log($"Setup Intersection and Road Splines in {(int)timePassed.TotalMinutes:00}:{timePassed.Seconds:00}");
        }

        [MenuItem(nameof(MicroWorld) + "/Road/Setup Intersections", priority = 2)]
        internal static void SetupIntersections()
        {
            Debug.Log("Setup Intersections...");

            var intersections = MicroWorldSpline.Intersections();

            if (intersections.Length < 2)
            {
                Debug.LogWarning("Please select at least two game objects named Intersection to detect intersections.");
                return;
            }

            var roadSystem = new GameObject(nameof(RoadSystem), new Type[] { typeof(RoadSystem) }).GetComponent<RoadSystem>();

            for (int i = 0; i < intersections.Length; i++)
            {
                var intersection = intersections[i];
                var connecitons = intersection.Connections();
            }
        }
    }
}