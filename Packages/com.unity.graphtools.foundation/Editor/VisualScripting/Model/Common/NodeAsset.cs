using System;
using UnityEditor.VisualScripting.GraphViewModel;
using UnityEngine;

namespace UnityEditor.VisualScripting.Model
{
    public class NodeAsset<T> : AbstractNodeAsset where T : NodeModel, new()
    {
        [SerializeField]
        protected T m_NodeModel;

        public T Node => m_NodeModel ?? (m_NodeModel = new T(){NodeAssetReference = this});
        public override INodeModel Model => Node;

        public void OnEnable()
        {
            //When first created, the asset might not have the model ready
            m_NodeModel?.DefineNode();
        }
    }
}
