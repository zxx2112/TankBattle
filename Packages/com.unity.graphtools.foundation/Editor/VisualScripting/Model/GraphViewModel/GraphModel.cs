using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor.VisualScripting.Editor;
using UnityEditor.VisualScripting.Model;
using UnityEditor.VisualScripting.Model.Stencils;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

namespace UnityEditor.VisualScripting.GraphViewModel
{
    public abstract class GraphModel : ScriptableObject, IGraphModel
    {
        [SerializeField]
        ModelState m_State;
        [SerializeField]
        GraphAssetModel m_AssetModel;
        [SerializeField]
        protected List<AbstractNodeAsset> m_NodeModels;
        [SerializeField]
        protected List<EdgeModel> m_EdgeModels;

        [SerializeField]
        protected List<StickyNoteModel> m_StickyNoteModels;

        [SerializeField]
        Stencil m_Stencil;

        const float k_IOHorizontalOffset = 150;
        const float k_IOVerticalOffset = 40;

        public abstract IList<VariableDeclarationModel> VariableDeclarations { get; }

        public GraphChangeList LastChanges { get; private set; }
        IGraphChangeList IGraphModel.LastChanges => LastChanges;

        protected GraphModel()
        {
            LastChanges = new GraphChangeList();
        }

        public string Name => name;

        public ModelState State
        {
            get => m_State;
            set => m_State = value;
        }

        public IGraphAssetModel AssetModel
        {
            get => m_AssetModel;
            set => m_AssetModel = (GraphAssetModel)value;
        }

        Dictionary<GUID, INodeModel> m_NodesByGuid;

        public IReadOnlyDictionary<GUID, INodeModel> NodesByGuid => m_NodesByGuid ?? (m_NodesByGuid = new Dictionary<GUID, INodeModel>());

        public IReadOnlyList<INodeModel> NodeModels => new NodeAssetListAdapter<INodeModel>(m_NodeModels);
        public IReadOnlyList<IEdgeModel> EdgeModels => m_EdgeModels;
        public IEnumerable<IStickyNoteModel> StickyNoteModels => m_StickyNoteModels;

        public Stencil Stencil
        {
            get => m_Stencil;
            set => m_Stencil = value;
        }

        public enum DeleteConnections
        {
            False,
            True
        }

        public string GetAssetPath()
        {
            return AssetDatabase.GetAssetPath(m_AssetModel);
        }

        public virtual string GetUniqueName(string baseName)
        {
            return baseName;
        }

        public TNodeType CreateNode<TNodeType>(string nodeName = "", Vector2 position = default, SpawnFlags spawnFlags = SpawnFlags.Default, Action<TNodeType> preDefineSetup = null, GUID? guid = null) where TNodeType : NodeModel
        {
            return (TNodeType)CreateNode(typeof(TNodeType), nodeName, position, spawnFlags, preDefineSetup == null ? (Action<NodeModel>)null : n => preDefineSetup.Invoke((TNodeType)n), guid);
        }

        public INodeModel CreateNode(Type nodeTypeToCreate, string nodeName, Vector2 position, SpawnFlags spawnFlags = SpawnFlags.Default, Action<NodeModel> preDefineSetup = null, GUID? guid = null)
        {
            var node = CreateNodeInternal(nodeTypeToCreate, nodeName, position, spawnFlags, preDefineSetup, guid);
            ((NodeModel)node).DefineNode();
            return node;
        }

        // Create node without calling DefineNode method
        internal INodeModel CreateNodeInternal(Type nodeTypeToCreate, string nodeName, Vector2 position,
            SpawnFlags spawnFlags = SpawnFlags.Default, Action<NodeModel> preDefineSetup = null, GUID? guid = null)
        {
            if (nodeTypeToCreate == null)
                throw new InvalidOperationException("Cannot create node with a null type");
            NodeModel nodeModel;
            if (spawnFlags.IsSerializable())
                nodeModel = (NodeModel)SpawnNodeAsset(nodeTypeToCreate).Model;
            else
                nodeModel = (NodeModel)Activator.CreateInstance(nodeTypeToCreate);

            nodeModel.Title = nodeName ?? nodeTypeToCreate.Name;
            nodeModel.Position = position;
            nodeModel.Guid = guid ?? GUID.Generate();
            nodeModel.GraphModel = this;
            preDefineSetup?.Invoke(nodeModel);
            nodeModel.DefineNode();
            if (!spawnFlags.IsOrphan())
            {
                if (spawnFlags.IsUndoable())
                    AddNode(nodeModel);
                else
                    AddNodeNoUndo(nodeModel);
                EditorUtility.SetDirty(this);
            }
            return nodeModel;

            AbstractNodeAsset SpawnNodeAsset(Type typeToSpawn)
            {
                var genericNodeAssetType = typeof(NodeAsset<>).MakeGenericType(typeToSpawn);
                var derivedTypes = TypeCache.GetTypesDerivedFrom(genericNodeAssetType);

                if (derivedTypes.Count == 0)
                    throw new InvalidOperationException($"No NodeAssets of type NodeAsset<{typeToSpawn.Name}>");
                Assert.AreEqual(derivedTypes.Count, 1,
                    $"Multiple NodeAssets of type NodeAsset<{typeToSpawn.Name}> have been found");

                return CreateInstance(derivedTypes[0]) as AbstractNodeAsset;
            }
        }

        public void AddNode(INodeModel nodeModel)
        {
            Utility.SaveAssetIntoObject(nodeModel.NodeAssetReference, (Object)AssetModel);
            Undo.RegisterCompleteObjectUndo(this, "Add Node");
            AddNodeInternal(nodeModel);
            LastChanges?.ChangedElements.Add(nodeModel);
        }

        public void AddNodeNoUndo(INodeModel nodeModel)
        {
            Utility.SaveAssetIntoObject(nodeModel.NodeAssetReference, (Object)AssetModel);
            AddNodeInternal(nodeModel);
            LastChanges?.ChangedElements.Add(nodeModel);
        }

        void AddNodeInternal(INodeModel nodeModel)
        {
            ((NodeModel)nodeModel).GraphModel = this;
            m_NodeModels.Add(nodeModel.NodeAssetReference);
            m_NodesByGuid.Add(nodeModel.Guid, nodeModel);
        }

        public void DeleteNodes(IReadOnlyCollection<INodeModel> nodesToDelete, DeleteConnections deleteConnections)
        {
            // Let's delay stack node models at the very end, otherwise we might end up with
            // orphan stacked nodes during the process
            ILookup<bool, INodeModel> stackNodeLookup = nodesToDelete.ToLookup(x => x is IStackModel);

            foreach (var node in stackNodeLookup[false])
                DeleteNode(node, deleteConnections, false);

            foreach (var node in stackNodeLookup[true])
                DeleteNode(node, deleteConnections);
        }

        public void DeleteNode(INodeModel nodeModel, DeleteConnections deleteConnections, bool deleteWhenEmpty = true)
        {
            Undo.RegisterCompleteObjectUndo(this, "Delete Node");

            var model = (NodeModel)nodeModel;

            if (model.ParentStackModel?.NodeAssetReference != null)
            {
                LastChanges?.ChangedElements.Add(nodeModel.ParentStackModel);
                var parentStack = (StackBaseModel)model.ParentStackModel;
                Undo.RegisterCompleteObjectUndo(parentStack.NodeAssetReference, "RemoveNode");
                Undo.RegisterCompleteObjectUndo(nodeModel.NodeAssetReference, "Unparent Node");
                parentStack.RemoveStackedNode(model, deleteConnections == DeleteConnections.False ? StackBaseModel.EdgeBehaviourOnRemove.Transfer : StackBaseModel.EdgeBehaviourOnRemove.Ignore);
                EditorUtility.SetDirty(parentStack.NodeAssetReference);
                if (deleteWhenEmpty && parentStack.Capabilities.HasFlag(CapabilityFlags.DeletableWhenEmpty) &&
                    !parentStack.NodeModels.Any())
                    DeleteNode(parentStack, DeleteConnections.True);
            }
            else
            {
                if (LastChanges != null)
                    LastChanges.DeletedElements += 1;
                m_NodeModels.Remove(((INodeModel)model).NodeAssetReference);
                UnregisterNodeGuid(model.Guid);
            }

            if (deleteConnections == DeleteConnections.True)
            {
                DeleteEdges(nodeModel.GetConnectedEdges());
            }

            Undo.DestroyObjectImmediate(model.NodeAssetReference);
            model.Destroy();
        }

        internal void RegisterNodeGuid(INodeModel model)
        {
            m_NodesByGuid.Add(model.Guid, model);
        }

        internal void UnregisterNodeGuid(GUID nodeModelGuid)
        {
            m_NodesByGuid.Remove(nodeModelGuid);
        }

        internal void MoveNode(INodeModel nodeToMove, Vector2 newPosition)
        {
            var nodeModel = (NodeModel)nodeToMove;
            Undo.RecordObject(nodeModel.NodeAssetReference, "Move");
            nodeModel.Move(newPosition);
        }

        public IEdgeModel CreateEdge(IPortModel inputPort, IPortModel outputPort)
        {
            var existing = EdgesConnectedToPorts(inputPort, outputPort);
            if (existing != null)
                return existing;

            var edgeModel = CreateOrphanEdge(inputPort, outputPort);
            AddEdge(edgeModel, inputPort, outputPort);

            return edgeModel;
        }

        public IEdgeModel CreateOrphanEdge(IPortModel input, IPortModel output)
        {
            Assert.IsNotNull(input);
            Assert.IsNotNull(input.NodeModel);
            Assert.IsNotNull(output);
            Assert.IsNotNull(output.NodeModel);

            var edgeModel = new EdgeModel(this, input, output);

            input.NodeModel.OnConnection(input, output);
            output.NodeModel.OnConnection(output, input);

            return edgeModel;
        }

        void AddEdge(IEdgeModel edgeModel, IPortModel inputPort, IPortModel outputPort)
        {
            Undo.RegisterCompleteObjectUndo(this, "Add Edge");
            ((EdgeModel)edgeModel).GraphModel = this;
            m_EdgeModels.Add((EdgeModel)edgeModel);
            LastChanges?.ChangedElements.Add(edgeModel);
            LastChanges?.ChangedElements.Add(inputPort.NodeModel);
            LastChanges?.ChangedElements.Add(outputPort.NodeModel);
        }

        public void DeleteEdge(IPortModel input, IPortModel output)
        {
            DeleteEdges(m_EdgeModels.Where(x => x.InputPortModel == input && x.OutputPortModel == output));
        }

        public void DeleteEdge(IEdgeModel edgeModel)
        {
            Undo.RegisterCompleteObjectUndo(this, "Delete Edge");
            var model = (EdgeModel)edgeModel;

            edgeModel.InputPortModel?.NodeModel.OnDisconnection(edgeModel.InputPortModel, edgeModel.OutputPortModel);
            edgeModel.OutputPortModel?.NodeModel.OnDisconnection(edgeModel.OutputPortModel, edgeModel.InputPortModel);

            LastChanges?.ChangedElements.Add(edgeModel.InputPortModel?.NodeModel);
            LastChanges?.ChangedElements.Add(edgeModel.OutputPortModel?.NodeModel);

            m_EdgeModels.Remove(model);
            if (LastChanges != null)
            {
                LastChanges.DeleteEdgeModels.Add(model);
                LastChanges.DeletedElements += 1;
            }
        }

        public void DeleteEdges(IEnumerable<IEdgeModel> edgeModels)
        {
            var edgesCopy = edgeModels.ToList();
            foreach (var edgeModel in edgesCopy)
                DeleteEdge(edgeModel);
        }

        public IStickyNoteModel CreateStickyNote(Rect position, SpawnFlags dataSpawnFlags = SpawnFlags.Default)
        {
            var stickyNodeModel = (StickyNoteModel)CreateOrphanStickyNote(position);
            if (!dataSpawnFlags.IsOrphan())
                AddStickyNote(stickyNodeModel);

            return stickyNodeModel;
        }

        IStickyNoteModel CreateOrphanStickyNote(Rect position)
        {
            var stickyNodeModel = new StickyNoteModel();
            stickyNodeModel.Position = position;
            stickyNodeModel.GraphModel = this;

            return stickyNodeModel;
        }

        void AddStickyNote(IStickyNoteModel model)
        {
            var stickyNodeModel = (StickyNoteModel)model;

            Undo.RegisterCompleteObjectUndo(this, "Add Sticky Note");
            LastChanges?.ChangedElements.Add(stickyNodeModel);
            stickyNodeModel.GraphModel = this;
            m_StickyNoteModels.Add(stickyNodeModel);
        }

        void DeleteStickyNote(IStickyNoteModel stickyNoteModel)
        {
            Undo.RegisterCompleteObjectUndo(this, "Delete Sticky Note");
            var model = (StickyNoteModel)stickyNoteModel;

            m_StickyNoteModels.Remove(model);
            if (LastChanges != null)
                LastChanges.DeletedElements += 1;
        }

        protected virtual void OnEnable()
        {
            if (m_NodeModels == null)
                m_NodeModels = new List<AbstractNodeAsset>();
            m_NodesByGuid = new Dictionary<GUID, INodeModel>(m_NodeModels.Count);
            ForeachINodeModel(model =>
            {
                model.PostGraphLoad();
                m_NodesByGuid.Add(model.Guid, model);
            });

            if (m_EdgeModels == null)
                m_EdgeModels = new List<EdgeModel>();
            if (m_StickyNoteModels == null)
                m_StickyNoteModels = new List<StickyNoteModel>();
        }

        public void Dispose() {}

        void ForeachINodeModel(Action<INodeModel> action)
        {
            void TraverseNode(INodeModel node)
            {
                action(node);
                if (node is StackBaseModel stack)
                {
                    foreach (var stackedNode in stack.NodeModels)
                    {
                        TraverseNode(stackedNode);
                    }
                }
            }

            foreach (var node in NodeModels)
            {
                TraverseNode(node);
            }
        }

        public IEnumerable<IEdgeModel> GetEdgesConnections(IPortModel portModel)
        {
            return EdgeModels.Where(e => portModel.Direction == Direction.Input ? e.InputPortModel == portModel : e.OutputPortModel == portModel);
        }

        public IEnumerable<IEdgeModel> GetEdgesConnections(INodeModel node)
        {
            return EdgeModels.Where(e => ReferenceEquals(e.InputPortModel?.NodeModel, node)
                || ReferenceEquals(e.OutputPortModel?.NodeModel, node));
        }

        public IEnumerable<IEdgeModel> GetEdgesConnections(IEnumerable<IPortModel> portModels)
        {
            var models = new List<IEdgeModel>();
            foreach (var portModel in portModels)
            {
                models.AddRange(GetEdgesConnections(portModel));
            }

            return models;
        }

        public IEnumerable<IPortModel> GetConnections(IPortModel portModel)
        {
            return GetEdgesConnections(portModel).Select(e => portModel.Direction == Direction.Input ? e.OutputPortModel : e.InputPortModel)
                .Where(p => p != null);
        }

        public enum Verbosity
        {
            Errors,
            Verbose
        }

        public string FriendlyScriptName => TypeSystem.CodifyString(AssetModel.Name);

        public void DeleteStickyNotes(IStickyNoteModel[] stickyNotesToDelete)
        {
            foreach (IStickyNoteModel stickyNoteModel in stickyNotesToDelete)
                DeleteStickyNote(stickyNoteModel);
        }

        public void BypassNodes(INodeModel[] actionNodeModels)
        {
            foreach (var model in actionNodeModels)
            {
                var inputEdgeModels = GetEdgesConnections(model.InputsByDisplayOrder).ToList();
                var outputEdgeModels = GetEdgesConnections(model.OutputsByDisplayOrder).ToList();

                if (!inputEdgeModels.Any() || !outputEdgeModels.Any())
                    continue;

                DeleteEdges(inputEdgeModels);
                DeleteEdges(outputEdgeModels);

                CreateEdge(outputEdgeModels[0].InputPortModel, inputEdgeModels[0].OutputPortModel);
            }
        }

        public IEdgeModel EdgesConnectedToPorts(IPortModel input, IPortModel output)
        {
            return EdgeModels.FirstOrDefault(e => e.InputPortModel == input && e.OutputPortModel == output);
        }

        public IEnumerable<IEdgeModel> EdgesConnectedToPorts(IPortModel portModels)
        {
            return EdgeModels.Where(e => e.InputPortModel == portModels || e.OutputPortModel == portModels);
        }

        public void ResetChanges()
        {
            LastChanges = new GraphChangeList();
        }

        public void CleanUp()
        {
            m_NodeModels.RemoveAll(n => n == null);
            m_StickyNoteModels.RemoveAll(s => s == null);
            DeleteEdges(m_EdgeModels.Where(e => !e.IsValid()));
            m_EdgeModels.RemoveAll(e => e == null);
        }

        struct DeclarationIndex
        {
            public VariableDeclarationModel declarationModel;
            public int index;
        }

        internal MacroRefNodeModel ExtractNodesAsMacro(VSGraphModel macroGraphModel, Vector2 position, IEnumerable<IGraphElementModel> elementModels)
        {
            Undo.RegisterCompleteObjectUndo(this, "Extract Nodes to Macro");

            var elementModelList = elementModels.ToList();

            // duplicate selected nodes
            VseGraphView.Duplicate(macroGraphModel, elementModelList, out Dictionary<INodeModel, NodeModel> originalToMacro);

            // connect new node, etc.
            List<INodeModel> models = elementModelList.OfType<INodeModel>().ToList();
            int inputIndex = 0;
            int outputIndex = 0;

            // if the same node is connected to multiple extracted ports, only create one input in the macro
            // ie. if the same variable is connected to both ports of an Add node, the resulting macro
            // will have one input and return its double
            Dictionary<IPortModel, DeclarationIndex> existingNodesToCreatedOutputVariables = new Dictionary<IPortModel, DeclarationIndex>();
            Dictionary<IPortModel, DeclarationIndex> existingNodesToCreatedInputVariables = new Dictionary<IPortModel, DeclarationIndex>();

            // TODO this should be done in a more efficient way: GetEdgesConnections already goes through all the edges of the graph
            Dictionary<INodeModel, List<IEdgeModel>> inputEdgesPerNode = models.ToDictionary(m => m, m => GetEdgesConnections(m.InputsById.Values).ToList());
            Dictionary<INodeModel, List<IEdgeModel>> outputEdgesPerNode = models.ToDictionary(m => m, m => GetEdgesConnections(m.OutputsById.Values).ToList());
            foreach (INodeModel model in models)
            {
                List<IEdgeModel> inputEdgeModels = inputEdgesPerNode[model];
                List<IEdgeModel> outputEdgeModels = outputEdgesPerNode[model];

                foreach (IEdgeModel edge in inputEdgeModels)
                {
                    INodeModel connectedNode = edge.OutputPortModel.NodeModel;
                    if (models.Contains(connectedNode)) // connected to another extracted node
                        continue;

                    // create/reuse declaration in macro graph
                    if (!existingNodesToCreatedInputVariables.TryGetValue(edge.OutputPortModel, out DeclarationIndex macroInputDecl))
                    {
                        macroInputDecl = new DeclarationIndex
                        {
                            declarationModel = macroGraphModel.CreateGraphVariableDeclaration($"Input {inputIndex}", edge.OutputPortModel.DataType, true),
                            index = inputIndex++,
                        };
                        macroInputDecl.declarationModel.Modifiers = ModifierFlags.ReadOnly;

                        existingNodesToCreatedInputVariables.Add(edge.OutputPortModel, macroInputDecl);
                    }

                    // create variable in macro graph, connect with extracted node port
                    NodeModel macroNodeWithInput = originalToMacro[model];
                    IVariableModel macroInputVar = macroGraphModel.CreateVariableNode(macroInputDecl.declarationModel, macroNodeWithInput.Position + new Vector2(-k_IOHorizontalOffset, k_IOVerticalOffset));
                    macroGraphModel.CreateEdge(macroNodeWithInput.InputsById[edge.InputPortModel.UniqueId], macroInputVar.OutputPort);
                }

                HashSet<IPortModel> portModelsConnectedToOutputsInDefinition = new HashSet<IPortModel>();
                foreach (IEdgeModel edge in outputEdgeModels)
                {
                    INodeModel connectedNode = edge.InputPortModel.NodeModel;
                    if (models.Contains(connectedNode)) // connected to another extracted node
                        continue;

                    // create/reuse declaration in macro graph
                    if (!existingNodesToCreatedOutputVariables.TryGetValue(edge.OutputPortModel, out DeclarationIndex macroOutputDecl))
                    {
                        macroOutputDecl = new DeclarationIndex
                        {
                            declarationModel = macroGraphModel.CreateGraphVariableDeclaration($"Output {outputIndex}", edge.InputPortModel.DataType, true),
                            index = outputIndex++,
                        };
                        macroOutputDecl.declarationModel.Modifiers = ModifierFlags.WriteOnly;

                        existingNodesToCreatedOutputVariables.Add(edge.OutputPortModel, macroOutputDecl);
                    }

                    // create variable in macro graph, connect with extracted node port
                    NodeModel macroNodeWithOutput = originalToMacro[model];
                    // if the extracted node output was connected to two ports, we do need two edges at the call site,
                    // but not two outputs+two edges in the definition
                    var outputPort = macroNodeWithOutput.OutputsById[edge.OutputPortModel.UniqueId];
                    if (portModelsConnectedToOutputsInDefinition.Add(outputPort))
                    {
                        IVariableModel macroOutputVar = macroGraphModel.CreateVariableNode(macroOutputDecl.declarationModel, macroNodeWithOutput.Position + new Vector2(k_IOHorizontalOffset * 2, k_IOVerticalOffset));
                        macroGraphModel.CreateEdge(macroOutputVar.OutputPort, outputPort);
                    }
                }
            }

            // create new macroRefNode
            Action<MacroRefNodeModel> preDefineSetup = n =>
            {
                MacroRefNodeModel macroNode = n;
                macroNode.Macro = macroGraphModel;
            };
            MacroRefNodeModel macroRefNodeModel =
                CreateNode("MyMacro", position, SpawnFlags.Default, preDefineSetup);

            HashSet<VariableDeclarationModel> declarationModelsDone = new HashSet<VariableDeclarationModel>();
            foreach (INodeModel model in models)
            {
                List<IEdgeModel> inputEdgeModels = inputEdgesPerNode[model];
                List<IEdgeModel> outputEdgeModels = outputEdgesPerNode[model];

                foreach (IEdgeModel edge in inputEdgeModels)
                {
                    if (models.Contains(edge.OutputPortModel.NodeModel))
                        continue;

                    VariableDeclarationModel decl = existingNodesToCreatedInputVariables[edge.OutputPortModel].declarationModel;
                    if (declarationModelsDone.Contains(decl)) // already done
                        continue;

                    CreateEdge(macroRefNodeModel.InputsById[decl.VariableName], edge.OutputPortModel);
                    declarationModelsDone.Add(decl);
                }

                foreach (IEdgeModel edge in outputEdgeModels)
                {
                    if (models.Contains(edge.InputPortModel.NodeModel))
                        continue;

                    // no declarationModelsDone check, we need to process every single edge here (case where one
                    // macro output is connected to multiple call site inputs)
                    var decl = existingNodesToCreatedOutputVariables[edge.OutputPortModel].declarationModel;
                    CreateEdge(edge.InputPortModel, macroRefNodeModel.OutputsById[decl.VariableName]);
                    declarationModelsDone.Add(decl);
                }
            }

            // delete selected nodes
            DeleteNodes(models, DeleteConnections.True);

            return macroRefNodeModel;
        }

        public INodeModel ExtractNodesAsFunction(IList<ISelectable> selection)
        {
            Undo.RegisterCompleteObjectUndo(this, "Extract Nodes to Function");

            // create new functionNode
            FunctionModel functionModel = (this as VSGraphModel).CreateFunction("MyFunction", new Vector2(1300, 0));

            // paste actionNodeModels
            var json = VseGraphView.OnSerializeGraphElements(selection.OfType<GraphElement>());
            TargetInsertionInfo info;
            info.Delta = functionModel.Position;
            info.OperationName = "Paste";
            info.TargetStack = functionModel;
            info.TargetStackInsertionIndex = 0;
            VseGraphView.OnUnserializeAndPaste(this as VSGraphModel, info, null, json);

            // connect new node, etc.
            Dictionary<IGraphElementModel, IHasGraphElementModel> selectedModels = selection
                .OfType<IHasGraphElementModel>()
                .ToDictionary(x => x.GraphElementModel);
            var models = selectedModels.Keys.OfType<INodeModel>().ToList();
            var elementInStack = models.FirstOrDefault(x => x.ParentStackModel != null);
            var newElementInStack = functionModel.NodeModels.FirstOrDefault();

            var inputEdgeModels = GetEdgesConnections(elementInStack?.InputsByDisplayOrder).ToList();

            foreach (var edge in inputEdgeModels)
            {
                if (models.Contains(edge.OutputPortModel.NodeModel))
                    continue;

                if (edge.InputPortModel.PortType == PortType.Instance)
                    continue;

                // create variables

                var parameterModel = functionModel.CreateAndRegisterFunctionParameterDeclaration(edge.InputPortModel.Name,
                    edge.OutputPortModel.DataType);

                // create variable pill
                var pill = (this as VSGraphModel).CreateVariableNode(parameterModel,
                    functionModel.Position);

                // connect it to the port on the element
                CreateEdge(newElementInStack?.InputsById[edge.InputPortModel.UniqueId], pill.OutputPort);
            }

            // create new functionRefCallNode
            int index = 0;
            foreach (var node in elementInStack?.ParentStackModel.NodeModels ?? Enumerable.Empty<INodeModel>())
            {
                if (node == elementInStack)
                    break;
                index++;
            }

            var refCallNode = (elementInStack?.ParentStackModel as StackBaseModel).CreateFunctionRefCallNode(functionModel, index);

            // connect nodes to ref call node
            foreach (var edge in inputEdgeModels)
            {
                if (models.Contains(edge.OutputPortModel.NodeModel))
                    continue;

                // connect it to the port on the element
                CreateEdge(refCallNode.InputsById[edge.InputPortModel.UniqueId], edge.OutputPortModel);
            }

            // delete selected nodes
            DeleteNodes(models, DeleteConnections.True);

            return functionModel;
        }

        public INodeModel ConvertNodeToFunction(INodeModel nodeToConvert)
        {
            // create new functionNode
            FunctionModel functionModel = (this as VSGraphModel).CreateFunction("MyFunction", nodeToConvert.Position);

            // go through all stack connected to StackNode and connect to new functionModel
            // remove from previous stack
            var parentStack = (StackBaseModel)nodeToConvert.ParentStackModel;

            Undo.RegisterCompleteObjectUndo(parentStack.NodeAssetReference, "RemoveNode");
            Undo.RegisterCompleteObjectUndo(nodeToConvert.NodeAssetReference, "Unparent Node");
            parentStack.RemoveStackedNode(nodeToConvert);
            EditorUtility.SetDirty(parentStack.NodeAssetReference);

            // delete old stack
            DeleteNode(parentStack, DeleteConnections.True);

            // add to new functionModel
            Undo.RegisterCompleteObjectUndo(functionModel.NodeAssetReference, "Add Stacked Node");
            functionModel.AddStackedNode(nodeToConvert, 0);

            return functionModel;
        }
    }
}
