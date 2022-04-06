using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Assem
{
    public class AssemInNamespaceNonstatic
    {
        public static void Execute()
        {
            Debug.Log(nameof(Assem) + nameof(AssemInNamespaceNonstatic) + nameof(Execute));
        }
    }

    public static class AssemInNamespaceStatic
    {
        public static void Execute()
        {
            Debug.Log(nameof(Assem) + nameof(AssemInNamespaceStatic) + nameof(Execute));
        }
    }
}