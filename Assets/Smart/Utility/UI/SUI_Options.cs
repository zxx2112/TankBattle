using UnityEditor;
using UnityEngine;

public static partial class SUI
{
    public static GUILayoutOption ExpandWidth
    {
        get { return GUILayout.ExpandWidth(true); }
    }

    public static GUILayoutOption DontExpandWidth
    {
        get { return GUILayout.ExpandWidth(false); }
    }

    public static GUILayoutOption ExpandHeight
    {
        get { return GUILayout.ExpandHeight(true); }
    }

    public static GUILayoutOption DontExpandHeight
    {
        get { return GUILayout.ExpandHeight(false); }
    }

    public static GUILayoutOption Width(float width)
    {
        return GUILayout.Width(width);
    }

    public static GUILayoutOption Height(float height)
    {
        return GUILayout.Height(height);
    }

    public static GUILayoutOption MaxHeight(float height)
    {
        return GUILayout.MaxHeight(height);
    }

    public static GUILayoutOption MinHeight(float height)
    {
        return GUILayout.MinHeight(height);
    }

    public static GUILayoutOption MaxWidth(float width)
    {
        return GUILayout.MaxWidth(width);
    }

    public static GUILayoutOption MinWidth(float width)
    {
        return GUILayout.MinWidth(width);
    }
}
