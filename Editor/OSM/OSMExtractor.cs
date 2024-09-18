using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using UnityEditor;

namespace Cuku.MicroWorld
{
    [CreateAssetMenu(menuName = nameof(MicroWorld) + "/" + nameof(OSMExtractor))]
    public class OSMExtractor : ScriptableObject
    {
        public OSMSource Source = default;
        public OSMElement[] Elements = default;

        [ContextMenu(nameof(Extract))]
        void Extract()
        {
            var startTime = System.DateTime.Now;
            var elements = Elements.Extract(Source);
            File.WriteAllText(DataPath(), JsonConvert.SerializeObject(elements));
            var timePassed = System.DateTime.Now - startTime;
            Debug.Log($"Extracted {elements.Length} elements ({$"{(int)timePassed.TotalMinutes:00}:{timePassed.Seconds:00}"})");
        }

        public string DataPath()
        {
            var assetPath = AssetDatabase.GetAssetPath(this);
            return Path.Combine(Path.GetDirectoryName(assetPath),
                Path.GetFileNameWithoutExtension(assetPath) + ".json");
        }
    }
}