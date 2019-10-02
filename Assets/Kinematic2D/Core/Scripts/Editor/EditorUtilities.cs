using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


public static class EditorUtilities
{
	public static void AbstractPropertyDrawer(SerializedProperty property)
	{
		// if(property == null)
		// 	throw new System.ArgumentNullException ("Null Reference Exception");

		// if(property.propertyType == SerializedPropertyType.ObjectReference)
		// {
		// 	if(property.objectReferenceValue == null)
		// 	{
		// 		//field is null, provide object field for user to insert instance to draw
		// 		EditorGUILayout.PropertyField(property);
		// 		return;
		// 	}
		// 	System.Type concreteType = property.objectReferenceValue.GetType();
		// 	UnityEngine.Object castedObject = (UnityEngine.Object) System.Convert.ChangeType(property.objectReferenceValue,concreteType);

		// 	UnityEditor.Editor editor= UnityEditor.Editor.CreateEditor(castedObject);

		// 	editor.OnInspectorGUI();
		// }
		// else
		// {
		// 	//otherwise fallback to normal property field
		// 	EditorGUILayout.PropertyField(property);
		// }
	}

}


