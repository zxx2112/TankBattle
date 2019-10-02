using UnityEditor;
using UnityEngine;

public static partial class SUI
{
    public static float LineHeight
    {
        get { return EditorGUIUtility.singleLineHeight; }
    }

    public static float VerticalSpace
    {
        get { return EditorGUIUtility.standardVerticalSpacing; }
    }

    public static float LabelWidth
    {
        get { return EditorGUIUtility.labelWidth; }
    }
}
