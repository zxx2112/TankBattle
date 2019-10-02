using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public partial class SmartInspector : EditorWindow
{
    //
    // ASSERT
    //

    bool IsNULL(object value)
    {
        return null == value;
    }

    bool IsNULL(params object[] args)
    {
        for (int i = 0; i < args.Length; i++)
        {
            if (null == args[i]) return true;
        }

        return false;
    }

    bool EditorNULL
    {
        get { return IsNULL(editor); }
    }

    bool IsSelectionNULL
    {
        get { return IsNULL(Selection.gameObjects) || Selection.gameObjects.Length == 0; }
    }
}
