using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor.Experimental.GraphView;
using UnityEditor.VisualScripting.Editor.SmartSearch;
using UnityEditor.VisualScripting.GraphViewModel;
using UnityEditor.VisualScripting.Model;
using UnityEditor.VisualScripting.Model.Stencils;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

namespace UnityEditor.VisualScripting.Editor
{
    public class HighLevelNode : Node
    {
        const string k_ScriptPropertyName = "m_Script";
        const string k_GeneratorAssetPropertyName = "m_GeneratorAsset";

        static readonly CustomStyleProperty<float> k_LabelWidth = new CustomStyleProperty<float>("--unity-hl-node-label-width");
        static readonly CustomStyleProperty<float> k_FieldWidth = new CustomStyleProperty<float>("--unity-hl-node-field-width");

        const float k_DefaultLabelWidth = 150;
        const float k_DefaultFieldWidth = 120;

        static readonly HashSet<string> k_ExcludedPropertyNames =
            new HashSet<string>(
                typeof(NodeModel)
                    .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .Select(f => f.Name)
            )
        {
            k_ScriptPropertyName,
            k_GeneratorAssetPropertyName
        };

        public HighLevelNode(INodeModel model, Store store, GraphView graphView)
            : base(model, store, graphView) {}

        protected override void UpdateFromModel()
        {
            base.UpdateFromModel();

            AddToClassList("highLevelNode");

            VisualElement topHorizontalDivider = this.MandatoryQ("divider", "horizontal");
            VisualElement topVerticalDivider = this.MandatoryQ("divider", "vertical");

            // GraphView automatically hides divider since there are no input ports
            topHorizontalDivider.RemoveFromClassList("hidden");
            topVerticalDivider.RemoveFromClassList("hidden");

            VisualElement output = this.MandatoryQ("output");
            output.AddToClassList("node-controls");

            var imguiContainer = CreateControls();

            imguiContainer.AddToClassList("node-controls");
            mainContainer.MandatoryQ("top").Insert(1, imguiContainer);
        }

        protected virtual VisualElement CreateControls()
        {
            return new IMGUIContainer(() =>
            {
                EditorGUIUtility.labelWidth = customStyle.TryGetValue(k_LabelWidth, out var labelWidth) ? labelWidth : k_DefaultLabelWidth;
                EditorGUIUtility.fieldWidth = customStyle.TryGetValue(k_FieldWidth, out var fieldWidth) ? fieldWidth : k_DefaultFieldWidth;
                DrawInspector(model.NodeAssetReference, RedefineNode);
            });
        }

        void DrawInspector(AbstractNodeAsset asset, Action onChangedCallback)
        {
            if (asset == null)
                return;

            EditorGUI.BeginChangeCheck();
            var obj = new SerializedObject(asset);
            obj.Update();
            var iterator = obj.FindProperty("m_NodeModel");

            for (var enterChildren = true; iterator.NextVisible(enterChildren); enterChildren = false)
            {
                if (k_ExcludedPropertyNames.Contains(iterator.name))
                    continue;

                var label = new GUIContent(iterator.displayName);

                var field = GetFieldViaPath(asset.GetType(), iterator.propertyPath);
                var typeSearcherAttribute = field?.GetCustomAttribute<TypeSearcherAttribute>();

                if (typeSearcherAttribute != null)
                    TypeReferencePicker(iterator, typeSearcherAttribute, label, onChangedCallback);
                else
                    EditorGUILayout.PropertyField(iterator, label, true);
            }

            obj.ApplyModifiedProperties();

            if (EditorGUI.EndChangeCheck())
                onChangedCallback();

            FieldInfo GetFieldViaPath(Type type, string path)
            {
                Type parentType = type;
                FieldInfo fi = type.GetField(path, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                string[] perDot = path.Split('.');
                foreach (string fieldName in perDot)
                {
                    fi = parentType.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (fi != null)
                        parentType = fi.FieldType;
                    else
                        return null;
                }
                if (fi != null)
                    return fi;
                return null;
            }
        }

        void TypeReferencePicker(SerializedProperty iterator, TypeSearcherAttribute attribute, GUIContent label, Action onChangedCallback)
        {
            //Fetch typename
            var typeHandleIdProperty = iterator.FindPropertyRelative(nameof(TypeHandle.Identification));
            var typeHandleAssetRefProperty = iterator.FindPropertyRelative(nameof(TypeHandle.GraphModelReference));

            var handle = new TypeHandle(typeHandleAssetRefProperty.objectReferenceValue as VSGraphModel, typeHandleIdProperty.stringValue);
            var stencil = model.GraphModel.Stencil;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(label);

            var friendlyName = handle.GetMetadata(stencil).FriendlyName;

            if (GUILayout.Button(friendlyName))
            {
                var mousePosition = mainContainer.LocalToWorld(Event.current.mousePosition);
                void Callback(TypeHandle type, int index)
                {
                    Assert.IsNotNull(typeHandleIdProperty);
                    Assert.IsNotNull(typeHandleAssetRefProperty);
                    typeHandleIdProperty.stringValue = type.Identification;
                    typeHandleAssetRefProperty.objectReferenceValue = type.GraphModelReference;
                    iterator.serializedObject.ApplyModifiedProperties();
                    onChangedCallback();
                }

                SearcherService.ShowTypes(stencil, mousePosition, Callback, attribute.Filter?.GetFilter(model));
            }

            EditorGUILayout.EndHorizontal();
        }
    }
}
