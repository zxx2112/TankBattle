using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public partial class SmartInspector
{
    // Main
    void DrawMaskField()
    {
        SUI.GUIChanged(MaskFieldVertical, UpdateVisibility);
    }

    // Vertical area
    void MaskFieldVertical()
    {
        SUI.Horizontal(EditorStyles.inspectorFullWidthMargins, MaskFieldLayout);
    }

    // Layout
    void MaskFieldLayout()
    {
        visible = EditorGUILayout.MaskField(GUIContent.none, visible, contents, "ExposablePopupMenu");
    }
}
