using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assem
{
    public static partial class AssemPartial
    {
        public static void ExecuteB()
        {
            Debug.Log(nameof(Assem) + nameof(AssemPartial) + nameof(ExecuteB));
        }
    }

    public static partial class AssemBPartial
    {
        public static void Execute()
        {
            Debug.Log(nameof(Assem) + nameof(AssemBPartial) + nameof(Execute));
        }
    }
}