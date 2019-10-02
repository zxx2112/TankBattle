using JetBrains.Annotations;
using UnityEditor.Experimental.GraphView;

namespace UnityEditor.VisualScripting.Editor.Plugins
{
    public interface IPluginHandler
    {
        void Register(Store store, GraphView graphView);
        void Unregister();
    }

    [PublicAPI]
    public interface IRoslynPluginHandler : IPluginHandler
    {
        void Apply(ref Microsoft.CodeAnalysis.SyntaxTree syntaxTree, UnityEngine.VisualScripting.CompilationOptions options);
    }
}
