using System;
using UnityEditor;

public partial class SUI
{
    //
    // GUIChanged
    //

    public static void GUIChanged(Action gui, Action change)
    {
        EditorGUI.BeginChangeCheck();
        gui();
        if (EditorGUI.EndChangeCheck())
        {
            change();
        }
    }
}
