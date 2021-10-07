using System;
using System.Collections;
using System.Collections.Generic;

public static class ExtensionMethods  {

    // https://en.wikipedia.org/wiki/Fisher%E2%80%93Yates_shuffle
    public static void Shuffle<T>(this IList<T> list, int? seed = null) {
        var rng = seed.HasValue ? new Random(seed.Value) : new Random();

        for(int i = list.Count - 1; i > 0; --i) {
            int j = rng.Next(0, i + 1);
            list.Swap(i, j);
        }
    }

    public static void Swap<T>(this IList<T> list, int a, int b) {
        T tmp = list[a];
        list[a] = list[b];
        list[b] = tmp;
    }

}
