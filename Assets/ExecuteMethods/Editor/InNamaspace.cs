using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace In
{
    public class InNamespaceNonstatic
    {
        public static void Execute()
        {
            Debug.Log(nameof(In) + nameof(InNamespaceNonstatic) + nameof(Execute));
        }
    }

    public static class InNamespaceStatic
    {
        public static void Execute()
        {
            Debug.Log(nameof(In) + nameof(InNamespaceStatic) + nameof(Execute));
        }
    }
}