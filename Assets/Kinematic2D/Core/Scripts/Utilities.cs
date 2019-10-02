using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
	using UnityEditor;
#endif

namespace Lightbug.CoreUtilities
{

public static class Utilities
{
	
	public static bool BelongsToLayerMask(int layer , int layerMask)
	{		
		return ( layerMask & (1 << layer) ) > 0;
	}


	/// <summary>
	/// Returns true if the target value is between a and b ( both exclusive ). 
	/// To include the limits values set the \"inclusive\" bool to true.
	/// </summary>
	public static bool isBetween( float target, float a , float b , bool inclusive = false )
	{
		if( a == b )
			return inclusive ? true : false;			

		if( b > a )
			return ( inclusive ? target >= a : target > a ) && ( inclusive ? target <= b : target < b );
		else
			return ( inclusive ? target >= b : target > b ) && ( inclusive ? target <= a : target < a );
	}

	/// <summary>
	/// Returns true if the target value is between a and b ( both exclusive ). 
	/// To include the limits values set the \"inclusive\" bool to true.
	/// </summary>
	public static bool isBetween( int target, int a , int b , bool inclusive = false )
	{
		if( a == b )
			return inclusive ? true : false;			

		if( b > a )
			return ( inclusive ? target >= a : target > a ) && ( inclusive ? target <= b : target < b );
		else
			return ( inclusive ? target >= b : target > b ) && ( inclusive ? target <= a : target < a );
		
	}
	

	public static bool isCloseTo( Vector3 input , Vector3 target , float tolerance )
	{
		return Vector3.Distance(input, target) <= tolerance;
		
	}

	public static bool isCloseTo( float input , float target , float tolerance )
	{
		return Mathf.Abs(target - input) <= tolerance;
	}

	public static Vector2 worldToLocal( Vector2 input , Transform transform )
	{
		float angle = Utilities.SignedAngle( transform.up , Vector2.up , Vector3.forward );
		Quaternion rotation = Quaternion.AngleAxis( - angle , Vector3.forward );
		return rotation * input;
	}

	public static Vector2 localToWorld( Vector2 input , Transform transform )
	{
		float angle = Utilities.SignedAngle( Vector2.up , transform.up , Vector3.forward );
		Quaternion rotation = Quaternion.AngleAxis( angle , Vector3.forward );
		return rotation * input;
	}
	

	
	public static System.Type[] GetAllDerivedObjects(System.Type targetType)
	{
		System.AppDomain aAppDomain = System.AppDomain.CurrentDomain;

		List<System.Type> result = new List<System.Type>();
		

		var assemblies = aAppDomain.GetAssemblies();
		foreach (var assembly in assemblies)
		{
			var types = assembly.GetTypes();
			foreach (var type in types)
			{
				if ( type.IsSubclassOf(targetType) )
					result.Add( type );
			}
		}
		return result.ToArray();
	}

	public static T GetOrAddComponent<T>( this GameObject targetGameObject ) where T : Component
	{
		T component = targetGameObject.GetComponent<T>();		
		if( component == null )
			component = targetGameObject.AddComponent<T>();
		
		return component;
	}

	public static T AddComponentOverride<T>( this GameObject targetGameObject ) where T : Component
	{
		T existingComponent = targetGameObject.GetComponent<T>();
		if( existingComponent != null )
			GameObject.Destroy( existingComponent );
		
		T component = targetGameObject.AddComponent<T>();
		
		return component;
	}

	public static bool IsNullOrEmpty( this string target )
	{
		return target == null || target.Length == 0;
	}

	public static bool IsNullOrWhiteSpace( this string target )
	{
		if( target == null )
			return true;

		for(int i = 0 ; i < target.Length ; i++ )
		{
			if( target[i] != ' ' )
				return false;
		}		
		
		return true;
	}

	#if UNITY_EDITOR
	public static void DrawColoredRect( Rect rect , Color color)
     {
          GUI.color = color;
          GUI.DrawTexture ( rect , EditorGUIUtility.whiteTexture);
          GUI.color = Color.white;
     }

	public static void DrawArrowGizmo( Vector3 start , Vector3 end , Color color , float radius = 0.25f )
	{
		Gizmos.color = color;
		Gizmos.DrawLine( start , end );
		
		Gizmos.DrawRay( 
			end , 
			Quaternion.AngleAxis( 45 , Vector3.forward ) * ( start - end ).normalized * radius
		);

		Gizmos.DrawRay( 
			end , 
			Quaternion.AngleAxis( -45 , Vector3.forward ) * ( start - end ).normalized * radius
		);
	}
	
	public static void DrawArrowHead( Vector3 start , Vector3 end , Color color , float radius = 20f , float angle = 45f )
	{
		Handles.color = color;
		
		Vector3[] points = new Vector3[]
		{
			end  + Quaternion.AngleAxis( angle , Vector3.forward ) * ( start - end ).normalized * radius ,
			end , 
			end  + Quaternion.AngleAxis( - angle  , Vector3.forward ) * ( start - end ).normalized * radius
		};

		Handles.DrawAAPolyLine( points );
	}

	public static void DrawCross( Vector3 point , float radius , Color color )
	{
		Gizmos.color = color;		
		
		Gizmos.DrawRay( 
			point + Vector3.up * 0.5f * radius , 
			Vector3.down * radius
		);

		Gizmos.DrawRay( 
			point + Vector3.right * 0.5f * radius , 
			Vector3.left * radius
		);
	}

	#endif
	
	public static float SignedAngle( Vector3 from , Vector3 to , Vector3 axis )
	{
		float angle = Vector3.Angle( from , to );
		Vector3 cross = Vector3.Cross( from , to ).normalized;
		float sign = cross == axis ? 1f : -1f;

		return sign * angle;
		
	}

	#if UNITY_EDITOR

	public static void AbstractPropertyDrawer(this SerializedProperty property)
	{
		if(property == null)
			throw new ArgumentNullException ("Null Property");

		if(property.propertyType == SerializedPropertyType.ObjectReference)
		{
			if(property.objectReferenceValue == null)
			{
				EditorGUILayout.PropertyField(property);
				return;
			}

			System.Type concreteType = property.objectReferenceValue.GetType();
			UnityEngine.Object castedObject = (UnityEngine.Object) System.Convert.ChangeType(property.objectReferenceValue,concreteType);

			UnityEditor.Editor editor= UnityEditor.Editor.CreateEditor(castedObject);

			editor.OnInspectorGUI();
		}
		else
		{
			EditorGUILayout.PropertyField(property);
		}
	}

	public static void DrawLabel( ref Rect rect , string label , GUIStyle style = null )
	{
		if( style != null )
			GUI.Label( rect , label , style );
		else
			GUI.Label( rect , label );

		rect.y += rect.height;
	}

	public static void DrawTitle( ref Rect rect , string title )
	{
		GUI.Label( rect , title , EditorStyles.boldLabel );
		rect.y += rect.height;
	}

	public static void GUISpace( ref Rect rect , int numberOfHalfLines = 1 )
	{
		rect.y += numberOfHalfLines * rect.height / 2;
	}

	public static void DrawTitleWithToggle( ref Rect rect , GUIContent title , SerializedProperty toggleProperty )
	{
		
		Rect rectA = rect;

		Rect rectB = rect;		
		rectB.width = 10f;
		rectB.x = rect.width - rectB.width;

		GUI.Label( rectA , title , EditorStyles.boldLabel );
		EditorGUI.PropertyField( rectB , toggleProperty , GUIContent.none );

		rect.y += rect.height;
	}

	public static void DrawProperty( ref Rect rect , SerializedProperty property , bool includeChildren = false , string label = null )
	{
		if( label != null )
			EditorGUI.PropertyField( rect , property , new GUIContent( label ) , includeChildren );
		else
			EditorGUI.PropertyField( rect , property , includeChildren );
		
		rect.y += EditorGUI.GetPropertyHeight( property );
	}

	public static void DrawProperties( ref Rect rect , string labelA , SerializedProperty propertyA , string labelB , SerializedProperty propertyB , float spaceRatio )
	{
		spaceRatio = Mathf.Clamp01( spaceRatio );

		Rect rectA = rect;
		rectA.width *= spaceRatio;

		Rect rectB = rect;
		rectB.x += rectA.width;
		rectB.width *= ( 1 - spaceRatio );

		EditorGUI.PropertyField( rectA , propertyA , new GUIContent( labelA ) );
		EditorGUI.PropertyField( rectB , propertyB , new GUIContent( labelB ) );

		rect.y += rect.height;
	}

	public static void DrawProperties( ref Rect rect , params SerializedProperty[] properties )
	{
		Rect dividedRect = rect;
		
		dividedRect.width /= properties.Length;

		for(int i = 0; i < properties.Length; i++)
		{
			EditorGUI.PropertyField( dividedRect , properties[i] );
			dividedRect.x += dividedRect.width;
		}

		//EditorGUI.PropertyField( rect , property );
		rect.y += rect.height;
	}
	
	public static void DrawEditorLayoutHorizontalLine(Color color, int thickness = 1, int padding = 10)
	{
		Rect rect = EditorGUILayout.GetControlRect( GUILayout.Height( padding + thickness ) );

		rect.height = thickness;
		rect.y += padding / 2;
		//rect.x -= 2;
		//rect.width +=6;

		EditorGUI.DrawRect( rect , color );
	}

	public static void DrawEditorHorizontalLine( ref Rect rect , Color color, int thickness = 1, int padding = 10)
	{		
		rect.height = thickness;
		rect.y += padding / 2;
		//rect.x -= 2;
		//rect.width +=6;

		EditorGUI.DrawRect( rect , color );

		rect.y += padding;
		rect.height = EditorGUIUtility.singleLineHeight;
	}


	//ScriptableObjects ----------------------------------------------------------------------------------------------

	/// <summary>
	/// Add a ScriptableObject to an asset. 
	/// </summary>
	/// <param name="scriptableObject"></param>
	/// <param name="list">The list that will hold the reference of the added element.</param>
	/// <param name="name">The name of the element.</param>
	/// <param name="hideFlags">hideFlags of the element.</param>
	/// <typeparam name="T"></typeparam>
	public static void AddElement<T>( this ScriptableObject scriptableObject , SerializedProperty listProperty , string name = "Element" , HideFlags hideFlags = HideFlags.None ) where T : ScriptableObject
    	{       
		if( !listProperty.isArray )
			throw new System.Exception( "\"listProperty\" is not a List." );
        
		T element = ScriptableObject.CreateInstance<T>();  

		element.name = name; 
		element.hideFlags = hideFlags;        

		string scriptableObjectPath = AssetDatabase.GetAssetPath( scriptableObject );
		
		//Undo.SetCurrentGroupName( "Add element to ScriptableObject" );
		//int group = Undo.GetCurrentGroup();

		
		AssetDatabase.AddObjectToAsset( element , scriptableObjectPath );
		AssetDatabase.SaveAssets();

		Undo.RegisterCreatedObjectUndo( element , "" );

		//Undo.CollapseUndoOperations( group );


		listProperty.InsertArrayElementAtIndex( listProperty.arraySize );
		SerializedProperty lastElement = listProperty.GetArrayElementAtIndex( listProperty.arraySize - 1 );
		lastElement.objectReferenceValue = element;

		
        
    	}

	public static void RemoveElement<T>( this ScriptableObject scriptableObject , int index , SerializedProperty listProperty ) where T : ScriptableObject
	{
		if( !listProperty.isArray )
			throw new System.Exception( "\"listProperty\" is not a List." );
		
		if( !Utilities.isBetween( index , 0 , listProperty.arraySize - 1 , true ) )
			throw new System.Exception( "\"index\" out of range." );
		
		if( listProperty.arraySize == 0 )
			return;
		
		SerializedProperty elementProperty = listProperty.GetArrayElementAtIndex( index );

		//Undo
		Undo.SetCurrentGroupName( "Remove element from ScriptableObject" );
		int group = Undo.GetCurrentGroup();
		
		
		
		Undo.DestroyObjectImmediate( elementProperty.objectReferenceValue );

		
		//Undo.RecordObject( listProperty.objectReferenceValue , "");
		listProperty.serializedObject.Update();

		listProperty.DeleteArrayElementAtIndex( index );  

		//Undo.RecordObject( listProperty.objectReferenceValue , "");
		listProperty.DeleteArrayElementAtIndex( index );

		listProperty.serializedObject.ApplyModifiedProperties();
		
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();

		Undo.CollapseUndoOperations( group );
				
		
		
	}

	public static void MoveElement<T>( this ScriptableObject scriptableObject , int elementIndex , int targetIndex , SerializedProperty listProperty )
	where T : ScriptableObject
	{
		if( !listProperty.isArray )
			throw new System.Exception( "\"listProperty\" is not a List." );
		
		if( 	!Utilities.isBetween( elementIndex , 0 , listProperty.arraySize - 1 , true ) || 
			!Utilities.isBetween( targetIndex , 0 , listProperty.arraySize - 1 , true ))
			throw new System.Exception( "\"indices\" out of range." );
		
		if( listProperty.arraySize == 0 )
			return;
		
		SerializedProperty elementProperty = listProperty.GetArrayElementAtIndex( elementIndex );
		SerializedProperty targetProperty = listProperty.GetArrayElementAtIndex( targetIndex );		

		T elementObject = elementProperty.objectReferenceValue as T;
		T targetObject = targetProperty.objectReferenceValue as T;

		elementProperty.objectReferenceValue = targetObject;
		targetProperty.objectReferenceValue = elementObject;		    
		
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
		
	}


	// Editor Styles
	//public static GUIStyle whiteLabelSmall = (GUIStyle)"sequenceGroupFont";
	//public static GUIStyle whiteLabelCentered = (GUIStyle)(GUIStyle)"PreLabel";
	public static GUIStyle gameViewBackground = (GUIStyle)"GameViewBackground";

	#endif

}

// #if UNITY_EDITOR

// public static class EditorUtilities
// {
// 	public static void AbstractPropertyDrawer(this SerializedProperty property)
// 	{
// 		if(property == null)
// 			throw new ArgumentNullException ("Null Property");

// 		if(property.propertyType == SerializedPropertyType.ObjectReference)
// 		{
// 			if(property.objectReferenceValue == null)
// 			{
// 				EditorGUILayout.PropertyField(property);
// 				return;
// 			}

// 			System.Type concreteType = property.objectReferenceValue.GetType();
// 			UnityEngine.Object castedObject = (UnityEngine.Object) System.Convert.ChangeType(property.objectReferenceValue,concreteType);

// 			UnityEditor.Editor editor= UnityEditor.Editor.CreateEditor(castedObject);

// 			editor.OnInspectorGUI();
// 		}
// 		else
// 		{
// 			EditorGUILayout.PropertyField(property);
// 		}
// 	}

// 	// public static void DrawInspectorWithoutHeader( this Editor editor )
// 	// {
// 	// 	editor.
// 	// }

// }


// #endif

}


