using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor.Experimental.GraphView;
using UnityEditor.VisualScripting.Editor;
using UnityEditor.VisualScripting.Model;
using UnityEditor.VisualScripting.Model.Stencils;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Profiling;
using Object = UnityEngine.Object;
using Port = UnityEditor.Experimental.GraphView.Port;

namespace UnityEditor.VisualScripting.GraphViewModel
{
    [Serializable]
    public abstract class NodeModel : INodeModel, ISerializationCallbackReceiver
    {
        [SerializeField]
        string m_GuidAsString; // serialize m_Guid
        GUID m_Guid; // serialized as m_GuidAsString
        [SerializeField]
        AbstractNodeAsset m_AssetReference;
        [SerializeField]
        GraphModel m_GraphModel;
        [SerializeField]
        AbstractNodeAsset m_ParentStackModel;
        [SerializeField]
        Vector2 m_Position;
        [SerializeField, HideInInspector]
        Color m_Color = new Color(0.776f, 0.443f, 0, 0.5f);
        [SerializeField, HideInInspector]
        bool m_HasUserColor;
        [SerializeField]
        string m_Title;

        // Serialize m_InputConstantsById dictionary Keys
        [SerializeField, HideInInspector]
        List<string> m_InputConstantKeys;

        // Serialize m_InputConstantsById dictionary Values
        [SerializeField, HideInInspector]
        protected List<AbstractNodeAsset> m_InputConstantsValues;
        [SerializeField]
        ModelState m_State;

        public GUID Guid
        {
            get => m_Guid;
            internal set => m_Guid = value;
        }

        protected Stencil Stencil => m_GraphModel.Stencil;

        public virtual string IconTypeString => "typeNode";

        public virtual string DataTypeString
        {
            get
            {
                IVariableDeclarationModel declarationModel = (this as IVariableModel)?.DeclarationModel;
                return declarationModel?.DataType.GetMetadata(Stencil).FriendlyName ?? string.Empty;
            }
        }

        public virtual string VariableString
        {
            get
            {
                IVariableDeclarationModel declarationModel = (this as IVariableModel)?.DeclarationModel;
                return declarationModel == null ? string.Empty : declarationModel.IsExposed ? "Exposed variable" : "Variable";
            }
        }

        // Capabilities
        public virtual CapabilityFlags Capabilities => CapabilityFlags.Selectable | CapabilityFlags.Deletable | CapabilityFlags.Movable | CapabilityFlags.Droppable;

        public ScriptableObject SerializableAsset => NodeAssetReference;
        public IGraphAssetModel AssetModel => GraphModel?.AssetModel;

        public IGraphModel GraphModel
        {
            get => m_GraphModel;
            set => m_GraphModel = (GraphModel)value;
        }

        public ModelState State
        {
            get => m_State;
            set => m_State = value;
        }

        public AbstractNodeAsset NodeAssetReference
        {
            get => m_AssetReference;
            set
            {
                m_AssetReference = value;
                OriginalInstanceId = value.GetInstanceID();
            }
        }

        public IStackModel ParentStackModel
        {
            get => m_ParentStackModel == null ? null : m_ParentStackModel.Model as IStackModel;
            set => m_ParentStackModel = value?.NodeAssetReference;
        }

        public virtual string Title
        {
            get => m_Title;
            set => m_Title = value;
        }

        public Vector2 Position
        {
            get => m_Position;
            set => m_Position = value;
        }

        public virtual bool IsCondition => false;
        public virtual bool IsInsertLoop => false;
        public virtual LoopConnectionType LoopConnectionType => LoopConnectionType.None;

        public bool IsBranchType => GetType().GetCustomAttribute<BranchedNodeAttribute>() != null;

        public Color Color
        {
            get => m_HasUserColor ? m_Color : Color.clear;
            set => m_Color = value;
        }

        // Allows maintaining a ports both by order and by their ids
        // IReadOnlyList<IPortModel> gives access to ports by display order
        // IReadOnlyDictionary<string, IPortModel> gives access to ports by Ids
        class OrderedPorts : IReadOnlyDictionary<string, IPortModel>, IReadOnlyList<IPortModel>
        {
            Dictionary<string, IPortModel> m_Dictionary;
            List<int> m_Order;
            List<IPortModel> m_PortModels;

            public OrderedPorts(int capacity = 0)
            {
                m_Dictionary = new Dictionary<string, IPortModel>(capacity);
                m_Order = new List<int>(capacity);
                m_PortModels = new List<IPortModel>(capacity);
            }

            public void Add(IPortModel portModel)
            {
                m_Dictionary.Add(portModel.UniqueId, portModel);
                m_PortModels.Add(portModel);
                m_Order.Add(m_Order.Count);
            }

            public void SwapPortsOrder(IPortModel a, IPortModel b)
            {
                int indexA = m_PortModels.IndexOf(a);
                int indexB = m_PortModels.IndexOf(b);
                int oldAOrder = m_Order[indexA];
                m_Order[indexA] = m_Order[indexB];
                m_Order[indexB] = oldAOrder;
            }

            #region IReadOnlyDictionary implementation
            public IEnumerator<KeyValuePair<string, IPortModel>> GetEnumerator() => m_Dictionary.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public int Count => m_Dictionary.Count;
            public bool ContainsKey(string key) => m_Dictionary.ContainsKey(key);

            public bool TryGetValue(string key, out IPortModel value)
            {
                return m_Dictionary.TryGetValue(key, out value);
            }

            public IPortModel this[string key] => m_Dictionary[key];

            public IEnumerable<string> Keys => m_Dictionary.Keys;
            public IEnumerable<IPortModel> Values => m_Dictionary.Values;
            #endregion IReadOnlyDictionary implementation

            #region IReadOnlyList<IPortModel> implementation
            IEnumerator<IPortModel> IEnumerable<IPortModel>.GetEnumerator()
            {
                Assert.AreEqual(m_Order.Count, m_PortModels.Count, "these lists are supposed to always be of the same size");
                return m_Order.Select(i => m_PortModels[i]).GetEnumerator();
            }

            public IPortModel this[int index] => m_PortModels[m_Order[index]];
            #endregion IReadOnlyList<IPortModel> implementation
        }

        OrderedPorts m_InputsById = new OrderedPorts();
        OrderedPorts m_OutputsById = new OrderedPorts();
        OrderedPorts m_PreviousInputs;
        OrderedPorts m_PreviousOutputs;

        Dictionary<string, ConstantNodeModel> m_InputConstantsById = new Dictionary<string, ConstantNodeModel>();

        public IReadOnlyDictionary<string, IPortModel> InputsById => m_InputsById;
        public IReadOnlyDictionary<string, IPortModel> OutputsById => m_OutputsById;

        public IReadOnlyDictionary<string, ConstantNodeModel> InputConstantsById => m_InputConstantsById;

        public virtual bool IsStacked => m_ParentStackModel != null;
        public virtual IReadOnlyList<IPortModel> InputsByDisplayOrder => m_InputsById;

        public virtual IReadOnlyList<IPortModel> OutputsByDisplayOrder => m_OutputsById;

        [Obsolete("Direct indexing dropped, use InputsById or InputsByDisplayOrder instead")]
        public virtual IReadOnlyList<IPortModel> InputPortModels => new List<IPortModel>();
        [Obsolete("Direct indexing dropped, use OutputsById or OutputsByDisplayOrder instead")]
        public virtual IReadOnlyList<IPortModel> OutputPortModels => new List<IPortModel>();

        public bool HasUserColor
        {
            get => m_HasUserColor;
            set => m_HasUserColor = value;
        }

        public int OriginalInstanceId { get; set; }

        public virtual void OnConnection(IPortModel selfConnectedPortModel, IPortModel otherConnectedPortModel)
        {
        }

        public virtual void OnDisconnection(IPortModel selfConnectedPortModel, IPortModel otherConnectedPortModel)
        {
        }

        public void PostGraphLoad()
        {
            if (m_Guid.Empty())
                m_Guid = GUID.Generate();
        }

        protected virtual void OnPreDefineNode()
        {
        }

        public void DefineNode()
        {
            OnPreDefineNode();

            m_PreviousInputs = m_InputsById;
            m_PreviousOutputs = m_OutputsById;
            m_InputsById = new OrderedPorts(m_InputsById.Count);
            m_OutputsById = new OrderedPorts(m_OutputsById.Count);

            OnDefineNode();

            RemoveUnusedPorts();

            if (NodeAssetReference != null)
                NodeAssetReference.name = Title;
        }

        void RemoveUnusedPorts()
        {
            foreach (var kv in m_PreviousInputs
                     .Where<KeyValuePair<string, IPortModel>>(kv => !m_InputsById.ContainsKey(kv.Key)))
            {
                DeletePort(kv.Value);
            }

            foreach (var kv in m_PreviousOutputs
                     .Where<KeyValuePair<string, IPortModel>>(kv => !m_OutputsById.ContainsKey(kv.Key)))
            {
                DeletePort(kv.Value);
            }

            // remove input constants that aren't used
            var idsToDeletes = m_InputConstantsById
                .Select(kv => kv.Key)
                .Where(id => !m_InputsById.ContainsKey(id)).ToList();
            foreach (var id in idsToDeletes)
            {
                m_InputConstantsById[id].Destroy();
                m_InputConstantsById.Remove(id);
            }
        }

        static PortModel ReuseOrCreatePortModel(PortModel model, IReadOnlyDictionary<string, IPortModel> previousPorts, OrderedPorts newPorts)
        {
            // reuse existing ports when ids match, otherwise add port
            string id = model.UniqueId;
            PortModel modelToAdd = model;
            if (previousPorts.TryGetValue(id, out var existingModel))
            {
                modelToAdd = (PortModel)existingModel;
                modelToAdd.Name = model.Name;
                modelToAdd.DataType = model.DataType;
                modelToAdd.PortType = model.PortType;
            }
            newPorts.Add(modelToAdd);
            return modelToAdd;
        }

        protected virtual void OnDefineNode()
        {
        }

        public void UndoRedoPerformed()
        {
            Profiler.BeginSample("NodeModel_UndoRedo");
            DefineNode();
            Profiler.EndSample();
        }

        public virtual Port.Capacity GetPortCapacity(PortModel portModel)
        {
            return portModel?.GetDefaultCapacity() ?? Port.Capacity.Multi;
        }

        public string GetId()
        {
            return OriginalInstanceId.ToString();
        }

        public void Move(Vector2 position)
        {
            Position = position;
        }

        public void ChangeColor(Color color)
        {
            HasUserColor = true;
            m_Color = color;
        }

        PortModel MakePortForNode(Direction direction, string portName, PortType portType, TypeHandle dataType, string portId)
        {
            return new PortModel(portName ?? "", portId)
            {
                Direction = direction,
                PortType = portType,
                DataType = dataType,
                NodeModel = this
            };
        }

        protected PortModel AddDataInput<TDataType>(string portName, string portId = null)
        {
            return AddDataInput(portName, typeof(TDataType).GenerateTypeHandle(Stencil), portId);
        }

        protected PortModel AddDataInput(string portName, TypeHandle typeHandle, string portId = null)
        {
            return AddInputPort(portName, PortType.Data, typeHandle, portId);
        }

        protected PortModel AddDataOutputPort<TDataType>(string portName, string portId = null)
        {
            return AddDataOutputPort(portName, typeof(TDataType).GenerateTypeHandle(Stencil), portId);
        }

        protected PortModel AddDataOutputPort(string portName, TypeHandle typeHandle, string portId = null)
        {
            return AddOutputPort(portName, PortType.Data, typeHandle, portId);
        }

        protected PortModel AddInstanceInput<TDataType>(string portName = null, string portId = null)
        {
            return AddInstanceInput(typeof(TDataType).GenerateTypeHandle(Stencil), portName, portId);
        }

        protected PortModel AddInstanceInput(TypeHandle dataType, string portName = null, string portId = null)
        {
            return AddInputPort(portName, PortType.Instance, dataType, portId);
        }

        protected PortModel AddInputExecutionPort(string portName, string portId = null)
        {
            return AddInputPort(portName, PortType.Execution, TypeHandle.ExecutionFlow, portId);
        }

        protected PortModel AddExecutionOutputPort(string portName, string portId = null)
        {
            return AddOutputPort(portName, PortType.Execution, TypeHandle.ExecutionFlow, portId);
        }

        protected PortModel AddLoopOutputPort(string portName, string portId = null)
        {
            return AddOutputPort(portName, PortType.Loop, TypeHandle.ExecutionFlow, portId);
        }

        protected virtual PortModel AddInputPort(string portName, PortType portType, TypeHandle dataType, string portId = null)
        {
            var portModel = MakePortForNode(Direction.Input, portName, portType, dataType, portId);
            portModel = ReuseOrCreatePortModel(portModel, m_PreviousInputs, m_InputsById);
            UpdateConstantForInput(portModel);
            return portModel;
        }

        protected virtual PortModel AddOutputPort(string portName, PortType portType, TypeHandle dataType, string portId = null)
        {
            var portModel = MakePortForNode(Direction.Output, portName, portType, dataType, portId);
            return ReuseOrCreatePortModel(portModel, m_PreviousOutputs, m_OutputsById);
        }

        void UpdateConstantForInput(PortModel inputPort)
        {
            var id = inputPort.UniqueId;
            if (m_InputConstantsById.TryGetValue(id, out var constant))
            {
                // Destroy existing constant if not compatible
                Type type = inputPort.DataType.Resolve(Stencil);
                if (constant.m_AssetReference != null && constant.Type != type)
                {
                    Undo.DestroyObjectImmediate(constant.m_AssetReference);
                    constant.Destroy();
                    m_InputConstantsById.Remove(id);
                }
            }

            // Create new constant if needed
            if (!m_InputConstantsById.ContainsKey(id)
                && inputPort.PortType == PortType.Data
                && inputPort.DataType != TypeHandle.Unknown
                && Stencil.GetConstantNodeModelType(inputPort.DataType) != null
                && m_AssetReference != null)
            {
                var embeddedConstant = (ConstantNodeModel)((VSGraphModel)GraphModel).CreateConstantNode(
                    inputPort.Name,
                    inputPort.DataType,
                    Vector2.zero,
                    SpawnFlags.Default | SpawnFlags.Orphan);
                Utility.SaveAssetIntoObject(embeddedConstant.NodeAssetReference, (Object)AssetModel);
                m_InputConstantsById[id] = embeddedConstant;
            }
        }

        internal void ReinstantiateInputConstants()
        {
            foreach (var id in m_InputConstantsById.Keys.ToList())
            {
                var inputConstant = m_InputConstantsById[id];
                AbstractNodeAsset newConstant = (AbstractNodeAsset)Object.Instantiate((Object)inputConstant.NodeAssetReference);
                m_InputConstantsById[id] = (ConstantNodeModel)newConstant.Model;
                Utility.SaveAssetIntoObject(newConstant, (Object)AssetModel);
            }
        }

        protected void DeleteInputPort(PortModel portModel)
        {
            DeletePort(portModel);
        }

        protected void DeleteOutputPort(PortModel portModel)
        {
            DeletePort(portModel);
        }

        protected void DeletePort(IPortModel portModel)
        {
            var edgeModels = GraphModel.GetEdgesConnections(portModel);
            ((GraphModel)GraphModel).DeleteEdges(edgeModels);
        }

        public bool Destroyed { get; private set; }

        public void Destroy() => Destroyed = true;

        public virtual void OnBeforeSerialize()
        {
            m_InputConstantsById.SerializeDictionaryToLists(ref m_InputConstantKeys, ref m_InputConstantsValues);
            m_GuidAsString = m_Guid.ToString();
        }

        public virtual void OnAfterDeserialize()
        {
            m_InputConstantsById.DeserializeDictionaryFromLists(m_InputConstantKeys, m_InputConstantsValues);
            if (!GUID.TryParse(m_GuidAsString, out m_Guid))
                m_Guid = default;
        }

        public static implicit operator bool(NodeModel n)
        {
            return n != null && n.m_AssetReference != null;
        }
    }

    [Serializable]
    public abstract class HighLevelNodeModel : NodeModel {}
}
