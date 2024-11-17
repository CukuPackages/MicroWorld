using Cuku.MicroWorld;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BuildingProperties))]
public class BuildingPropertiesEditor : Editor
{
    int space = 10;
    SerializedProperty door;
    SerializedProperty window;
    SerializedProperty roofType;
    SerializedProperty facadeMaterials;
    SerializedProperty windowMaterials;
    SerializedProperty flatRoofMaterials;

    private void OnEnable()
    {
        door = serializedObject.FindProperty(nameof(BuildingProperties.Door));
        window = serializedObject.FindProperty(nameof(BuildingProperties.Window));
        roofType = serializedObject.FindProperty(nameof(BuildingProperties.RoofType));
        facadeMaterials = serializedObject.FindProperty(nameof(BuildingProperties.FacadeMaterial));
        windowMaterials = serializedObject.FindProperty(nameof(BuildingProperties.OpeningMaterial));
        flatRoofMaterials = serializedObject.FindProperty(nameof(BuildingProperties.RoofMaterial));
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        BuildingProperties buildingProperties = (BuildingProperties)target;

        buildingProperties.WorldScale = EditorGUILayout.FloatField(nameof(buildingProperties.WorldScale), buildingProperties.WorldScale);

        EditorGUILayout.Space(space);

        buildingProperties.WindowOpeningWidth = EditorGUILayout.FloatField(nameof(buildingProperties.WindowOpeningWidth), buildingProperties.WindowOpeningWidth);

        EditorGUILayout.Space(space);

        buildingProperties.OverrideFloorHeight = EditorGUILayout.Toggle(nameof(buildingProperties.OverrideFloorHeight), buildingProperties.OverrideFloorHeight);
        if (buildingProperties.OverrideFloorHeight)
            buildingProperties.FloorHeight = EditorGUILayout.Vector2Field(nameof(buildingProperties.FloorHeight), buildingProperties.FloorHeight);

        EditorGUILayout.Space(space);

        buildingProperties.OverrideFloorCount = EditorGUILayout.Toggle(nameof(buildingProperties.OverrideFloorCount), buildingProperties.OverrideFloorCount);
        if (buildingProperties.OverrideFloorCount)
            buildingProperties.FloorCount = EditorGUILayout.Vector2IntField(nameof(buildingProperties.FloorCount), buildingProperties.FloorCount);

        EditorGUILayout.Space(space);

        buildingProperties.OverrideDoor = EditorGUILayout.Toggle(nameof(buildingProperties.OverrideDoor), buildingProperties.OverrideDoor);
        if (buildingProperties.OverrideDoor)
            EditorGUILayout.PropertyField(door, new GUIContent(nameof(buildingProperties.Door)), true);

        EditorGUILayout.Space(space);

        buildingProperties.OverrideWindow = EditorGUILayout.Toggle(nameof(buildingProperties.OverrideWindow), buildingProperties.OverrideWindow);
        if (buildingProperties.OverrideWindow)
            EditorGUILayout.PropertyField(window, new GUIContent(nameof(buildingProperties.Window)), true);

        EditorGUILayout.Space(space);

        buildingProperties.OverrideRoofType = EditorGUILayout.Toggle(nameof(buildingProperties.OverrideRoofType), buildingProperties.OverrideRoofType);
        if (buildingProperties.OverrideRoofType)
            EditorGUILayout.PropertyField(roofType, new GUIContent(nameof(buildingProperties.RoofType)), true);

        EditorGUILayout.Space(space);

        buildingProperties.OverrideFacadeMaterial = EditorGUILayout.Toggle(nameof(buildingProperties.OverrideFacadeMaterial), buildingProperties.OverrideFacadeMaterial);
        if (buildingProperties.OverrideFacadeMaterial)
            EditorGUILayout.PropertyField(facadeMaterials, new GUIContent(nameof(buildingProperties.FacadeMaterial)), true);

        EditorGUILayout.Space(space);

        buildingProperties.OverrideOpeningMaterial = EditorGUILayout.Toggle(nameof(buildingProperties.OverrideOpeningMaterial), buildingProperties.OverrideOpeningMaterial);
        if (buildingProperties.OverrideOpeningMaterial)
            EditorGUILayout.PropertyField(windowMaterials, new GUIContent(nameof(buildingProperties.OpeningMaterial)), true);

        EditorGUILayout.Space(space);

        buildingProperties.OverrideRoofMaterial = EditorGUILayout.Toggle(nameof(buildingProperties.OverrideRoofMaterial), buildingProperties.OverrideRoofMaterial);
        if (buildingProperties.OverrideRoofMaterial)
            EditorGUILayout.PropertyField(flatRoofMaterials, new GUIContent(nameof(buildingProperties.RoofMaterial)), true);

        serializedObject.ApplyModifiedProperties();

        if (GUI.changed)
            EditorUtility.SetDirty(buildingProperties);
    }
}
