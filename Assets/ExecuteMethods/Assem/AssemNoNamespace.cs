using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssemNoNamespaceNonstatic
{
    public static void Execute()
    {
        Debug.Log(nameof(AssemNoNamespaceNonstatic) + nameof(Execute));
    }
}

public class AssemNoNamespaceStatic
{
    public static void Execute()
    {
        Debug.Log(nameof(AssemNoNamespaceStatic) + nameof(Execute));
    }
}