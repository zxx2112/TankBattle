using System;
using JetBrains.Annotations;
using Unity.Mathematics;
using UnityEditor.UIElements;
using UnityEditor.VisualScripting.Model;
using UnityEngine;
using UnityEngine.UIElements;
using Packages.VisualScripting.Editor;
using UnityEditor.VisualScripting.Editor.ConstantEditor;

namespace UnityEditor.VisualScripting.Entities.Editor
{
    [GraphtoolsExtensionMethods]
    static class ConstantEditorExtensions
    {
        static void Dispatch<T>(IConstantEditorBuilder builder, T oldValue, T newValue)
        {
            using (ChangeEvent<T> other = ChangeEvent<T>.GetPooled(oldValue, newValue))
                builder.OnValueChanged(other);
        }

        static void CreateField<T>(string label, IConstantEditorBuilder builder, VisualElement container, T oldValue, float fieldValue, Func<ChangeEvent<float>, T> generateValue)
        {
            var field = new FloatField(label);
            field.value = fieldValue;
            container.Add(field);
            field.RegisterValueChangedCallback(evt => Dispatch(builder, oldValue, generateValue(evt)));
        }

        public static VisualElement BuildVector2Editor(this IConstantEditorBuilder builder, ConstantNodeModel<Vector2> v)
        {
            var root = new VisualElement();
            root.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(UICreationHelper.TemplatePath + "ConstantEditors.uss"));
            root.AddToClassList("vs-inline-float-editor");
            CreateField("x", builder, root, v.value, v.value.x, ChangeX);
            CreateField("y", builder, root, v.value, v.value.y, ChangeY);

            return root;

            Vector2 ChangeX(ChangeEvent<float> evt) => new Vector2(evt.newValue, v.value.y);
            Vector2 ChangeY(ChangeEvent<float> evt) => new Vector2(v.value.x, evt.newValue);
        }

        public static VisualElement BuildVector3Editor(this IConstantEditorBuilder builder, ConstantNodeModel<Vector3> v)
        {
            var root = new VisualElement();
            root.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(UICreationHelper.TemplatePath + "ConstantEditors.uss"));
            root.AddToClassList("vs-inline-float-editor");

            CreateField("x", builder, root, v.value, v.value.x, ChangeX);
            CreateField("y", builder, root, v.value, v.value.y, ChangeY);
            CreateField("z", builder, root, v.value, v.value.z, ChangeZ);

            return root;

            //Local Functions
            Vector3 ChangeX(ChangeEvent<float> evt) => new Vector3(evt.newValue, v.value.y, v.value.z);
            Vector3 ChangeY(ChangeEvent<float> evt) => new Vector3(v.value.x, evt.newValue, v.value.z);
            Vector3 ChangeZ(ChangeEvent<float> evt) => new Vector3(v.value.x, v.value.y, evt.newValue);
        }

        public static VisualElement BuildVector4Editor(this IConstantEditorBuilder builder, ConstantNodeModel<Vector4> v)
        {
            var root = new VisualElement();
            root.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(UICreationHelper.TemplatePath + "ConstantEditors.uss"));
            root.AddToClassList("vs-inline-float-editor");

            CreateField("x", builder, root, v.value, v.value.x, ChangeX);
            CreateField("y", builder, root, v.value, v.value.y, ChangeY);
            CreateField("z", builder, root, v.value, v.value.z, ChangeZ);
            CreateField("w", builder, root, v.value, v.value.w, ChangeW);

            return root;

            //Local Functions
            Vector4 ChangeX(ChangeEvent<float> evt) => new Vector4(evt.newValue, v.value.y, v.value.z, v.value.w);
            Vector4 ChangeY(ChangeEvent<float> evt) => new Vector4(v.value.x, evt.newValue, v.value.z, v.value.w);
            Vector4 ChangeZ(ChangeEvent<float> evt) => new Vector4(v.value.x, v.value.y, evt.newValue, v.value.w);
            Vector4 ChangeW(ChangeEvent<float> evt) => new Vector4(v.value.x, v.value.y, v.value.z, evt.newValue);
        }

        public static VisualElement BuildFloat2Editor(this IConstantEditorBuilder builder, ConstantNodeModel<float2> f)
        {
            var root = new VisualElement();
            root.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(UICreationHelper.TemplatePath + "ConstantEditors.uss"));
            root.AddToClassList("vs-inline-float-editor");
            CreateField("x", builder, root, f.value, f.value.x, ChangeX);
            CreateField("y", builder, root, f.value, f.value.y, ChangeY);

            return root;

            float2 ChangeX(ChangeEvent<float> evt) => new float2(evt.newValue, f.value.y);
            float2 ChangeY(ChangeEvent<float> evt) => new float2(f.value.x, evt.newValue);
        }

        public static VisualElement BuildFloat3Editor(this IConstantEditorBuilder builder, ConstantNodeModel<float3> f)
        {
            var root = new VisualElement();
            root.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(UICreationHelper.TemplatePath + "ConstantEditors.uss"));
            root.AddToClassList("vs-inline-float-editor");

            CreateField("x", builder, root, f.value, f.value.x, ChangeX);
            CreateField("y", builder, root, f.value, f.value.y, ChangeY);
            CreateField("z", builder, root, f.value, f.value.z, ChangeZ);

            return root;

            //Local Functions
            float3 ChangeX(ChangeEvent<float> evt) => new float3(evt.newValue, f.value.y, f.value.z);
            float3 ChangeY(ChangeEvent<float> evt) => new float3(f.value.x, evt.newValue, f.value.z);
            float3 ChangeZ(ChangeEvent<float> evt) => new float3(f.value.x, f.value.y, evt.newValue);
        }

        public static VisualElement BuildFloat4Editor(this IConstantEditorBuilder builder, ConstantNodeModel<float4> f)
        {
            var root = new VisualElement();
            root.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(UICreationHelper.TemplatePath + "ConstantEditors.uss"));
            root.AddToClassList("vs-inline-float-editor");

            CreateField("x", builder, root, f.value, f.value.x, ChangeX);
            CreateField("y", builder, root, f.value, f.value.y, ChangeY);
            CreateField("z", builder, root, f.value, f.value.z, ChangeZ);
            CreateField("w", builder, root, f.value, f.value.w, ChangeW);

            return root;

            //Local Functions
            float4 ChangeX(ChangeEvent<float> evt) => new float4(evt.newValue, f.value.y, f.value.z, f.value.w);
            float4 ChangeY(ChangeEvent<float> evt) => new float4(f.value.x, evt.newValue, f.value.z, f.value.w);
            float4 ChangeZ(ChangeEvent<float> evt) => new float4(f.value.x, f.value.y, evt.newValue, f.value.w);
            float4 ChangeW(ChangeEvent<float> evt) => new float4(f.value.x, f.value.y, f.value.z, evt.newValue);
        }

        public static VisualElement BuildQuaternionEditor(this IConstantEditorBuilder builder, ConstantNodeModel<Quaternion> q)
        {
            var root = new VisualElement();
            root.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(UICreationHelper.TemplatePath + "ConstantEditors.uss"));
            root.AddToClassList("vs-inline-float-editor");

            CreateField("x", builder, root, q.value, q.value.x, ChangeX);
            CreateField("y", builder, root, q.value, q.value.y, ChangeY);
            CreateField("z", builder, root, q.value, q.value.z, ChangeZ);
            CreateField("w", builder, root, q.value, q.value.w, ChangeW);

            return root;

            //Local Functions
            Quaternion ChangeX(ChangeEvent<float> evt) => new Quaternion(evt.newValue, q.value.y, q.value.z, q.value.w);
            Quaternion ChangeY(ChangeEvent<float> evt) => new Quaternion(q.value.x, evt.newValue, q.value.z, q.value.w);
            Quaternion ChangeZ(ChangeEvent<float> evt) => new Quaternion(q.value.x, q.value.y, evt.newValue, q.value.w);
            Quaternion ChangeW(ChangeEvent<float> evt) => new Quaternion(q.value.x, q.value.y, q.value.z, evt.newValue);
        }

        static VisualElement BuildSingleFieldEditor<T>(this IConstantEditorBuilder builder, T oldValue, BaseField<T> field)
        {
            var root = new VisualElement();
            //Mimic UIElement property fields style
            root.AddToClassList("unity-property-field");
            root.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(UICreationHelper.TemplatePath + "ConstantEditors.uss"));
            field.value = oldValue;
            root.Add(field);
            field.RegisterValueChangedCallback(evt => builder.OnValueChanged(evt));
            return root;
        }

        public static VisualElement BuildColorEditor(this IConstantEditorBuilder builder, ConstantNodeModel<Color> c)
        {
            return builder.BuildSingleFieldEditor(c.value, new ColorField());
        }

        public static VisualElement BuildFloatEditor(this IConstantEditorBuilder builder, ConstantNodeModel<float> f)
        {
            return builder.BuildSingleFieldEditor(f.value, new FloatField());
        }

        public static VisualElement BuildDoubleEditor(this IConstantEditorBuilder builder, ConstantNodeModel<double> d)
        {
            return builder.BuildSingleFieldEditor(d.value, new DoubleField());
        }

        public static VisualElement BuildIntEditor(this IConstantEditorBuilder builder, ConstantNodeModel<int> i)
        {
            return builder.BuildSingleFieldEditor(i.value, new IntegerField());
        }

        public static VisualElement BuildBoolEditor(this IConstantEditorBuilder builder, ConstantNodeModel<bool> b)
        {
            return builder.BuildSingleFieldEditor(b.value, new Toggle());
        }

        public static VisualElement BuildStringEditor(this IConstantEditorBuilder builder, ConstantNodeModel<string> s)
        {
            return builder.BuildSingleFieldEditor(s.value, new TextField());
        }
    }
}
