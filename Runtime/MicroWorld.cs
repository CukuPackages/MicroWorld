#if UNITY_EDITOR
using JBooth.MicroVerseCore;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Cuku.MicroWorld
{
    public static class MicroWorld
    {
        public static string MicroVerseTerrainDataPath(TerrainData terrainData)
             => Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(AssetDatabase.GetAssetPath(terrainData))), nameof(MicroVerse));

        public static void DuplicateDirectory(string sourceDirPath, string destinationDirPath)
        {
            if (!Directory.Exists(destinationDirPath))
                Directory.CreateDirectory(destinationDirPath);

            // Get the files in the source directory
            string[] files = Directory.GetFiles(sourceDirPath);

            // Copy each file to the destination directory
            foreach (string file in files)
            {
                string fileName = Path.GetFileName(file);
                string destinationFilePath = Path.Combine(destinationDirPath, fileName);
                File.Copy(file, destinationFilePath, false);
            }

            // Get the subdirectories in the source directory
            string[] subDirectories = Directory.GetDirectories(sourceDirPath);

            // Recursively duplicate each subdirectory
            foreach (string subDirectory in subDirectories)
            {
                string directoryName = Path.GetFileName(subDirectory);
                string destinationSubDirPath = Path.Combine(destinationDirPath, directoryName);
                DuplicateDirectory(subDirectory, destinationSubDirPath);
            }
        }

        public static Transform[] Children(string parentName)
        {
            var parent = GameObject.Find(parentName).transform;
            var intersections = new HashSet<Transform>(parent.GetComponentsInChildren<Transform>());
            intersections.Remove(parent);
            return intersections.ToArray();
        }
    }
}
#endif