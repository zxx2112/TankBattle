using System;

namespace UnityEditor.VisualScripting.Model.NodeAssets
{
    public abstract class LoopNodeAsset<T> : NodeAsset<T>, ILoopNodeAsset where T : LoopNodeModel, new() {}
    public interface ILoopNodeAsset {}
}
