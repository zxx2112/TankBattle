using System;

namespace UnityEditor.VisualScripting.Model.NodeAssets
{
    public abstract class GenericEventFunctionAsset<T> : GenericFunctionAsset<T>, IEventFunctionAsset where T : EventFunctionModel, new() {}
    public interface IEventFunctionAsset {}
}
