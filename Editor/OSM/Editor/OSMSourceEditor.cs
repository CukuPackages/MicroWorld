using UnityEditor;
using UnityEngine;

namespace Cuku.MicroWorld
{
    [CustomEditor(typeof(OSMSource))]
    public class OSMSourceEditor : Editor
    {
        SerializedProperty dataProperty;

        private void OnEnable()
            => dataProperty = serializedObject.FindProperty(nameof(OSMSource.Data));

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (GUILayout.Button("Select OSM Data"))
            {
                dataProperty.stringValue = EditorUtility.OpenFilePanel("Select OSM Data", "Assets", "pbf");
                EditorUtility.SetDirty(target);
            }

            SerializedProperty property = serializedObject.GetIterator();
            property.NextVisible(true);

            while (property.NextVisible(false))
                EditorGUILayout.PropertyField(property, true);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
