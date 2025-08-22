using System;
using System.Collections.Generic;

namespace HelperClasses
{
    public static class ListExtensions
    {
        private static readonly Random random = new Random();

        /// <summary>
        /// Mischt eine Liste zufällig (Fisher-Yates-Shuffle) und gibt eine neue gemischte Liste zurück.
        /// </summary>
        /// <typeparam name="T">Typ der Listenelemente</typeparam>
        /// <param name="list">Die zu mischende Liste</param>
        /// <returns>Eine neue gemischte Kopie der Liste</returns>
        public static List<T> Shuffled<T>(this List<T> list)
        {
            if (list == null || list.Count <= 1)
                return new List<T>(list ?? new List<T>());

            // Erstelle eine Kopie der Liste, um das Original nicht zu modifizieren
            List<T> shuffledList = new List<T>(list);

            // Fisher-Yates-Shuffle
            for (int i = shuffledList.Count - 1; i > 0; i--)
            {
                int randomIndex = random.Next(i + 1); // Zufälliger Index zwischen 0 und i
                (shuffledList[i], shuffledList[randomIndex]) = (shuffledList[randomIndex], shuffledList[i]);
            }

            return shuffledList;
        }
    }
}