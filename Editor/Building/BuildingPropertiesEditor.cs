using Cuku.MicroWorld;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BuildingProperties))]
public class BuildingPropertiesEditor : Editor
{
    public override void OnInspectorGUI()
    {
        BuildingProperties buildingProperties = (BuildingProperties)target;

        EditorGUILayout.LabelField(nameof(BuildingProperties), EditorStyles.boldLabel);

        buildingProperties.WorldScale = EditorGUILayout.FloatField(nameof(buildingProperties.WorldScale), buildingProperties.WorldScale);

        buildingProperties.OverrideFloorHeight = EditorGUILayout.Toggle(nameof(buildingProperties.OverrideFloorHeight), buildingProperties.OverrideFloorHeight);
        if (buildingProperties.OverrideFloorHeight)
            buildingProperties.FloorHeight = EditorGUILayout.FloatField(nameof(buildingProperties.FloorHeight), buildingProperties.FloorHeight);

        buildingProperties.OverrideFloorCount = EditorGUILayout.Toggle(nameof(buildingProperties.OverrideFloorCount), buildingProperties.OverrideFloorCount);
        if (buildingProperties.OverrideFloorCount)
            buildingProperties.FloorCount = EditorGUILayout.IntField(nameof(buildingProperties.FloorCount), buildingProperties.FloorCount);

        buildingProperties.OverrideRoofType = EditorGUILayout.Toggle(nameof(buildingProperties.OverrideRoofType), buildingProperties.OverrideRoofType);
        if (buildingProperties.OverrideRoofType)
            buildingProperties.RoofType = (RoofType)EditorGUILayout.EnumPopup(nameof(buildingProperties.RoofType), buildingProperties.RoofType);

        buildingProperties.OverrideFacadeMaterial = EditorGUILayout.Toggle(nameof(buildingProperties.OverrideFacadeMaterial), buildingProperties.OverrideFacadeMaterial);
        if (buildingProperties.OverrideFacadeMaterial)
            buildingProperties.FacadeMaterial = (Material)EditorGUILayout.ObjectField(nameof(buildingProperties.FacadeMaterial), buildingProperties.FacadeMaterial, typeof(Material), allowSceneObjects: false);

        buildingProperties.OverrideWindowMaterial = EditorGUILayout.Toggle(nameof(buildingProperties.OverrideWindowMaterial), buildingProperties.OverrideWindowMaterial);
        if (buildingProperties.OverrideWindowMaterial)
            buildingProperties.WindowMaterial = (Material)EditorGUILayout.ObjectField(nameof(buildingProperties.WindowMaterial), buildingProperties.WindowMaterial, typeof(Material), allowSceneObjects: false);

        buildingProperties.OverrideFlatRoofMaterial = EditorGUILayout.Toggle(nameof(buildingProperties.OverrideFlatRoofMaterial), buildingProperties.OverrideFlatRoofMaterial);
        if (buildingProperties.OverrideFlatRoofMaterial)
            buildingProperties.FlatRoofMaterial = (Material)EditorGUILayout.ObjectField(nameof(buildingProperties.FlatRoofMaterial), buildingProperties.FlatRoofMaterial, typeof(Material), allowSceneObjects: false);

        buildingProperties.OverrideNonFlatRoofMaterial = EditorGUILayout.Toggle(nameof(buildingProperties.OverrideNonFlatRoofMaterial), buildingProperties.OverrideNonFlatRoofMaterial);
        if (buildingProperties.OverrideNonFlatRoofMaterial)
            buildingProperties.NonFlatRoofMaterial = (Material)EditorGUILayout.ObjectField(nameof(buildingProperties.NonFlatRoofMaterial), buildingProperties.NonFlatRoofMaterial, typeof(Material), allowSceneObjects: false);

        if (GUI.changed)
            EditorUtility.SetDirty(buildingProperties);
    }
}
