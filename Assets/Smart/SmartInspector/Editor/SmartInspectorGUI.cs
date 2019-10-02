using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public partial class SmartInspector : EditorWindow
{
    //
    // GUI
    //

    void MainGUI()
    {
        if (SISettings.toggles)
        {
            OnToggleGUI();
        }
        else
        {
            DrawHeader();
            DrawMaskField();
            SUI.Space(SUI.VerticalSpace);
            DrawSearchBar();
            DrawEditors();
        }
    }

    //
    // Header
    //

    void DrawHeader()
    {
        if (!SISettings.DrawHeader)
        {
            SUI.Space(SUI.VerticalSpace * 2);

            return;
        }

        SUI.NoSpace();

        editor.DrawHeader();
    }

    //
    // Searchbar
    //

    void DrawSearchBar()
    {
        SUI.Horizontal(EditorStyles.inspectorFullWidthMargins, SearchBarLayout);
        SUI.Space(SUI.VerticalSpace);
    }

    void SearchBarLayout()
    {
        filter = GUILayout.TextField(filter, "ToolbarSeachTextField");
        SUI.Button(GUIContent.none, "ToolbarSeachCancelButton", ()=> filter = "");
    }

    //
    // Editors
    //

    Vector2 editorsScroll;

    void DrawEditors()
    {
        SplitFilter();
        SearchPropertyLabel();
        EditorGUIUtility.hierarchyMode = true;
        EditorGUIUtility.wideMode = true;
        editorsScroll = SUI.ScrollView(editorsScroll, EditorsScrollView, SUI.DontExpandHeight);
    }

    void EditorsScrollView()
    {
        for (int i = 0; i < editors.Length; i++)
        {
            if (IsNULL(editors)) { return; }

            if(propSearch)
            {
                if (SplitFilter(editors[i])) { continue; }
            }
            else
            {
                if (Filter(editors[i])) { continue; }
            }

            if (!IsVisible(editors[i])) { continue; }

            SUI.Vertical(() => DrawEditor(i));
        }
    }

    void DrawEditor(int i)
    {
        EditorExpanse expanse = DrawTitle(i);

        if (!expanse.IsExpanded) { return; }

        if (propSearch)
        {
            DrawEditorManually(i);
        }
        else
        {
            editors[i].OnInspectorGUI();
        }
    }

    void DrawEditorManually(int i)
    {
        // E x i t
        if (splitFilter.Length <= 0) { return; }
        // E x i t
        if (splitFilter.Length <= 1) { return; }

        SerializedProperty itr = editors[i].serializedObject.GetIterator();
        
        while (itr.NextVisible(true))
        {
            GUI.enabled = itr.name != "m_Script";
            
            if (SearchProperty(splitFilter[1], itr.name))
            {
                EditorGUILayout.BeginHorizontal("ProgressBarBar", SUI.DontExpandHeight);
                EditorGUILayout.PropertyField(itr, SUI.DontExpandHeight);
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.PropertyField(itr);
            }
            GUI.enabled = true;
        }
    }

    EditorExpanse DrawTitle(int i)
    {
        EditorExpanse expanse = new EditorExpanse(editors[i]);
        if (!SISettings.titles) { return expanse; }
        expanse.IsExpanded = SUI.Title(expanse.IsExpanded, editors[i]);
        return expanse;
    }
}
