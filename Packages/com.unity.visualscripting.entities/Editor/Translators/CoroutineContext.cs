using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Packages.VisualScripting.Editor.Stencils;
using Unity.Collections;
using Unity.Entities;
using UnityEditor.VisualScripting.GraphViewModel;
using UnityEditor.VisualScripting.Model.Stencils;
using UnityEngine;
using VisualScripting.Entities.Runtime;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace UnityEditor.VisualScripting.Model.Translators
{
    sealed class CoroutineContext : TranslationContext
    {
        const string k_MoveNext = "MoveNext";

        readonly string m_ComponentTypeName;
        Dictionary<ArgumentSyntax, ParameterSyntax> m_Parameters = new Dictionary<ArgumentSyntax, ParameterSyntax>();
        HashSet<TypeHandle> m_AccessedComponents = new HashSet<TypeHandle>();
        HashSet<ComponentDefinition> m_DeclaredComponentArray = new HashSet<ComponentDefinition>();
        HashSet<ExpressionSyntax> m_DeclaredCommandBuffers = new HashSet<ExpressionSyntax>();

        public CoroutineContext(TranslationContext parent, RoslynEcsTranslator translator)
            : base(parent)
        {
            IterationContext = parent.IterationContext;
            m_ComponentTypeName = translator.MakeUniqueName($"{IterationContext.GroupName}Coroutine").ToPascalCase();
        }

        public override TranslationContext PushContext(IIteratorStackModel query,
            RoslynEcsTranslator roslynEcsTranslator, UpdateMode mode)
        {
            return new ForEachContext(query, this, mode);
        }

        protected override StatementSyntax OnPopContext()
        {
            var coroutineParameterName = RoslynEcsBuilder.BuildCoroutineParameterName(m_ComponentTypeName);

            // Build Coroutine MoveNext call statement
            var block = Block();

            // Assign times fields
            block = block.AddStatements(ExpressionStatement(RoslynBuilder.Assignment(
                RoslynBuilder.MemberReference(
                    IdentifierName(coroutineParameterName),
                    nameof(ICoroutine.DeltaTime)),
                Parent.GetOrDeclareDeltaTime())));

            // Call MoveNext
            block = block.AddStatements(IfStatement(
                RoslynBuilder.MethodInvocation(
                    k_MoveNext,
                    IdentifierName(coroutineParameterName),
                    m_Parameters.Keys,
                    Enumerable.Empty<TypeSyntax>()),
                ReturnStatement()));

            // Remove component when coroutine is completed
            var removeStatement = Parent.GetEntityManipulationTranslator().RemoveComponent(
                Parent,
                IdentifierName(Parent.EntityName),
                m_ComponentTypeName);
            block = removeStatement.Aggregate(block, (current, syntax) => current.AddStatements(syntax));

            return block;
        }

        public override void AddStatement(StatementSyntax statement)
        {
            Parent.AddStatement(statement);
        }

        public override void AddEntityDeclaration(string variableName)
        {
            Parent.AddEntityDeclaration(variableName);
        }

        public override string GetComponentVariableName(IIteratorStackModel query, TypeHandle type)
        {
            return Parent.GetComponentVariableName(query, type);
        }

        public override void RecordComponentAccess(RoslynEcsTranslator.IterationContext query, TypeHandle componentType,
            RoslynEcsTranslator.AccessMode mode)
        {
            Parent.RecordComponentAccess(query, componentType, mode);
            if (IterationContext == query)
                m_AccessedComponents.Add(componentType);
        }

        public override ExpressionSyntax GetCachedValue(string key, ExpressionSyntax value, TypeHandle type,
            params IdentifierNameSyntax[] attributes)
        {
            var constant = base.GetCachedValue(key, value, type, attributes);

            // Use the same identifier as the parentJob. This avoid things like myCoroutine.FixedTime0 = FixedTime
            var variableName = constant is IdentifierNameSyntax ins
                ? ins.Identifier.Text
                : key.Replace(".", "_");

            var found = m_Parameters.FirstOrDefault(p => p.Value.Identifier.Text.Equals(variableName));
            if (found.Value != null)
                return IdentifierName(found.Value.Identifier);

            var param = Parameter(Identifier(variableName))
                .WithType(TypeSystem.BuildTypeSyntax(type.Resolve(IterationContext.Stencil)));
            if (attributes.Any())
                param.AddAttributeLists(AttributeList(SeparatedList(attributes.Select(Attribute))));

            m_Parameters.Add(Argument(constant), param);

            return IdentifierName(param.Identifier);
        }

        protected override StatementSyntax GetOrDeclareEntityArray(RoslynEcsTranslator.IterationContext context,
            out StatementSyntax arrayDisposal)
        {
            if (Parent is JobContext)
            {
                m_Parameters.Add(
                    Argument(IdentifierName(context.EntitiesArrayName)),
                    Parameter(Identifier(context.EntitiesArrayName))
                        .WithType(TypeSystem.BuildTypeSyntax(typeof(NativeArray<Entity>))));
            }
            else
            {
                m_Parameters.Add(
                    Argument(IdentifierName(context.GroupName)),
                    Parameter(Identifier(context.GroupName))
                        .WithType(TypeSystem.BuildTypeSyntax(typeof(EntityQuery))));
            }

            return base.GetOrDeclareEntityArray(context, out arrayDisposal);
        }

        public override string GetOrDeclareComponentArray(RoslynEcsTranslator.IterationContext ctx,
            ComponentDefinition componentDefinition, out LocalDeclarationStatementSyntax arrayInitialization,
            out StatementSyntax arrayDisposal)
        {
            var declaration = Parent.GetOrDeclareComponentArray(
                ctx,
                componentDefinition,
                out arrayInitialization,
                out arrayDisposal);
            var parameter = declaration.ToCamelCase();

            if (!(Parent is JobContext) || m_DeclaredComponentArray.Contains(componentDefinition))
                return parameter;

            var componentType = componentDefinition.TypeHandle.Resolve(ctx.Stencil);
            var arrayType = typeof(NativeArray<>).MakeGenericType(componentType);

            m_Parameters.Add(
                Argument(IdentifierName(declaration)),
                Parameter(Identifier(parameter))
                    .WithType(TypeSystem.BuildTypeSyntax(arrayType)));
            m_DeclaredComponentArray.Add(componentDefinition);

            return parameter;
        }

        public override IdentifierNameSyntax GetOrDeclareCommandBuffer(bool isConcurrent)
        {
            var declaration = Parent.GetOrDeclareCommandBuffer(isConcurrent);
            var parameter = IdentifierName(declaration.Identifier.Text.ToCamelCase());
            if (m_DeclaredCommandBuffers.Contains(parameter))
                return parameter;

            var cmdType = isConcurrent && Parent is JobContext
                ? typeof(EntityCommandBuffer.Concurrent)
                : typeof(EntityCommandBuffer);

            m_Parameters.Add(
                Argument(IdentifierName(declaration.Identifier)),
                Parameter(parameter.Identifier).WithType(TypeSystem.BuildTypeSyntax(cmdType)));
            m_DeclaredCommandBuffers.Add(parameter);

            return parameter;
        }

        public override string GetJobIndexParameterName()
        {
            var declaration = Parent.GetJobIndexParameterName();
            var parameter = declaration.ToCamelCase();

            m_Parameters.Add(
                Argument(IdentifierName(declaration)),
                Parameter(Identifier(parameter)).WithType(TypeSystem.BuildTypeSyntax(typeof(int))));

            return parameter;
        }

        public override IdentifierNameSyntax GetEventBufferWriter(RoslynEcsTranslator.IterationContext iterationContext,
            ExpressionSyntax entity, Type eventType, out StatementSyntax bufferInitialization)
        {
            var declaration = Parent.GetEventBufferWriter(iterationContext, entity, eventType, out bufferInitialization);
            var parameter = declaration.Identifier.Text.ToCamelCase();

            m_Parameters.Add(
                Argument(declaration),
                Parameter(Identifier(parameter))
                    .WithType(
                    GenericName(Identifier("DynamicBuffer"))
                        .WithTypeArgumentList(
                        TypeArgumentList(
                            SingletonSeparatedList(TypeSystem.BuildTypeSyntax(eventType))))));

            return IdentifierName(parameter);
        }

        public override ExpressionSyntax GetSingletonVariable(IVariableDeclarationModel variable)
        {
            var exp = Parent.GetSingletonVariable(variable);
            var key = $"{RootContext.SingletonComponentTypeName}{variable.VariableName}";
            return GetCachedValue(key, exp, variable.DataType);
        }

        public void BuildComponent(RoslynEcsTranslator translator)
        {
            // Create coroutine component
            var members = BuildComponentMembers(translator);
            DeclareComponent<ISystemStateComponentData>(m_ComponentTypeName, members);

            // Add coroutine in the query archetype
            IncludeCoroutineComponent(IterationContext, m_ComponentTypeName);

            // Add coroutine component
            var rootContext = (RootContext)Parent.Parent;
            rootContext.PrependStatement(BuildAddCoroutineComponent(IterationContext, m_ComponentTypeName));
        }

        static ExpressionStatementSyntax BuildAddCoroutineComponent(
            RoslynEcsTranslator.IterationContext iterationContext, string coroutineName)
        {
            return ExpressionStatement(
                InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName(nameof(EntityManager)),
                        GenericName(
                            Identifier(nameof(EntityManager.AddComponent)))
                            .WithTypeArgumentList(
                            TypeArgumentList(
                                SingletonSeparatedList<TypeSyntax>(
                                    IdentifierName(coroutineName))))))
                    .WithArgumentList(
                    ArgumentList(
                        SingletonSeparatedList(
                            Argument(
                                IdentifierName(
                                    CoroutineTranslator.MakeExcludeCoroutineQueryName(iterationContext)))))));
        }

        IEnumerable<MemberDeclarationSyntax> BuildComponentMembers(RoslynTranslator translator)
        {
            var states = new List<SwitchSectionSyntax>();
            var variables = new Dictionary<string, FieldDeclarationSyntax>();
            var stateIndex = 0;

            // Add members (MoveNext + variables) from coroutine nodes in stack
            var stack = Parent.IterationContext.Query;
            BuildStack(translator, stack, ref variables, ref states, ref stateIndex);

            // Fields
            var members = new List<MemberDeclarationSyntax>
            {
                RoslynBuilder.DeclareField(
                    typeof(int),
                    RoslynEcsBuilder.CoroutineStateVariableName,
                    AccessibilityFlags.Private)
            };
            members.AddRange(variables.Values);

            // Parameters + Arguments
            foreach (var component in m_AccessedComponents)
            {
                var componentName = GetComponentVariableName(IterationContext.Query, component);
                var componentType = component.Resolve(IterationContext.Stencil);

                m_Parameters.Add(
                    Argument(IdentifierName(componentName))
                        .WithRefOrOutKeyword(Token(SyntaxKind.RefKeyword)),
                    Parameter(Identifier(componentName))
                        .WithModifiers(TokenList(Token(SyntaxKind.RefKeyword)))
                        .WithType(TypeSystem.BuildTypeSyntax(componentType)));
            }

            foreach (var localVariable in IterationContext.Query.FunctionVariableModels)
            {
                m_Parameters.Add(
                    Argument(IdentifierName(localVariable.Name)),
                    Parameter(Identifier(localVariable.Name))
                        .WithType(TypeSystem.BuildTypeSyntax(localVariable.DataType.Resolve(IterationContext.Stencil))));
            }

            // Create MoveNext method
            members.Add(
                RoslynBuilder.DeclareMethod(k_MoveNext, AccessibilityFlags.Public, typeof(bool))
                    .WithParameterList(
                        ParameterList(SeparatedList(m_Parameters.Values)))
                    .WithBody(
                        Block(
                            SwitchStatement(IdentifierName(RoslynEcsBuilder.CoroutineStateVariableName))
                                .WithOpenParenToken(Token(SyntaxKind.OpenParenToken))
                                .WithCloseParenToken(Token(SyntaxKind.CloseParenToken))
                                .WithSections(List(states)),
                            ReturnStatement(
                                LiteralExpression(SyntaxKind.FalseLiteralExpression)))));

            return members;
        }

        static void BuildStack(RoslynTranslator translator, IStackModel stack,
            ref Dictionary<string, FieldDeclarationSyntax> variables, ref List<SwitchSectionSyntax> states,
            ref int stateIndex)
        {
            translator.RegisterBuiltStack(stack);

            foreach (var node in stack.NodeModels)
            {
                switch (node)
                {
                    case IfConditionNodeModel _:
                        translator.AddError(node, "Coroutine (e.g. Wait) and Condition nodes can't coexist within the same stack for now. This feature is coming in a further release");
                        continue;

                    case ReturnNodeModel _:
                        translator.AddError(node, "Coroutine (e.g. Wait) and Return nodes can't coexist within the same stack for now. This feature is coming in a further release");
                        continue;
                }

                var blocks = translator.BuildNode(node);
                if (node is CoroutineNodeModel coroutineNode)
                {
                    foreach (var variable in coroutineNode.Fields)
                    {
                        if (variables.ContainsKey(variable.Key))
                            continue;

                        variables.Add(variable.Key, variable.Value);
                    }

                    foreach (var block in blocks)
                    {
                        states.Add(SwitchSection()
                            .WithLabels(
                                SingletonList<SwitchLabelSyntax>(
                                    CaseSwitchLabel(
                                        LiteralExpression(
                                            SyntaxKind.NumericLiteralExpression,
                                            Literal(stateIndex)))))
                            .WithStatements(
                                SingletonList(block as StatementSyntax)));

                        stateIndex++;
                    }
                }
                else
                {
                    var statements = new List<StatementSyntax>();
                    foreach (var block in blocks)
                    {
                        switch (block)
                        {
                            case StatementSyntax statementNode:
                                statements.Add(statementNode);
                                break;
                            case ExpressionSyntax expressionNode:
                                statements.Add(
                                    ExpressionStatement(expressionNode)
                                        .WithAdditionalAnnotations(
                                        new SyntaxAnnotation(Annotations.AnnotationKind,
                                            node.NodeAssetReference.GetInstanceID().ToString())));
                                break;
                            default:
                                throw new InvalidOperationException("Expected a statement or expression " +
                                    $"node, found a {node.GetType()} when building {block}");
                        }
                    }

                    states.Add(SwitchSection()
                        .WithLabels(
                            SingletonList<SwitchLabelSyntax>(
                                CaseSwitchLabel(
                                    LiteralExpression(
                                        SyntaxKind.NumericLiteralExpression,
                                        Literal(stateIndex)))))
                        .WithStatements(
                            SingletonList((StatementSyntax)Block(statements)
                                .AddStatements(
                                    RoslynEcsBuilder.BuildCoroutineNextState(),
                                    ReturnStatement(
                                        LiteralExpression(SyntaxKind.TrueLiteralExpression))))));

                    stateIndex++;
                }
            }

            foreach (var outputPort in stack.OutputPorts)
            {
                foreach (var connectedStack in outputPort.ConnectionPortModels)
                {
                    if (connectedStack.NodeModel is IStackModel nextStack)
                    {
                        BuildStack(translator, nextStack, ref variables, ref states, ref stateIndex);
                    }
                }
            }
        }
    }
}
