using System;

namespace UnityEditor.VisualScripting.Model.NodeAssets
{
    public class ConstantNodeAsset<T> : NodeAsset<T>, IConstantNodeAsset where T : ConstantNodeModel, new() {}
    public interface IConstantNodeAsset {}
}
