using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RaymarchController))]

public class ControllerEditor : Editor
{
    public SerializedProperty
        shader_prop,
        emissiveColor_prop,
        darkMode_prop,
        useLighting_prop,
        highlightGradient_prop,
        nonHighlightStrength_prop,
        highlightStrength_prop,
        filter_prop,
        highlightType_prop;

    void OnEnable()
    {
        this.shader_prop = serializedObject.FindProperty("_Shader");
        this.emissiveColor_prop = serializedObject.FindProperty("emissiveColor");
        this.darkMode_prop = serializedObject.FindProperty("darkMode");
        this.useLighting_prop = serializedObject.FindProperty("useLighting");
        this.highlightGradient_prop = serializedObject.FindProperty("highlightGradient");
        this.nonHighlightStrength_prop = serializedObject.FindProperty("nonHighlightStrength");
        this.highlightStrength_prop = serializedObject.FindProperty("highlightStrength");
        this.filter_prop = serializedObject.FindProperty("filter");
        this.highlightType_prop = serializedObject.FindProperty("highlightType");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(shader_prop);
        EditorGUILayout.PropertyField(useLighting_prop, new GUIContent("Light shapes"));
        EditorGUILayout.PropertyField(darkMode_prop, new GUIContent("Dark mode"));

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
}
