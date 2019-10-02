using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEditor.VisualScripting.Editor;
using UnityEditor.VisualScripting.GraphViewModel;
using UnityEditor.VisualScripting.Model.Stencils;
using UnityEditor.VisualScripting.Model.Translators;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.VisualScripting;
using Object = UnityEngine.Object;

using UnityEditor.VisualScripting.Editor.Plugins;

namespace UnityEditor.VisualScripting.Model
{
    [PublicAPI]
    public class VSGraphModel : GraphModel, IVSGraphModel
    {
        [SerializeField]
        List<VariableDeclarationModel> m_GraphVariableModels = new List<VariableDeclarationModel>();

        public IEnumerable<IVariableDeclarationModel> GraphVariableModels => m_GraphVariableModels;
        public override IList<VariableDeclarationModel> VariableDeclarations => m_GraphVariableModels;

        public IEnumerable<IStackModel> StackModels => NodeModels.OfType<IStackModel>();

        protected override void OnEnable()
        {
            base.OnEnable();

            foreach (var field in m_GraphVariableModels)
            {
                if (field)
                    field.GraphModel = this;
                else
                    Debug.LogError("Null graphVariableModels in graph. Should only happen during tests for some reason");
            }

            // temp code following the fix for VSB-213, delete soon
            foreach (FunctionModel functionModel in StackModels.OfType<FunctionModel>())
            {
                foreach (var functionVariable in functionModel.FunctionVariableModels.Cast<VariableDeclarationModel>())
                    functionVariable.MigrateFunctionModel();
                foreach (var functionVariable in functionModel.FunctionParameterModels.Cast<VariableDeclarationModel>())
                    functionVariable.MigrateFunctionModel();
            }
        }

        public VariableDeclarationModel CreateGraphVariableDeclaration(string variableName, TypeHandle variableDataType, bool isExposed)
        {
            var field = VariableDeclarationModel.Create(variableName, variableDataType, isExposed, this, VariableType.GraphVariable, ModifierFlags.None, null);
            Undo.RegisterCompleteObjectUndo(this, "Create Graph Variable");
            m_GraphVariableModels.Add(field);
            return field;
        }

        public void ReorderGraphVariableDeclaration(IVariableDeclarationModel variableDeclarationModel, int index)
        {
            Assert.IsTrue(index >= 0);

            Undo.RegisterCompleteObjectUndo(this, "Reorder Graph Variable Declaration");

            var varDeclarationModel = (VariableDeclarationModel)variableDeclarationModel;
            if (varDeclarationModel.VariableType == VariableType.GraphVariable)
            {
                var oldIndex = m_GraphVariableModels.IndexOf(varDeclarationModel);
                m_GraphVariableModels.RemoveAt(oldIndex);
                if (index > oldIndex) index--;    // the actual index could have shifted due to the removal
                if (index >= m_GraphVariableModels.Count)
                    m_GraphVariableModels.Add(varDeclarationModel);
                else
                    m_GraphVariableModels.Insert(index, varDeclarationModel);
                LastChanges.ChangedElements.Add(variableDeclarationModel);
                LastChanges.DeletedElements++;
            }
        }

        public void DeleteVariableDeclarations(IEnumerable<VariableDeclarationModel> variableModels, bool deleteUsages)
        {
            Undo.RegisterCompleteObjectUndo(this, "Remove Variable Declarations");

            foreach (VariableDeclarationModel variableModel in variableModels)
            {
                if (LastChanges != null)
                {
                    LastChanges.BlackBoardChanged = true;
                    if (variableModel.Owner is IFunctionModel fun)
                        LastChanges.ChangedElements.Add(fun);
                }
                if (variableModel.VariableType == VariableType.GraphVariable || variableModel.VariableType == VariableType.ComponentQueryField)
                {
                    m_GraphVariableModels.Remove(variableModel);
                }
                else if (variableModel.VariableType == VariableType.FunctionVariable)
                {
                    var functionModel = ((FunctionModel)variableModel.FunctionModel);
                    Assert.IsNotNull(functionModel, "Function Variable must reference the invokable owning them");
                    Undo.RegisterCompleteObjectUndo(functionModel.NodeAssetReference, "Remove Function Variable");
                    functionModel.RemoveFunctionVariableDeclaration(variableModel);
                }
                else if (variableModel.VariableType == VariableType.FunctionParameter)
                {
                    var functionModel = ((FunctionModel)variableModel.FunctionModel);
                    Assert.IsNotNull(functionModel, "Function Parameter must reference the invokable owning them");
                    Undo.RegisterCompleteObjectUndo(functionModel.NodeAssetReference, "Remove Function Parameter");
                    functionModel.RemoveFunctionParameterDeclaration(variableModel);
                }

                if (deleteUsages)
                {
                    var nodesToDelete = FindUsages(variableModel).Cast<INodeModel>().ToList();
                    DeleteNodes(nodesToDelete, DeleteConnections.True);
                }
                Undo.DestroyObjectImmediate(variableModel);
            }
        }

        public void MoveVariableDeclaration(IVariableDeclarationModel variableDeclarationModel, IHasVariableDeclaration destination)
        {
            var currentOwner = variableDeclarationModel.Owner;
            var model = (VariableDeclarationModel)variableDeclarationModel;

            Undo.RegisterCompleteObjectUndo(GetUnityObjectBehindInstance(currentOwner), "Move Variable Declaration");
            Undo.RegisterCompleteObjectUndo(GetUnityObjectBehindInstance(destination), "Move Variable Declaration");
            Undo.RegisterCompleteObjectUndo(model, "Move Variable Declaration");

            currentOwner.VariableDeclarations.Remove(model);
            destination.VariableDeclarations.Add(model);
            LastChanges.ChangedElements.Add(model);
            model.Owner = destination;

            Object GetUnityObjectBehindInstance(IHasVariableDeclaration container)
            {
                if (container is INodeModel nodeModel)
                    return nodeModel.NodeAssetReference;
                if (container is VSGraphModel vsGraphModel)
                    return vsGraphModel;
                return null;
            }
        }

//        public virtual string GetUniqueName(string baseName)
//        {
        // TODO: fixme - kept for later
//            var roslynTranslator = stencil.CreateTranslator() as RoslynTranslator;
//            if (roslynTranslator == null)
//                return baseName;
//            var syntaxTree = roslynTranslator.Translate(this, CompilationOptions.Default);
//            return UniqueNameGenerator.CreateUniqueVariableName(syntaxTree, baseName);
//            return baseName;
//        }

        public IEnumerable<VariableNodeModel> FindUsages(VariableDeclarationModel decl)
        {
            return decl.FindReferencesInGraph().Cast<VariableNodeModel>();
        }

        public CompilationResult Compile(AssemblyType assemblyType, ITranslator translator, CompilationOptions compilationOptions, IEnumerable<IPluginHandler> pluginHandlers = null)
        {
            Stencil.PreProcessGraph(this);
            CompilationResult result;

            try
            {
                result = translator.TranslateAndCompile(this, assemblyType, compilationOptions);

                if (result.status == CompilationStatus.Failed)
                {
                    Stencil.OnCompilationFailed(this, result);
                }
                else
                {
                    Stencil.OnCompilationSucceeded(this, result);
                }
            }
            catch (Exception e)
            {
                result = null;
                Debug.LogException(e);
            }

            return result;
        }

        public bool CheckIntegrity(Verbosity errors)
        {
            //Assert.IsTrue((UnityEngine.Object)assetModel, "graph asset is invalid");
            for (var i = 0; i < m_EdgeModels.Count; i++)
            {
                var edge = m_EdgeModels[i];
                Assert.IsNotNull(edge.InputPortModel, $"Edge {i} input is null, output: {edge.OutputPortModel}");
                Assert.IsNotNull(edge.OutputPortModel, $"Edge {i} output is null, input: {edge.InputPortModel}");
            }
            CheckNodeList(NodeModels);
            if (errors == Verbosity.Verbose)
                Debug.Log("Integrity check succeeded");
            return true;
        }

        void CheckNodeList(IReadOnlyList<INodeModel> nodeModels, Dictionary<GUID, int> existingGuids = null)
        {
            if (existingGuids == null)
                existingGuids = new Dictionary<GUID, int>(nodeModels.Count * 4); // wild guess of total number of nodes, including stacked nodes
            for (var i = 0; i < nodeModels.Count; i++)
            {
                INodeModel node = nodeModels[i];

                Assert.IsTrue(node.NodeAssetReference != null, $"Node asset {i} is null");
                Assert.IsNotNull(node, $"Node {i} is null");
                Assert.IsTrue(AssetModel.IsSameAsset(node.AssetModel), $"Node {i} asset is not matching its actual asset");
                Assert.IsFalse(node.Guid.Empty(), $"Node {i} ({node.GetType()}) has an empty Guid");
                Assert.IsFalse(existingGuids.TryGetValue(node.Guid, out var oldIndex), $"duplicate GUIDs: Node {i} ({node.GetType()}) and Node {oldIndex}");
                existingGuids.Add(node.Guid, i);

                if (node.Destroyed)
                    continue;
                CheckNodePorts(node.InputsById);
                CheckNodePorts(node.OutputsById);
                if (node is IStackModel stackModel)
                    CheckNodeList(stackModel.NodeModels, existingGuids);
            }
        }

        static void CheckNodePorts(IReadOnlyDictionary<string, IPortModel> portsById)
        {
            foreach (var kv in portsById)
            {
                string portId = portsById[kv.Key].UniqueId;
                Assert.AreEqual(kv.Key, portId, $"Node {kv.Key} port and its actual id {portId} mismatch");
            }
        }

        public void QuickCleanup()
        {
            for (var i = m_EdgeModels.Count - 1; i >= 0; i--)
            {
                var edge = m_EdgeModels[i];
                if (edge?.InputPortModel == null || edge.OutputPortModel == null)
                    m_EdgeModels.RemoveAt(i);
            }
            var models = m_NodeModels.Select(asset => asset.Model).ToList();
            CleanupNodes(models);
            m_NodeModels = models.Select(model => model.NodeAssetReference).ToList();
        }

        static void CleanupNodes(IList<INodeModel> models)
        {
            for (var i = models.Count - 1; i >= 0; i--)
            {
                if (models[i].Destroyed)
                    models.RemoveAt(i);
                else if (models[i] is IStackModel stack)
                    CleanupNodes(stack.NodeModels as NodeAssetListAdapter<INodeModel>);
            }
        }

        public string SourceFilePath => Stencil.GetSourceFilePath(this);

        public string TypeName => TypeSystem.CodifyString(AssetModel.Name);

        public ITranslator CreateTranslator()
        {
            return Stencil.CreateTranslator();
        }

        public IEnumerable<INodeModel> GetAllNodes()
        {
            return StackModels.Union(NodeModels).Concat(StackModels.SelectMany(s => s.NodeModels));
        }

        public List<VariableDeclarationModel> DuplicateGraphVariableDeclarations(List<IVariableDeclarationModel> variableDeclarationModels)
        {
            List<VariableDeclarationModel> duplicatedModels = new List<VariableDeclarationModel>();
            foreach (IVariableDeclarationModel original in variableDeclarationModels)
            {
                if (original.VariableType != VariableType.GraphVariable)
                    continue;
                string uniqueName = GetUniqueName(original.Name);
                VariableDeclarationModel copy = Instantiate((VariableDeclarationModel)original);
                copy.name = uniqueName;
                if (copy.InitializationModel != null)
                {
                    copy.CreateInitializationValue();
                    ((ConstantNodeModel)copy.InitializationModel).ObjectValue = original.InitializationModel.ObjectValue;
                }

                Undo.RegisterCreatedObjectUndo(copy, "Duplicate variable");
                Utility.SaveAssetIntoObject(copy, (Object)AssetModel);

                duplicatedModels.Add(copy);
            }

            Undo.RegisterCompleteObjectUndo(this, "Create Graph Variables");
            m_GraphVariableModels.AddRange(duplicatedModels);

            return duplicatedModels;
        }

        public IEnumerable<INodeModel> GetEntryPoints()
        {
            return Stencil.GetEntryPoints(this);
        }
    }
}
