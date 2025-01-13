using UnityEditor;
using UnityEngine;

namespace Cuku.MicroWorld
{
    public static class MicroWorldRoad
    {

        [MenuItem(nameof(MicroWorld) + "/Road/Setup Splines", priority = 3)]
        internal static void SetupSplines()
        {
            Debug.Log("Setup Intersections and Roads Splines...");

            var startTime = System.DateTime.Now;

            MicroWorldSpline.Crop();
            MicroWorldSpline.SplitIntersecting();
            MicroWorldSpline.CreateIntersections();
            MicroWorldSpline.ConnectContinuous();
            MicroWorldSpline.Smooth();

            var timePassed = System.DateTime.Now - startTime;
            Debug.Log($"Setup Intersection and Road Splines in {(int)timePassed.TotalMinutes:00}:{timePassed.Seconds:00}");
        }
    }
}
