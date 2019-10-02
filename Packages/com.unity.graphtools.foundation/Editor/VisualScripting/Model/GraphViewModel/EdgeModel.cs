using System;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor.VisualScripting.Model;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

namespace UnityEditor.VisualScripting.GraphViewModel
{
    [Serializable]
    public class EdgeModel : IEdgeModel, IUndoRedoAware
    {
        [Serializable]
        internal struct PortReference
        {
            [SerializeField]
            AbstractNodeAsset m_NodeModel;

            INodeModel NodeModel
            {
                get => m_NodeModel?.Model;
                set => m_NodeModel = value?.NodeAssetReference;
            }

            // for backward compatibility, rely on UniqueId rather than index
            [SerializeField, FormerlySerializedAs("Index")]
            int m_ObsoleteIndex;

            [Obsolete("only here for backward compatibility with old edges")]
            public int Index => - 1;

            [SerializeField]
            public string UniqueId;

            public void Assign(IPortModel portModel)
            {
                Assert.IsNotNull(portModel);
                NodeModel = portModel.NodeModel;
                UniqueId = portModel.UniqueId;
            }

            public IPortModel GetPortModel(Direction direction, ref IPortModel previousValue)
            {
                if (NodeModel == null)
                    return previousValue = null;

                // when removing a set property member, we patch the edges portIndex
                // the cached value needs to be invalidated
                if (previousValue != null && (previousValue.NodeModel != NodeModel || previousValue.Direction != direction))
                    previousValue = null;

                if (previousValue != null)
                    return previousValue;

                previousValue = null;
                // backward compatibility with older graphs
                if (String.IsNullOrEmpty(UniqueId) && m_ObsoleteIndex != -1)
                {
                    NodeModel actualModel = (NodeModel)NodeModel;
                    var portModelsByIndex = direction == Direction.Input ? actualModel.InputsByDisplayOrder : actualModel.OutputsByDisplayOrder;
                    previousValue = portModelsByIndex.ElementAt(m_ObsoleteIndex);
                    UniqueId = previousValue.UniqueId;
                    m_ObsoleteIndex = -1;
                }
                else
                {
                    var portModelsByGuid = direction == Direction.Input ? NodeModel.InputsById : NodeModel.OutputsById;
                    if (UniqueId != null)
                        portModelsByGuid.TryGetValue(UniqueId, out previousValue);
                }
                return previousValue;
            }

            public override string ToString()
            {
                var ownerStr = NodeModel?.ToString() ?? "<null>";
                return $"{ownerStr}@{UniqueId}";
            }
        }

        [SerializeField]
        GraphModel m_GraphModel;
        [SerializeField]
        PortReference m_InputPortReference;
        [SerializeField]
        PortReference m_OutputPortReference;

        IPortModel m_InputPortModel;
        IPortModel m_OutputPortModel;

        public EdgeModel(IGraphModel graphModel, IPortModel inputPort, IPortModel outputPort)
        {
            GraphModel = graphModel;
            SetFromPortModels(inputPort, outputPort);
        }

        public ScriptableObject SerializableAsset => m_GraphModel;
        public IGraphAssetModel AssetModel => GraphModel?.AssetModel;

        public IGraphModel GraphModel
        {
            get => m_GraphModel;
            set => m_GraphModel = (GraphModel)value;
        }

        // Capabilities
        public CapabilityFlags Capabilities => CapabilityFlags.Selectable | CapabilityFlags.Deletable;

        public void SetFromPortModels(IPortModel newInputPortModel, IPortModel newOutputPortModel)
        {
            m_InputPortReference.Assign(newInputPortModel);
            m_InputPortModel = newInputPortModel;

            m_OutputPortReference.Assign(newOutputPortModel);
            m_OutputPortModel = newOutputPortModel;
        }

        public IPortModel InputPortModel => m_InputPortReference.GetPortModel(Direction.Input, ref m_InputPortModel);
        public IPortModel OutputPortModel => m_OutputPortReference.GetPortModel(Direction.Output, ref m_OutputPortModel);

        public string GetId()
        {
            return String.Empty;
        }

        public string OutputId => m_OutputPortReference.UniqueId;

        public string InputId => m_InputPortReference.UniqueId;

        public override string ToString()
        {
            return $"{m_InputPortReference} -> {m_OutputPortReference}";
        }

        public void UndoRedoPerformed()
        {
            m_InputPortModel = default;
            m_OutputPortModel = default;
        }
    }
}
