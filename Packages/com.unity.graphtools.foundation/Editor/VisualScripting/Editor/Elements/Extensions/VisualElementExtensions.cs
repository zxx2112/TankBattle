using System;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using UnityEditor.UIElements;
using UnityEditor.VisualScripting.Editor.SmartSearch;
using UnityEditor.VisualScripting.GraphViewModel;
using UnityEditor.VisualScripting.Model;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityEditor.VisualScripting.Editor.ConstantEditor
{
    [PublicAPI]
    public static class VisualElementExtensions
    {
        public static VisualElement CreateEditorForNodeModel(this VisualElement element, IConstantNodeModel model, Action<IChangeEvent> onValueChanged)
        {
            VisualElement editorElement;

            var ext = ModelUtility.ExtensionMethodCache<IConstantEditorBuilder>.GetExtensionMethod(model.GetType(), ConstantEditorBuilder.FilterMethods, ConstantEditorBuilder.KeySelector);
            if (ext != null)
            {
                Action<IChangeEvent> myValueChanged = evt =>
                {
                    var p = evt.GetType().GetProperty("newValue");
                    var newValue = p.GetValue(evt);
                    ((ConstantNodeModel)model).ObjectValue = newValue;
                    onValueChanged(evt);
                };
                var constantBuilder = new ConstantEditorBuilder(myValueChanged);
                editorElement = (VisualElement)ext.Invoke(null, new object[] { constantBuilder, model });
            }
            else if (model is ConstantNodeModel constantNodeModel && constantNodeModel.NodeAssetReference != null)
            {
                var serializedObject = new SerializedObject(constantNodeModel.NodeAssetReference);

                SerializedProperty serializedProperty = serializedObject.FindProperty("m_NodeModel.value");
                var propertyField = new PropertyField(serializedProperty);

                editorElement = propertyField;
                editorElement.SetEnabled(!constantNodeModel.IsLocked);

                // delayed because the initial binding would cause an event otherwise, and then a compilation
                propertyField.schedule.Execute(() =>
                {
                    var onValueChangedEventCallback = new EventCallback<IChangeEvent>(onValueChanged);

                    // HERE BE DRAGONS
                    // there's no way atm to be notified that a PropertyField's value changed so we build a ChangeEvent<T>
                    // callback registration using reflection, but actually provide an Action<IChangeEvent>
                    Type type = constantNodeModel.Type;
                    Type eventType = typeof(ChangeEvent<>).MakeGenericType(type);
                    MethodInfo genericRegisterCallbackMethod = typeof(VisualElement).GetMethods().Single(m =>
                    {
                        var parameterInfos = m.GetParameters();
                        return m.Name == nameof(VisualElement.RegisterCallback) && parameterInfos.Length == 2 && parameterInfos[1].ParameterType == typeof(TrickleDown);
                    });
                    MethodInfo registerCallbackMethod = genericRegisterCallbackMethod.MakeGenericMethod(eventType);
                    registerCallbackMethod.Invoke(propertyField, new object[] { onValueChangedEventCallback, TrickleDown.NoTrickleDown });
                }).ExecuteLater(1);
            }
            else
            {
                Debug.Log($"Could not draw Editor GUI for node of type {model.GetType()}");
                editorElement = new Label("<Unknown>");
            }

            return editorElement;
        }

        public static void RemovePropertyFieldValueLabel(this PropertyField propertyField)
        {
            // PropertyFields show a label saying "Value" by default which we don't really like
            // We only keep it in the case where the PropertyField has a "Foldout"
            // this ensures we have at least some text on folded property fields
            if (!(propertyField.Children().First() is Foldout))
                propertyField.Q<Label>(classes: new[] { "unity-base-field__label" })?.RemoveFromHierarchy();
        }
    }
}
