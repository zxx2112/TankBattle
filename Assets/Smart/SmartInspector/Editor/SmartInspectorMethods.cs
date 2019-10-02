using System;
using UnityEditor;
using UnityEngine;

public partial class SmartInspector : EditorWindow
{
    //
    // Methods
    //

    [MenuItem("Tools/Smart/Inspector")]
    public static SmartInspector Open()
    {
        if (instance) { instance.Show(); return instance; }

        instance = GetWindow<SmartInspector>("Smart Inspector");

        return instance;
    }

    void OnSceneView(SceneView view)
    {
        Repaint();
    }

    void DrawToggle(int i, Editor value)
    {
        value.serializedObject.Update();

        Component component = value.target as Component;

        bool isVisible = IsVisible(value);

        SUI.GUIChanged  (() =>
        {
            GUIContent content = SUI.Content(component);

            content.text = string.Format("{0} - {1}", i, component.GetType().Name);

            isVisible = SUI.Toggle(isVisible, content, "Radio", SUI.Height(SUI.LineHeight));
        },
        () =>
        {
            SetVisible(value, isVisible);
        });

        value.serializedObject.ApplyModifiedProperties();

        editor.Repaint();

        Repaint();
    }

    void Toggle(Editor editor, bool value)
    {
        editor.serializedObject.Update();

        Component component = editor.target as Component;
        
        SetVisible(editor, value);

        editor.serializedObject.ApplyModifiedProperties();

        this.editor.Repaint();

        Repaint();
    }

    void Toggle(int i)
    {
        Editor value = editors[i];

        value.serializedObject.Update();

        Component component = value.target as Component;

        SetVisible(value, visible == (visible | (1 << i)));

        value.serializedObject.ApplyModifiedProperties();

        editor.Repaint();

        Repaint();
    }

    //
    // Hide Flags
    //

    int GetHideFlags(Editor editorComponent)
    {
        SerializedObject so = editorComponent.serializedObject;

        return so.Get(ohFlags).intValue;
    }

    void SetHideFlag(Editor editorComponent, int value)
    {
        SerializedObject so = editorComponent.serializedObject;

        so.Get(ohFlags).intValue = value;
    }

    //
    // Visibility
    //

    void SetVisible(Editor editorComponent, bool value)
    {
        int intVal = (int)(value ? HideFlags.None : HideFlags.HideInInspector);

        SetHideFlag(editorComponent, intVal);
    }

    bool IsVisible(Editor editorComponent)
    {
        HideFlags flag = (HideFlags)GetHideFlags(editorComponent);

        return flag == HideFlags.None;
    }

    void UpdateVisibility()
    {
        for (int i = 0; i < editors.Length; i++)
        {
            bool value = visible == (visible | (1 << i));

            Toggle(editors[i], value);
        }
    }

    void UpdateVisibility(bool value)
    {
        for (int i = 0; i < editors.Length; i++)
        {
            if (IsNULL(editors)) { return; }

            editors[i].serializedObject.Update();
            SetVisible(editors[i], value);
            editors[i].serializedObject.ApplyModifiedProperties();
        }
    }

    bool Filter(Editor editor)
    {
        if(filter == "") { return false; }

        string name = editor.target.GetType().Name;
        name = name.ToLower();

        return !name.Contains(filter.ToLower());
    }

    bool SplitFilter(Editor editor)
    {
        if(splitFilter.Length <= 0) { return false; }

        if (splitFilter[0] == "") { return false; }

        string name = editor.target.GetType().Name;
        name = name.ToLower();

        return !name.Contains(splitFilter[0].ToLower());
    }

    bool SearchProperty(string search, string target)
    {
        if (string.IsNullOrWhiteSpace(search)) { return false; }

        target = target.ToLower();

        return target.Contains(search.ToLower());
    }

    //
    // Property Search
    //

    bool propSearch = false;
    string[] splitFilter = new string[0];

    void SplitFilter()
    {
        // Split filter
        splitFilter = filter.Split('.');
        // Flag
        propSearch = splitFilter.Length > 1;
    }

    void SearchPropertyLabel()
    {
        // E x i t
        if (splitFilter.Length <= 1) { return; }
        // Label
        SUI.Space(SUI.VerticalSpace);
        EditorGUILayout.BeginVertical(EditorStyles.inspectorFullWidthMargins);
        SUI.Label(string.Format("(Beta) Property Search: {0}", splitFilter[1]), "assetlabel");
        EditorGUILayout.EndVertical();
    }
}
