using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using UnityEditor;

namespace Cuku.MicroWorld
{
    [CreateAssetMenu(menuName = nameof(OSM) + "/" + nameof(Extractor))]
    public class Extractor : ScriptableObject
    {
        public Source Source = default;
        public Element[] Elements = default;

        [ContextMenu(nameof(Extract))]
        void Extract()
        {
            var startTime = System.DateTime.Now;
            var elements = Elements.ExtractElements(Source);
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