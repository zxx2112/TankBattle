using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEditor.VisualScripting.Editor.SmartSearch;
using UnityEditor.VisualScripting.Model;
using UnityEditor.VisualScripting.Model.Stencils;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityEditor.VisualScripting.Editor.ConstantEditor
{
    [GraphtoolsExtensionMethods]
    public static class ConstantEditorExtensions
    {
        public static VisualElement BuildEnumEditor(this IConstantEditorBuilder builder, EnumConstantNodeModel enumConstant)
        {
            var enumEditor = new Button { text = enumConstant.EnumValue.ToString() }; // TODO use a bindable element
            enumEditor.clickable.clickedWithEventInfo += e =>
            {
                SearcherService.ShowEnumValues("Pick a value", enumConstant.EnumType.Resolve(enumConstant.GraphModel.Stencil), e.originalMousePosition, (v, i) =>
                {
                    enumConstant.value.Value = Convert.ToInt32(v);
                    enumEditor.text = v.ToString();
                    builder.OnValueChanged?.Invoke(null);
                });
            };
            enumEditor.SetEnabled(!enumConstant.IsLocked);
            return enumEditor;
        }

        public static VisualElement BuildStringWrapperEditor(this IConstantEditorBuilder builder, IStringWrapperConstantModel icm)
        {
            var enumEditor = new Button { text = icm.ObjectValue.ToString() }; // TODO use a bindable element
            enumEditor.clickable.clickedWithEventInfo += e =>
            {
                List<string> allInputNames = icm.GetAllInputNames();
                SearcherService.ShowValues("Pick a value", allInputNames, e.originalMousePosition, (v, pickedIndex) =>
                {
                    icm.SetValueFromString(v);
                    enumEditor.text = v;
                    builder.OnValueChanged?.Invoke(null);
                });
            };
            enumEditor.SetEnabled(!icm.IsLocked);
            return enumEditor;
        }
    }
}
