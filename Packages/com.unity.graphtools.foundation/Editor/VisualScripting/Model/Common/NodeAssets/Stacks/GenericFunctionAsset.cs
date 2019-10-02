using System;

namespace UnityEditor.VisualScripting.Model.NodeAssets
{
    public abstract class GenericFunctionAsset<T> : GenericStackAsset<T>, IFunctionAsset where T : FunctionModel, new() {}
    public interface IFunctionAsset {}
}
