using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Type = System.Type;

public partial class SmartInspector : EditorWindow
{
    private Editor editor;
    private Editor[] editors;

    //
    // Selection NULL
    //

    bool SelectionNULL()
    {
        if (IsSelectionNULL) { return true; }

        editor = null;
        editors = null;

        Repaint();

        return false;
    }

    void GetEditors()
    {
        if (SelectionNULL()) { return; }
        // Create an editor with the gameObjects Selected
        editor = Editor.CreateEditor(Selection.gameObjects);
        // Exit if editor is null
        if (EditorNULL) { Repaint(); return; }
        // Get the component array from each target
        SerializedProperty[] components = GetComponentArray(editor.targets);
        // Get editor keylist
        Keylist<Type, Object> keylist = GetEditorKeylist(components);
        // Create editors
        CreateEditors(keylist);
    }

    SerializedProperty[] GetComponentArray(Object[] targets)
    {
        // Current serialized object per target
        SerializedObject so = null;
        // Current component array
        SerializedProperty property = null;
        // The result
        List<SerializedProperty> result = new List<SerializedProperty>();

        for (int i = 0; i < targets.Length; i++)
        {
            // Create serialized object from target
            so = new SerializedObject(targets[i]);
            // Get the component array from a single target
            property = so.Get("m_Component");

            result.AddRange(GetComponents(property));
        }

        return result.ToArray();
    }

    SerializedProperty[] GetComponents(SerializedProperty value)
    {
        // Current component property
        SerializedProperty property = null;
        // The result
        List<SerializedProperty> result = new List<SerializedProperty>();

        for (int i = 0; i < value.arraySize; i++)
        {
            // Get the component
            property = value.Child(i).Get("component");
            // Add component as property
            result.Add(property);
        }

        return result.ToArray();
    }

    Keylist<Type, Object> GetEditorKeylist(SerializedProperty[] components)
    {
        // We are going to organize components by type using a Keylist
        Keylist<Type, Object> keylist = new Keylist<Type, Object>();
        // Current type
        Type type = null;
        // Current object reference value
        Object value = null;

        for (int i = 0; i < components.Length; i++)
        {
            // Get object value
            value = components[i].objectReferenceValue;
            // Get type
            type = value.GetType();
            // Add to key list
            keylist.Add(type, value);
        }

        return keylist;
    }

    void CreateEditors(Keylist<Type, Object> keylist)
    {
        // Each editor contains targets which are an array of components by type from different GameObjects
        editors = new Editor[keylist.KeyCount];
        // Create content from key types
        contents = new string[keylist.KeyCount];

        for(int i = 0; i < editors.Length; i++)
        {
            // Create editor from array of components of the same type
            editors[i] = Editor.CreateEditor(keylist.Values[i].ToArray());
            // Get name
            contents[i] = keylist[i].Name;
        }
    }

    //
    // Classes
    //

    public class EditorExpanse
    {
        public Editor editor;
        private readonly string ohFlags = "m_ObjectHideFlags";

        public bool IsExpanded
        {
            get { return GetExpandedValue(); }
            set { SetExpandedValue(value); }
        }

        public EditorExpanse(Editor editor)
        {
            this.editor = editor;
        }

        bool GetExpandedValue()
        {
            //return InternalEditorUtility.GetIsInspectorExpanded(editor.target);
            return editor.GetProperty(ohFlags).isExpanded;
        }

        void SetExpandedValue(bool value)
        {
            //InternalEditorUtility.SetIsInspectorExpanded(editor, value);
            editor.GetProperty(ohFlags).isExpanded = value;
        }
    }
}
