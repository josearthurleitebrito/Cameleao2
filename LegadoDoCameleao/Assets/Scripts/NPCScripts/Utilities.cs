using System.Collections.Generic;
using UnityEngine;

// Esta classe DEVE ser estática para funcionar como extensão
public static class Utilities
{
    // Método para embaralhar listas (Fisher-Yates)
    public static void Shuffle<T>(this IList<T> list)
    {
        System.Random rng = new System.Random();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}