using UnityEditor;
using UnityEngine;

public partial class SmartInspector : EditorWindow
{
    public static SmartInspector instance;

    private int visible;
    private string filter = "";
    private string[] contents;
    private readonly string ohFlags = "m_ObjectHideFlags";

    //
    // Unity
    //

    private void OnEnable()
    {
        if (null == instance) { instance = this; }

        GetEditors();

        SceneView.duringSceneGui += OnSceneView;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneView;
    }

    private void OnSelectionChange()
    {
        GetEditors();
    }

    private void OnGUI()
    {
        // E x i t
        if (IsSelectionNULL) { return; }
        // E x i t
        if (EditorNULL) { return; }
        
        editor.serializedObject.Update();

        ProcessEvent(Event.current);

        MainGUI();

        editor.serializedObject.ApplyModifiedProperties();
    }
}
