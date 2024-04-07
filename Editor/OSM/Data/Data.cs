using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using UnityEditor;

namespace Cuku.MicroWorld
{
    [CreateAssetMenu(menuName = nameof(OSM) + "/" + nameof(Data) + " Extractor")]
    public class Data : ScriptableObject
    {
        public Source Source = default;
        public Element[] Elements = default;

        [ContextMenu(nameof(Extract))]
        void Extract()
        {
            var startTime = System.DateTime.Now;
            File.WriteAllText(DataPath(), JsonConvert.SerializeObject(Elements.ExtractElementsPoints(Source)));
            var timePassed = System.DateTime.Now - startTime;
            Debug.Log($"Data extracted sucessfully ({$"{(int)timePassed.TotalMinutes:00}:{timePassed.Seconds:00}"})");
        }

        public string DataPath()
        {
            var assetPath = AssetDatabase.GetAssetPath(this);
            return Path.Combine(Path.GetDirectoryName(assetPath),
                Path.GetFileNameWithoutExtension(assetPath) + ".json");
        }
    }
}