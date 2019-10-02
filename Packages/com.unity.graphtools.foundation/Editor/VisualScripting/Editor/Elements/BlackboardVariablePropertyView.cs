using System;
using UnityEditor.VisualScripting.Editor.SmartSearch;
using UnityEditor.VisualScripting.Model;
using UnityEditor.VisualScripting.Model.Stencils;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityEditor.VisualScripting.Editor
{
    public class BlackboardVariablePropertyView : BlackboardExtendedFieldView
    {
        Toggle m_ExposedToggle;
        Toggle m_ArrayToggle;
        TextField m_TooltipTextField;
        readonly VisualElement m_InitializationElement;
        readonly Store m_Store;
        readonly Stencil m_Stencil;

        IVariableDeclarationModel VariableDeclarationModel => userData as IVariableDeclarationModel;
        string TypeText => VariableDeclarationModel.DataType.GetMetadata(m_Stencil).FriendlyName;
        bool IsArray => VariableDeclarationModel.DataType.IsVsArrayType(m_Stencil);

        static readonly GUIContent k_InitializationContent = new GUIContent("");

        readonly SerializedObject m_InitializationObject;

        public BlackboardVariablePropertyView(Store store, IVariableDeclarationModel variableDeclarationModel,
                                              Blackboard.RebuildCallback rebuildCallback, Stencil stencil)
            : base(variableDeclarationModel, rebuildCallback)
        {
            m_Store = store;
            m_Stencil = stencil;

            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);

            if (variableDeclarationModel.VariableType != VariableType.FunctionVariable &&
                variableDeclarationModel.VariableType != VariableType.GraphVariable)
                return;

            if (!variableDeclarationModel.InitializationModel?.NodeAssetReference)
            {
                if (stencil.GetVariableInitializer().RequiresInitialization(variableDeclarationModel))
                {
                    m_InitializationElement = new Button(OnInitializationButton) {text = "Create Init value"};
                    m_InitializationElement.AddToClassList("rowButton");
                }
            }
            else
            {
                m_InitializationObject = new SerializedObject(variableDeclarationModel.InitializationModel.NodeAssetReference);
                m_InitializationObject.Update();
                m_InitializationElement = new IMGUIContainer(OnInitializationGUI);
            }
        }

        public BlackboardVariablePropertyView WithTypeSelector()
        {
            var typeButton = new Button(() =>
                SearcherService.ShowTypes(
                    m_Stencil,
                    Event.current.mousePosition,
                    (t, i) => OnTypeChanged(t)
                )
                ) { text = TypeText };
            typeButton.AddToClassList("rowButton");
            AddRow("Type", typeButton);

            return this;
        }

        public BlackboardVariablePropertyView WithExposedToggle()
        {
            if (VariableDeclarationModel.VariableType == VariableType.GraphVariable)
            {
                m_ExposedToggle = new Toggle { value = VariableDeclarationModel.IsExposed };
                AddRow("Exposed", m_ExposedToggle);
            }

            return this;
        }

        public BlackboardVariablePropertyView WithArrayToggle()
        {
            m_ArrayToggle = new Toggle { value = IsArray };
            AddRow("Array", m_ArrayToggle);

            return this;
        }

        public BlackboardVariablePropertyView WithTooltipField()
        {
            m_TooltipTextField = new TextField
            {
                isDelayed = true,
                value = VariableDeclarationModel.Tooltip
            };
            AddRow("Tooltip", m_TooltipTextField);

            return this;
        }

        public BlackboardVariablePropertyView WithInitializationField()
        {
            //TODO For now we will hide the unsupported array initialization via the blackboard.
            if (!VariableDeclarationModel.DataType.IsVsArrayCompatible(m_Stencil))
                AddRow("Initialization", m_InitializationElement);

            return this;
        }

        void OnDetachFromPanel(DetachFromPanelEvent evt)
        {
            m_ArrayToggle.UnregisterValueChangedCallback(OnArrayChanged);
            m_ExposedToggle?.UnregisterValueChangedCallback(OnExposedChanged);
            m_TooltipTextField.UnregisterValueChangedCallback(OnTooltipChanged);
        }

        void OnAttachToPanel(AttachToPanelEvent evt)
        {
            schedule.Execute(() => m_ExposedToggle?.RegisterValueChangedCallback(OnExposedChanged));
            schedule.Execute(() => m_ArrayToggle.RegisterValueChangedCallback(OnArrayChanged));
            m_TooltipTextField.RegisterValueChangedCallback(OnTooltipChanged);
        }

        void OnInitializationGUI()
        {
            m_InitializationObject.Update();
            EditorGUI.BeginChangeCheck();
            ConstantNodeModelInspector.ConstantEditorGUI(m_InitializationObject, k_InitializationContent, m_Stencil);
            if (EditorGUI.EndChangeCheck())
            {
                m_InitializationObject.ApplyModifiedProperties();
                m_InitializationObject.Update();
                m_InitializationElement.MarkDirtyRepaint();
                m_Store.Dispatch(new RefreshUIAction(UpdateFlags.RequestCompilation));
            }
        }

        void RefreshUI(Blackboard.RebuildMode rebuildMode = Blackboard.RebuildMode.BlackboardAndGraphView)
        {
            m_ArrayToggle.UnregisterValueChangedCallback(OnArrayChanged);
            m_ExposedToggle?.UnregisterValueChangedCallback(OnExposedChanged);
            m_RebuildCallback?.Invoke(rebuildMode);
        }

        void OnInitializationButton()
        {
            ((VariableDeclarationModel)userData).CreateInitializationValue();

            RefreshUI();
        }

        void OnTypeChanged(TypeHandle handle)
        {
            m_Store.Dispatch(new UpdateTypeAction((VariableDeclarationModel)VariableDeclarationModel, handle));
            RefreshUI();
        }

        void OnArrayChanged(ChangeEvent<bool> evt)
        {
            m_Store.Dispatch(new UpdateTypeRankAction((VariableDeclarationModel)VariableDeclarationModel, m_ArrayToggle.value));
            RefreshUI();
        }

        void OnExposedChanged(ChangeEvent<bool> evt)
        {
            m_Store.Dispatch(new UpdateExposedAction((VariableDeclarationModel)VariableDeclarationModel, m_ExposedToggle.value));
            RefreshUI();
        }

        void OnTooltipChanged(ChangeEvent<string> evt)
        {
            m_Store.Dispatch(new UpdateTooltipAction((VariableDeclarationModel)VariableDeclarationModel, m_TooltipTextField.value));
            RefreshUI();
        }
    }
}
