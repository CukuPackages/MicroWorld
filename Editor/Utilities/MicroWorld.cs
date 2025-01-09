using JBooth.MicroVerseCore;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Cuku.MicroWorld
{
    public static class MicroWorld
    {
        internal static string MicroVerseTerrainDataPath(this TerrainData terrainData)
             => Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(AssetDatabase.GetAssetPath(terrainData))), nameof(MicroVerse));

        internal static void DuplicateDirectory(string sourceDirPath, string destinationDirPath)
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
    }
}
