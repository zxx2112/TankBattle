using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using UnityEditor.VisualScripting.GraphViewModel;
using UnityEditor.VisualScripting.Model.Translators;
using UnityEngine;
using UnityEngine.Assertions;
using VisualScripting.Entities.Runtime;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace UnityEditor.VisualScripting.Model.Stencils
{
    [Serializable]
    public class CoroutineNodeModel : LoopNodeModel
    {
        [SerializeField]
        TypeHandle m_CoroutineType;

        public TypeHandle CoroutineType
        {
            get => m_CoroutineType;
            set
            {
                Assert.IsTrue(typeof(ICoroutine).IsAssignableFrom(value.Resolve(Stencil)),
                    $"The type {value} does not implement ICoroutine.");
                m_CoroutineType = value;
            }
        }

        public MethodInfo MethodInfo { get; private set; }
        public string VariableName => $"m_{CoroutineType.Name(Stencil)}";
        public override string InsertLoopNodeTitle => CoroutineType.Name(Stencil);
        public override bool IsInsertLoop => true;
        public override LoopConnectionType LoopConnectionType => LoopConnectionType.LoopStack;
        public override Type MatchingStackType => typeof(CoroutineStackModel);
        public Dictionary<string, FieldDeclarationSyntax> Fields { get; } =
            new Dictionary<string, FieldDeclarationSyntax>();

        protected override void OnDefineNode()
        {
            base.OnDefineNode();

            var type = CoroutineType.Resolve(Stencil);
            MethodInfo = type.GetMethod("MoveNext");
            AddField(type, VariableName, AccessibilityFlags.Private);

            AddField(typeof(float), nameof(ICoroutine.DeltaTime), AccessibilityFlags.Public);

            foreach (var field in type.GetFields())
                AddDataInput(field.Name, field.FieldType.GenerateTypeHandle(Stencil));
        }

        void AddField(Type variableType, string variableName, AccessibilityFlags variableAccessibility)
        {
            if (!Fields.ContainsKey(variableName))
                Fields.Add(variableName, RoslynBuilder.DeclareField(variableType, variableName, variableAccessibility));
        }
    }

    [GraphtoolsExtensionMethods]
    public static class CoroutineTranslator
    {
        public static IEnumerable<SyntaxNode> BuildCoroutine(this RoslynEcsTranslator translator,
            CoroutineNodeModel model, IPortModel portModel)
        {
            var nodeName = model.CoroutineType.Name(translator.Stencil);
            if (!(translator.context is CoroutineContext))
            {
                translator.AddError(model, $"{nodeName} node is not allowed in a static function");
                yield break;
            }

            // TODO Remove this to enable nested coroutines (need to be fixed first)
            if (model.ParentStackModel is LoopStackModel)
            {
                translator.AddError(model, $"{nodeName} node is not allowed in an execution stack yet. This will come in a further release.");
                yield break;
            }

            yield return RoslynEcsBuilder.BuildCoroutineState(
                BuildInitState(model, translator)
                    .Concat(Enumerable.Repeat(RoslynEcsBuilder.BuildCoroutineNextState(), 1))
                    .ToArray());

            var deltaTimeStatement = ExpressionStatement(
                RoslynBuilder.Assignment(
                    RoslynBuilder.MemberReference(
                        IdentifierName(model.VariableName),
                        nameof(ICoroutine.DeltaTime)),
                    IdentifierName(nameof(ICoroutine.DeltaTime))));

            var moveNextInvocation = RoslynBuilder.MethodInvocation(
                "MoveNext",
                IdentifierName(model.VariableName),
                Enumerable.Empty<ArgumentSyntax>(),
                null);

            if (model.OutputPort.ConnectionPortModels.FirstOrDefault()?.NodeModel is LoopStackModel loopStack)
            {
                var block = Block();
                block = RoslynTranslatorExtensions.BuildLocalDeclarations(translator, loopStack)
                    .Aggregate(block, (current, localDeclaration) => current.AddStatements(localDeclaration));

                translator.BuildStack(loopStack, ref block, StackExitStrategy.Continue);
                yield return RoslynEcsBuilder.BuildCoroutineState(
                    deltaTimeStatement,
                    IfStatement(moveNextInvocation, block)
                        .WithElse(
                        ElseClause(
                            Block(RoslynEcsBuilder.BuildCoroutineNextState()))));
                yield break;
            }

            yield return RoslynEcsBuilder.BuildCoroutineState(
                deltaTimeStatement,
                IfStatement(
                    PrefixUnaryExpression(SyntaxKind.LogicalNotExpression, moveNextInvocation),
                    RoslynEcsBuilder.BuildCoroutineNextState()));
        }

        static IEnumerable<StatementSyntax> BuildInitState(CoroutineNodeModel model, RoslynEcsTranslator translator)
        {
            if (model.MethodInfo?.DeclaringType == null)
                yield break;

            int i = 0;
            foreach (var field in model.MethodInfo.DeclaringType.GetFields())
            {
                yield return ExpressionStatement(
                    RoslynBuilder.Assignment(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName(model.VariableName),
                            IdentifierName(field.Name)),
                        translator.BuildPort(model.InputsByDisplayOrder[i++]).Single()
                    )
                );
            }
        }

        public static string MakeExcludeCoroutineQueryName(RoslynEcsTranslator.IterationContext iterationContext)
        {
            return $"{iterationContext.GroupName}ExcludeCoroutine";
        }
    }
}
