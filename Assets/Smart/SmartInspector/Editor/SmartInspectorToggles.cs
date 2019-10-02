using UnityEditor;
using UnityEngine;

public partial class SmartInspector : EditorWindow
{
    //
    // Toggles
    //

    Vector2 toggleScroll;

    void OnToggleGUI()
    {
        SUI.Vertical(DrawToggles);
    }

    void DrawTogglesHeader()
    {
        if (GUILayout.Button("Toggle", "PreButton"))
        {
            bool isVisible = false;

            for (int i = 0; i < editors.Length; i++)
            {
                if (IsNULL(editors)) { return; }
                editors[i].serializedObject.Update();

                isVisible = IsVisible(editors[i]);
                if (isVisible) { visible |= (1 << i); }
                SetVisible(editors[i], !isVisible);
                editors[i].serializedObject.ApplyModifiedProperties();
            }
        }

        if (GUILayout.Button("On", "PreButton"))
        {
            UpdateVisibility(true);
        }

        if (GUILayout.Button("Off", "PreButton"))
        {
            UpdateVisibility(false);
        }
    }

    void DrawToggles()
    {
        SUI.Horizontal(DrawTogglesHeader);

        SUI.Space(SUI.VerticalSpace);

        DrawSearchBar();

        SUI.Label("Components");

        toggleScroll = SUI.ScrollView(toggleScroll, EditorStyles.inspectorFullWidthMargins, DrawTogglesScroll);
    }

    void DrawTogglesScroll()
    {
        for (int i = 0; i < editors.Length; i++)
        {
            if (Filter(editors[i])) { continue; }

            DrawToggle(i, editors[i]);
        }
    }
}
