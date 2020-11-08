using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Operation)), CanEditMultipleObjects]
public class OperationEditor : Editor
{
    public SerializedProperty
        operation_Prop,
        blendStrength_Prop;

    void OnEnable()
    {
        this.operation_Prop = serializedObject.FindProperty("operation");
        this.blendStrength_Prop = serializedObject.FindProperty("blendStrength");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(operation_Prop);

        Operation.OpFunction st = (Operation.OpFunction)operation_Prop.enumValueIndex;

        switch(st)
        {
            case Operation.OpFunction.None:
            case Operation.OpFunction.Subtract:
            case Operation.OpFunction.Intersect:           
                break;
            case Operation.OpFunction.Blend:
                EditorGUILayout.PropertyField(blendStrength_Prop, new GUIContent("BlendStrength"));
                break;
        }

        serializedObject.ApplyModifiedProperties();
    }
}
