using System.Collections.Generic;
using UnityEngine;

namespace OmnicatLabs.Utils
{
    [System.Serializable]
    public class RandomItem<T>
    {
        public int frequency;
        [HideInInspector]
        public int defaultCount;
        public T item;
    }

    public class PercentRandom<T>
    {
        public List<RandomItem<T>> items = new List<RandomItem<T>>();
        private int numOfValidPoints;

        /// <summary>
        /// Initialiazes the randomizer with a given list of generic objects
        /// </summary>
        public void Init(List<RandomItem<T>> _items)
        {
            items.AddRange(_items);
        }

        public T PickObject()
        {
            if (items.Count == 0)
                Debug.LogError("No objects in the list");

            CollectPoints();

            Reset();

            //random
            int randomIndex = Random.Range(0, numOfValidPoints);
            int validPoints = 0;

            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].frequency > 0)
                {
                    validPoints += items[i].frequency;
                }

                if (validPoints > randomIndex)
                {
                    items[i].frequency--;
                    return items[i].item;
                }
            }

            return default(T);
        }

        private void CollectPoints()
        {
            //get number of valid points
            numOfValidPoints = 0;

            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].frequency > 0)
                {
                    numOfValidPoints += items[i].frequency;
                }
            }
        }

        private void Reset()
        {
            //check for reset
            if (numOfValidPoints < 1)
            {
                for (int i = 0; i < items.Count; i++)
                {
                    items[i].frequency = items[i].defaultCount;
                    numOfValidPoints += items[i].frequency;
                }
            }
        }
    }
}