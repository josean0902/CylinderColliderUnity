using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CylinderCollider))]
public class CylinderColliderEditor : Editor
{
    private SerializedProperty autoRebuild;

    private void OnEnable()
    {
        autoRebuild = serializedObject.FindProperty("autoRebuild");
        Undo.undoRedoPerformed += OnUndoRedo;
    }


    private void OnDisable()
    {
        Undo.undoRedoPerformed -= OnUndoRedo;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUI.BeginChangeCheck();

        DrawDefaultInspector();

        GUILayout.Space(10);
        EditorGUILayout.LabelField("Cylinder Collider Tools", EditorStyles.boldLabel);

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Build Colliders"))
                Build();

            if (GUILayout.Button("Clean Colliders"))
                Clean();
        }

        GUILayout.Space(5);
        EditorGUILayout.PropertyField(autoRebuild, new GUIContent("Auto Rebuild"));

        bool changed = EditorGUI.EndChangeCheck();

        serializedObject.ApplyModifiedProperties();

        if (changed && autoRebuild.boolValue)
            Build();
    }

    private void Build()
    {
        CylinderCollider cylinderCollider = (CylinderCollider)target;
        Undo.RecordObject(cylinderCollider.gameObject, "Build Cylinder Colliders");
        cylinderCollider.BuildCollider();
        EditorUtility.SetDirty(cylinderCollider);
    }

    private void Clean()
    {
        autoRebuild.boolValue = false;

        CylinderCollider cylinderCollider = (CylinderCollider)target;
        Undo.RecordObject(cylinderCollider.gameObject, "Clean Cylinder Colliders");
        cylinderCollider.CleanCollider();
        EditorUtility.SetDirty(cylinderCollider);
    }

    private void OnUndoRedo()
    {
        string undoActionName = Undo.GetCurrentGroupName();

        if ((undoActionName == "Build Cylinder Colliders" ||
            undoActionName == "Clean Cylinder Colliders")
            && autoRebuild.boolValue)
            Build();

    }

}