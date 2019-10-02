using System;
using UnityEditor;
using UnityEngine;

public partial class SUI
{
    //
    // Space
    //

    public static void Space(float pixels)
    {
        GUILayout.Space(pixels);
    }

    public static void NoSpace()
    {
        Space(0);
    }
    
    //
    // Button (GUIcontent)
    //

    public static void Button(GUIContent label, Action action)
    {
        if (GUILayout.Button(label)) { action(); }
    }

    public static void Button(GUIContent label, Action action, params GUILayoutOption[] options)
    {
        if (GUILayout.Button(label, options)) { action(); }
    }

    public static void Button(GUIContent label, string style, Action action)
    {
        if (GUILayout.Button(label, style)) { action(); }
    }

    public static void Button(GUIContent label, string style, Action action, params GUILayoutOption[] options)
    {
        if (GUILayout.Button(label, style, options)) { action(); }
    }

    //
    // Button (string label)
    //

    public static void Button(string label, Action action)
    {
        if (GUILayout.Button(label)) { action(); }
    }

    public static void Button(string label, string style, Action action)
    {
        if (GUILayout.Button(label, style)) { action(); }
    }

    public static void Button(string label, string style, Action action, params GUILayoutOption[] options)
    {
        if (GUILayout.Button(label, style, options)) { action(); }
    }

    public static void Button(string label, GUIStyle style, Action action)
    {
        if (GUILayout.Button(label, style)) { action(); }
    }

    //
    // Label
    //
    
    public static void Label(string label)
    {
        GUILayout.Label(label);
    }

    public static void Label(GUIContent content)
    {
        GUILayout.Label(content);
    }

    public static void Label(GUIContent content, params GUILayoutOption[] options)
    {
        GUILayout.Label(content, options);
    }

    public static void Label(GUIContent content, string style, params GUILayoutOption[] options)
    {
        GUILayout.Label(content, style, options);
    }

    public static void Label(string label, string style)
    {
        GUILayout.Label(label, style);
    }

    public static void Label(string label, params GUILayoutOption[] options)
    {
        GUILayout.Label(label, options);
    }

    public static void Label(string label, string style, params GUILayoutOption[] options)
    {
        GUILayout.Label(label, style, options);
    }

    //
    // Toggle
    //

    public static bool Toggle(bool value)
    {
        return GUILayout.Toggle(value, GUIContent.none);
    }

    public static bool Toggle(bool value, string label)
    {
        return GUILayout.Toggle(value, label);
    }

    public static bool Toggle(bool value, string label, string style)
    {
        return GUILayout.Toggle(value, label, style);
    }

    public static bool Toggle(bool value, GUIContent content, params GUILayoutOption[] options)
    {
        return GUILayout.Toggle(value, content, options);
    }

    public static bool Toggle(bool value, GUIContent content, string style, params GUILayoutOption[] options)
    {
        return GUILayout.Toggle(value, content, style, options);
    }

    //
    // Flexible Space
    //

    public static void FlexibleSpace()
    {
        GUILayout.FlexibleSpace();
    }
}
