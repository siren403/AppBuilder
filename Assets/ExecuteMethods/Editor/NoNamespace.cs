using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoNamespaceNonstatic
{
    public static void Execute()
    {
        Debug.Log(nameof(NoNamespaceNonstatic) + nameof(Execute));
    }
}

public static class NoNamespaceStatic
{
    public static void Execute()
    {
        Debug.Log(nameof(NoNamespaceStatic) + nameof(Execute));
    }
}