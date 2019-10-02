using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor.VisualScripting.Model.Stencils
{
    [Serializable]
    public class CoroutineStackModel : LoopStackModel
    {
        public override Type MatchingStackedNodeType => typeof(CoroutineNodeModel);
        public override bool AllowChangesToModel => false;

        public override List<TitleComponent> BuildTitle()
        {
            return new List<TitleComponent>
            {
                new TitleComponent
                {
                    titleComponentType = TitleComponentType.String,
                    titleObject = "Coroutine"
                }
            };
        }
    }
}
