using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assem
{
    public static partial class AssemPartial
    {
        public static void Execute()
        {
            Debug.Log(nameof(Assem) + nameof(AssemPartial) + nameof(Execute));
        }
    }
    
    public static partial class AssemPartial
    {
        public static void Execute2()
        {
            Debug.Log(nameof(Assem) + nameof(AssemPartial) + nameof(Execute2));
        }
    }
}