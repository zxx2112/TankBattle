using System;

namespace UnityEditor.VisualScripting.Model.NodeAssets
{
    public abstract class LoopStackAsset<T> : GenericFunctionAsset<T>, ILoopStackAsset where T : LoopStackModel, new() {}
    public interface ILoopStackAsset {}
}
