using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Packages.VisualScripting.Editor.Stencils;
using Unity.Entities;
using UnityEditor.VisualScripting.Extensions;
using UnityEditor.VisualScripting.GraphViewModel;
using UnityEditor.VisualScripting.Model.Translators;
using UnityEngine;
using UnityEngine.Assertions;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace UnityEditor.VisualScripting.Model.Stencils
{
    [PublicAPI]
    [Serializable]
    public abstract class OnEntitiesEventBaseNodeModel : EventFunctionModel, IPrivateIteratorStackModel, IOrderedStack, IHasInstancePort
    {
        const string k_Title = "On Update Matching";
        const int k_NonComponentVariablesCount = 1;

        public abstract UpdateMode Mode { get; }

        [SerializeField]
        int m_Order;

        public int Order
        {
            get => m_Order;
            set => m_Order = value;
        }

        [SerializeField]
        List<CriteriaModel> m_CriteriaModels = new List<CriteriaModel>();

        public ComponentQueryDeclarationModel ComponentQueryDeclarationModel { get; private set; }

        public IReadOnlyList<CriteriaModel> CriteriaModels => m_CriteriaModels;

        public void AddCriteriaModelNoUndo(CriteriaModel criteriaModel)
        {
            m_CriteriaModels.Add(criteriaModel);
            UpdateTakenNames();
        }

        public void InsertCriteriaModelNoUndo(int index, CriteriaModel criteriaModel)
        {
            m_CriteriaModels.Insert(index, criteriaModel);
            UpdateTakenNames();
        }

        public void RemoveCriteriaModelNoUndo(CriteriaModel criteriaModel)
        {
            m_CriteriaModels.Remove(criteriaModel);
            UpdateTakenNames();
        }

        public int IndexOfCriteriaModel(CriteriaModel criteriaModel)
        {
            return m_CriteriaModels.IndexOf(criteriaModel);
        }

        void UpdateTakenNames()
        {
            m_TakenNames = CriteriaModels.Select(x => x.Name).ToHashSet();
        }

        public bool AddTakenName(string proposedName) => m_TakenNames.Add(proposedName);

        public VariableDeclarationModel ItemVariableDeclarationModel => m_FunctionParameterModels?.Count > 0
        ? m_FunctionParameterModels[0]
        : null;

        public override string Title => MakeTitle();
        public override bool AllowMultipleInstances => true;

        public IPortModel InstancePort { get; private set; }

        protected abstract string MakeTitle();


        protected override void OnDefineNode()
        {
            ReturnType = TypeHandle.Void;
            InstancePort = AddInstanceInput<EntityQuery>();
            OutputPort = AddExecutionOutputPort(null);
            if (NodeAssetReference != null)
                NodeAssetReference.name = Title;
        }

        protected override void OnCreateLoopVariables(VariableCreator variableCreator, IPortModel connectedPortModel)
        {
            if ((connectedPortModel?.NodeModel as IObjectReference)?.ReferencedObject is
                VariableDeclarationModel variableDeclarationModel &&
                variableDeclarationModel.DataType != typeof(EntityQuery).GenerateTypeHandle(Stencil))
                return;

            var query = ComponentQueryDeclarationModel;
            var entityName = query != null
                ? $"{query.Name}Entity"
                : "Entity";

            variableCreator.DeclareVariable<LoopVariableDeclarationModel>(
                entityName,
                typeof(Entity).GenerateTypeHandle(Stencil),
                LoopStackModel.TitleComponentIcon.Item, VariableFlags.Generated);

            Assert.AreEqual(k_NonComponentVariablesCount, variableCreator.DeclaredVariablesCount);
            if (Mode != UpdateMode.OnEnd)
                ForAllEntitiesStackModel.CreateComponentVariables(Stencil, variableCreator, this);
        }

        public override void OnConnection(IPortModel selfConnectedPortModel, IPortModel otherConnectedPortModel)
        {
            if (otherConnectedPortModel != null && otherConnectedPortModel.PortType != PortType.Execution)
            {
                ComponentQueryDeclarationModel = GetConnectedEntityQuery(otherConnectedPortModel);
                CreateLoopVariables(otherConnectedPortModel);
            }

            ((VSGraphModel)GraphModel).LastChanges.ChangedElements.Add(this);

            if (NodeAssetReference != null)
                NodeAssetReference.name = Title;
        }

        public static ComponentQueryDeclarationModel GetConnectedEntityQuery(IPortModel otherPortModel)
        {
            return (otherPortModel?.NodeModel
                as VariableNodeModel)?.DeclarationModel
                as ComponentQueryDeclarationModel;
        }

        HashSet<string> m_TakenNames = new HashSet<string>();
        public IReadOnlyCollection<string> TakenNames => m_TakenNames;
    }

    [PublicAPI]
    public enum UpdateMode
    {
        OnUpdate,
        OnStart,
        OnEnd,
        OnEvent
    }

    [GraphtoolsExtensionMethods]
    public static class OnEntitiesEventBaseTranslator
    {
        public static IEnumerable<SyntaxNode> BuildOnEntitiesEventBase(this RoslynEcsTranslator translator,
            OnEntitiesEventBaseNodeModel model, IPortModel portModel)
        {
            if (model.InstancePort == null || !model.InstancePort.Connected)
                return Enumerable.Empty<SyntaxNode>();
            var query = model.ComponentQueryDeclarationModel;
            if (!query)
                return Enumerable.Empty<SyntaxNode>();

            translator.GameObjectCodeGen = false;
            EcsStencil ecsStencil = (EcsStencil)translator.Stencil;
            if (ecsStencil.entryPointsToQueries.TryGetValue(model, out var queries) &&
                queries.Any(q => q.Components.Any(c => EcsStencil.IsValidGameObjectComponentType(c.Component.TypeHandle.Resolve(ecsStencil)))))
            {
                translator.GameObjectCodeGen = true;
            }

            translator.PushContext(model, model.Mode);

            var entityTypeHandle = typeof(Entity).GenerateTypeHandle(translator.Stencil);
            var entityName = model.FunctionParameterModels.SingleOrDefault(
                m => m.DataType == entityTypeHandle)?.VariableName;
            translator.context.AddEntityDeclaration(entityName);

            foreach (var declarationModel in model.FunctionVariableModels)
                translator.context.AddStatement(declarationModel.DeclareLocalVariable(translator));

            translator.context.AddCriterionCondition(translator, entityName, model.CriteriaModels);

            var block = Block();
            translator.BuildStack(model, ref block, StackExitStrategy.Continue);
            foreach (var stmt in block.Statements)
                translator.context.AddStatement(stmt);

            // will return a job scheduling statement or the entire foreach block
            return translator.PopContext();
        }
    }
}
