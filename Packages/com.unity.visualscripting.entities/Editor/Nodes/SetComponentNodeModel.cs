using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Unity.Entities;
using UnityEditor.VisualScripting.Editor.SmartSearch;
using UnityEditor.VisualScripting.GraphViewModel;
using UnityEditor.VisualScripting.Model.Translators;
using UnityEditor.VisualScripting.SmartSearch;

namespace UnityEditor.VisualScripting.Model.Stencils
{
    [SearcherItem(typeof(EcsStencil), SearcherContext.Stack, "Components/Set Component")]
    [Serializable]
    public class SetComponentNodeModel : EcsHighLevelNodeModel, IHasEntityInputPort
    {
        public const string EntityLabel = "entity";

        public IPortModel EntityPort { get; private set; }

        [TypeSearcher(typeof(SetComponentFilter))]
        public TypeHandle ComponentType = TypeHandle.Unknown;

        ComponentPortsDescription m_ComponentDescription;

        protected override void OnDefineNode()
        {
            EntityPort = AddDataInput<Entity>("entity");

            if (ComponentType != TypeHandle.Unknown)
            {
                m_ComponentDescription = AddPortsForComponent(ComponentType);
            }
        }

        public IEnumerable<IPortModel> GetPortsForComponent()
        {
            return m_ComponentDescription.GetFieldIds().Select(id => InputsById[id]);
        }
    }

    [GraphtoolsExtensionMethods]
    public static class SetComponentTranslator
    {
        public static IEnumerable<SyntaxNode> Build(
            this RoslynEcsTranslator translator,
            SetComponentNodeModel model,
            IPortModel portModel)
        {
            var componentType = model.ComponentType.Resolve(model.GraphModel.Stencil);
            var componentInputs = model.GetPortsForComponent().ToArray();
            var entitySyntax = translator.BuildPort(model.EntityPort).SingleOrDefault() as ExpressionSyntax;
            var componentSyntax = translator.BuildComponentFromInput(componentType, componentInputs);
            var entityTranslator = translator.context.GetEntityManipulationTranslator();

            return entityTranslator.SetComponent(translator.context, entitySyntax, componentType, componentSyntax);
        }
    }

    class SetComponentFilter : ISearcherFilter
    {
        public SearcherFilter GetFilter(INodeModel model)
        {
            var ecsStencil = (EcsStencil)model.GraphModel.Stencil;
            if (ecsStencil.ComponentDefinitions.TryGetValue(model, out var componentDefinitions))
            {
                var componentTypes = componentDefinitions.Select(def => def.TypeHandle);
                return new SearcherFilter(SearcherContext.Type).WithComponents(componentTypes);
            }

            return new SearcherFilter(SearcherContext.Type)
                .WithComponentData(ecsStencil)
                .WithSharedComponentData(ecsStencil);
        }
    }
}
