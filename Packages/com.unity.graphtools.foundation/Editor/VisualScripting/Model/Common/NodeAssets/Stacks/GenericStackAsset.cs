using System;
using UnityEditor.VisualScripting.GraphViewModel;

namespace UnityEditor.VisualScripting.Model.NodeAssets
{
    public abstract class GenericStackAsset<T> : NodeAsset<T>, IStackAsset where T : StackBaseModel, new()
    {
        public StackBaseModel StackModel => Node;
    }

    public interface IStackAsset
    {
        StackBaseModel StackModel { get; }
    }
}
