using UnityEditor;
using UnityEngine;

public static class SISettings
{
    //
    // Draw Header
    //

    public static readonly string drawHeaderKey = "SW_DrawHeader";
    private static readonly bool defaultDrawHeader = true;

    public static bool DrawHeader
    {
        get { return EditorPrefs.GetBool(drawHeaderKey, defaultDrawHeader); }
        set { EditorPrefs.SetBool(drawHeaderKey, value); }
    }

    //
    // Draw Inspector
    //

    public static readonly string titlesKey = "SW_DrawInspectors";
    private static readonly bool defaultTitles = true;

    public static bool titles
    {
        get { return EditorPrefs.GetBool(titlesKey, defaultTitles); }
        set { EditorPrefs.SetBool(titlesKey, value); }
    }

    //
    // Draw Inspector
    //

    public static readonly string toggleKey = "SW_Toggles";
    private static readonly bool defaultToggles = false;

    public static bool toggles
    {
        get { return EditorPrefs.GetBool(toggleKey, defaultToggles); }
        set { EditorPrefs.SetBool(toggleKey, value); }
    }
}
