using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnyObj = UnityEngine.Object;

public static partial class SUI
{

    //
    // Scroll View
    //

    public static Vector2 ScrollView(Vector2 scroll, Action action)
    {
        scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.ExpandHeight(false));
        {
            action();
        }
        EditorGUILayout.EndScrollView();

        return scroll;
    }

    public static Vector2 ScrollView(Vector2 scroll, string style, Action action)
    {
        scroll = EditorGUILayout.BeginScrollView(scroll, style);
        {
            action();
        }
        EditorGUILayout.EndScrollView();

        return scroll;
    }

    public static Vector2 ScrollView(Vector2 scroll, GUIStyle style, Action action)
    {
        scroll = EditorGUILayout.BeginScrollView(scroll, style);
        {
            action();
        }
        EditorGUILayout.EndScrollView();

        return scroll;
    }

    public static Vector2 ScrollView(Vector2 scroll, Action action, params GUILayoutOption[] options)
    {
        scroll = EditorGUILayout.BeginScrollView(scroll, options);
        {
            action();
        }
        EditorGUILayout.EndScrollView();

        return scroll;
    }

    public static Vector2 ScrollView(Vector2 scroll, string style, Action action, params GUILayoutOption[] options)
    {
        scroll = EditorGUILayout.BeginScrollView(scroll, style, options);
        {
            action();
        }
        EditorGUILayout.EndScrollView();

        return scroll;
    }

    //
    // Vertical
    //

    public static void Vertical(Action content)
    {
        EditorGUILayout.BeginVertical();
        {
            content();
        }
        EditorGUILayout.EndVertical();
    }

    public static void Vertical(Action content, params GUILayoutOption[] options)
    {
        EditorGUILayout.BeginVertical(options);
        {
            content();
        }
        EditorGUILayout.EndVertical();
    }

    public static void Vertical(GUIStyle style, Action content)
    {
        EditorGUILayout.BeginVertical(style);
        {
            content();
        }
        EditorGUILayout.EndVertical();
    }

    public static void Vertical(string style, Action content)
    {
        EditorGUILayout.BeginVertical(style);
        {
            content();
        }
        EditorGUILayout.EndVertical();
    }

    public static void Vertical(string style, Action content, params GUILayoutOption[] options)
    {
        EditorGUILayout.BeginVertical(style, options);
        {
            content();
        }
        EditorGUILayout.EndVertical();
    }

    //
    // Horizontal
    //

    public static void Horizontal(Action content)
    {
        EditorGUILayout.BeginHorizontal();
        {
            content();
        }
        EditorGUILayout.EndHorizontal();
    }

    public static void Horizontal(Action content, params GUILayoutOption[] options)
    {
        EditorGUILayout.BeginHorizontal(options);
        {
            content();
        }
        EditorGUILayout.EndHorizontal();
    }

    public static void Horizontal(GUIStyle style, Action content)
    {
        EditorGUILayout.BeginHorizontal(style);
        {
            content();
        }
        EditorGUILayout.EndHorizontal();
    }

    public static void Horizontal(string style, Action content)
    {
        EditorGUILayout.BeginHorizontal(style);
        {
            content();
        }
        EditorGUILayout.EndHorizontal();
    }

    public static void Horizontal(string style, Action content, params GUILayoutOption[] options)
    {
        EditorGUILayout.BeginHorizontal(style, options);
        {
            content();
        }
        EditorGUILayout.EndHorizontal();
    }

    //
    // VectorField
    //

    public static Vector3 VectorField(Vector3 value)
    {
        return EditorGUILayout.Vector3Field(GUIContent.none, value);
    }

    public static Vector3 VectorCustom(Vector3 value, string style)
    {
        Horizontal(() =>
        {
            value.x = EditorGUILayout.FloatField(value.x, style);
            value.y = EditorGUILayout.FloatField(value.y, style);
            value.z = EditorGUILayout.FloatField(value.z, style);
        });

        return value;
    }

    public static Vector3 VectorCustom(Vector3 value)
    {
        Horizontal(() =>
        {
            value.x = EditorGUILayout.FloatField(value.x);
            value.y = EditorGUILayout.FloatField(value.y);
            value.z = EditorGUILayout.FloatField(value.z);
        });

        return value;
    }

    //
    // Enum
    //

    public static Enum EnumField(Enum value)
    {
        return EditorGUILayout.EnumPopup(GUIContent.none, value);
    }
    public static Enum EnumField(Enum value, string style)
    {
        return EditorGUILayout.EnumPopup(GUIContent.none, value, style);
    }

    //
    // Layer
    //

    public static LayerMask LayerField(LayerMask layer, GUIStyle style)
    {
        int lval = InternalEditorUtility.LayerMaskToConcatenatedLayersMask(layer);

        LayerMask ltmp = EditorGUILayout.MaskField(lval, InternalEditorUtility.layers, style);

        return InternalEditorUtility.ConcatenatedLayersMaskToLayerMask(ltmp.value);
    }

    //
    // Field
    //

    public static void Field(string label, Action action)
    {
        Horizontal(() =>
        {
            Label(label, Width(LabelWidth));

            action();
        });
    }

    //
    // Misc
    //

    public static GUIContent Content(UnyObj obj)
    {
        return EditorGUIUtility.ObjectContent(obj, obj.GetType());
    }
    public static GUIContent Content(UnyObj obj, Type type)
    {
        return EditorGUIUtility.ObjectContent(obj, type);
    }
    public static bool Title(bool isExpanded, Editor editor)
    {
        return EditorGUILayout.InspectorTitlebar(isExpanded, editor);
    }
    public static bool Title(bool isExpanded, UnyObj obj)
    {
        return EditorGUILayout.InspectorTitlebar(isExpanded, obj);
    }

    //
    // Object Field
    //

    public static T ObjectField<T>(T value, bool allowSceneObjects) where T : UnyObj
    {
        return (T)EditorGUILayout.ObjectField(value, typeof(T), allowSceneObjects);
    }
}
