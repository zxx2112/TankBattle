using System;
using System.Collections.Generic;
using System.Linq;
using Packages.VisualScripting.Editor.Stencils;
using UnityEditor.VisualScripting.GraphViewModel;
using UnityEditor.VisualScripting.Model;
using UnityEditor.VisualScripting.Model.Stencils;

namespace Packages.VisualScripting.Editor.Stencils
{
    public interface IIteratorStackModel : IFunctionModel, ICriteriaModelContainer
    {
        ComponentQueryDeclarationModel ComponentQueryDeclarationModel { get; }
        VariableDeclarationModel ItemVariableDeclarationModel { get; }
    }

    interface IPrivateIteratorStackModel : IIteratorStackModel
    {
        IList<VariableDeclarationModel> FunctionParameters { get; }
        UpdateMode Mode { get; }
    }
}

public static class IteratorStackModelExtensions
{
    public static bool ContainsCoroutine(this IIteratorStackModel stack)
    {
        if (stack.NodeModels.Any(n => n is CoroutineNodeModel))
            return true;

        foreach (var outputPort in stack.OutputPorts)
            foreach (var connectedStack in outputPort.ConnectionPortModels)
                if (connectedStack.NodeModel is IStackModel nextStack
                    && nextStack.NodeModels.Any(n => n is CoroutineNodeModel))
                    return true;

        return false;
    }
}
