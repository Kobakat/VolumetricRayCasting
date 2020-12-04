using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RaymarchController))]

public class ControllerEditor : Editor
{
    public SerializedProperty
        #region Exposed
        shader_prop,
        darkMode_prop,
        useLighting_prop,
        #endregion

        #region Filter 
        emissiveColor_prop,
        highlightGradient_prop,
        nonHighlightStrength_prop,
        highlightStrength_prop,
        filter_prop,
        highlightType_prop,
        #endregion

        #region Lighting
        lightMode_prop,
        flipAngle_prop,
        litMultiplier_prop,
        unlitMultiplier_prop,
        customAngle_prop;
        #endregion

    void OnEnable()
    {
        #region Exposed
        this.shader_prop = serializedObject.FindProperty("_Shader");     
        this.darkMode_prop = serializedObject.FindProperty("darkMode");
        this.useLighting_prop = serializedObject.FindProperty("useLighting");
        #endregion

        #region Filter
        this.emissiveColor_prop = serializedObject.FindProperty("emissiveColor");
        this.highlightGradient_prop = serializedObject.FindProperty("highlightGradient");
        this.nonHighlightStrength_prop = serializedObject.FindProperty("nonHighlightStrength");
        this.highlightStrength_prop = serializedObject.FindProperty("highlightStrength");
        this.filter_prop = serializedObject.FindProperty("filter");
        this.highlightType_prop = serializedObject.FindProperty("highlightType");
        #endregion

        #region Lighting
        this.lightMode_prop = serializedObject.FindProperty("lightMode");
        this.flipAngle_prop = serializedObject.FindProperty("flipAngle");
        this.litMultiplier_prop = serializedObject.FindProperty("litMultiplier");
        this.unlitMultiplier_prop = serializedObject.FindProperty("unlitMultiplier");
        this.customAngle_prop = serializedObject.FindProperty("customAngle");
        #endregion
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        #region Exposed

        EditorGUILayout.PropertyField(shader_prop);
        EditorGUILayout.PropertyField(useLighting_prop, new GUIContent("Light shapes"));
        EditorGUILayout.PropertyField(darkMode_prop, new GUIContent("Dark mode"));

        #endregion

        #region Filter
        EditorGUILayout.PropertyField(filter_prop);
  
        RaymarchController.Filter st = (RaymarchController.Filter)filter_prop.enumValueIndex;

        switch(st)
        {
            case RaymarchController.Filter.Highlight:
                DisplayHighlightVariables();                  
                break;

            default:
                break;
        }

        #endregion

        #region Lighting
        EditorGUILayout.PropertyField(lightMode_prop);

        DisplayLightVariables();
        #endregion
        serializedObject.ApplyModifiedProperties();
    }



    void DisplayHighlightVariables()
    {
        EditorGUILayout.PropertyField(nonHighlightStrength_prop, new GUIContent("Base Strength"));
        EditorGUILayout.PropertyField(highlightStrength_prop, new GUIContent("Highlight Strength"));
        EditorGUILayout.PropertyField(highlightGradient_prop, new GUIContent("Gradient Strength"));

        EditorGUILayout.PropertyField(highlightType_prop);
        RaymarchController.HighlightType st = (RaymarchController.HighlightType)highlightType_prop.enumValueIndex;

        switch (st)
        {
            case RaymarchController.HighlightType.ShapeColor:
                break;
            case RaymarchController.HighlightType.SingleColor:
                EditorGUILayout.PropertyField(emissiveColor_prop);
                break;
        }
    }

    void DisplayLightVariables()
    {
        RaymarchController.LightMode lt = (RaymarchController.LightMode)lightMode_prop.enumValueIndex;

        switch (lt)
        {
            case RaymarchController.LightMode.Lambertian:
                break;
            case RaymarchController.LightMode.CelShaded:
                EditorGUILayout.PropertyField(litMultiplier_prop, new GUIContent("Lit Multiplier"));
                EditorGUILayout.PropertyField(unlitMultiplier_prop, new GUIContent("Unlit multiplier"));
                EditorGUILayout.PropertyField(customAngle_prop, new GUIContent("Use Custom Angle"));

                if (customAngle_prop.boolValue)
                    EditorGUILayout.PropertyField(flipAngle_prop, new GUIContent("Custom angle"));
                break;
        }
    }
}
