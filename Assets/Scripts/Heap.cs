using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Heap<T> where T : IHeapItem<T> {
    //array of items
    T[] items;
    //current number of items
    int currentItemCount;

    //load new heap
    public Heap(int maxHeapSize) {
        items = new T[maxHeapSize];
    }

    //add new item
    public void Add(T item) {
        //set index to the current number of items
        item.HeapIndex = currentItemCount;
        //add new item to array
        items[currentItemCount] = item;

        //since we have placed the item at the end of the array, we want to now sort it up accordingly
        SortUp(item);

        //increment number of items
        currentItemCount++;
    }

    public T RemoveFirst() {
        //get the first item
        T firstItem = items[0];
        //increment number of items
        currentItemCount--;

        //set last item as the new first item
        items[0] = items[currentItemCount];
        items[0].HeapIndex = 0;

        //since we have placed the item at the top of the array, we want to sort it down accordingly
        SortDown(items[0]);

        return firstItem;
    }

    void SortUp(T item) {
        while (true) {
            //get the index of the parent
            int parentIndex = ((item.HeapIndex - 1) / 2);

            //get the parent item
            T parentItem = items[parentIndex];

            //if the parent has a lower priority than the item, swap them
            if (item.CompareTo(parentItem) > 0) {
                Swap(item, parentItem);
            } else {
                break;
            }
        }
    }

    void SortDown(T item) {
        while (true) {
            //get children indexes
            int childIndexLeft = ((item.HeapIndex * 2) + 1);
            int childIndexRight = ((item.HeapIndex * 2) + 2);

            //the index of the item we will be swapping
            int swapIndex = 0;

            //if we have a left child (aka first child)
            //if this returns false, that means that this item is the lowest item
            if (childIndexLeft < currentItemCount) {
                //set the index to the left child
                swapIndex = childIndexLeft;

                //if we have a right child
                if (childIndexRight < currentItemCount) {
                    //if the right child has a lower priority than the left child
                    if (items[childIndexLeft].CompareTo(items[childIndexRight]) < 0) {
                        //set the index to the right child
                        swapIndex = childIndexRight;
                    }
                }

                //if the item has a highest priority than the child with the lowest priority, swap
                if (item.CompareTo(items[swapIndex]) < 0) {
                    Swap(item, items[swapIndex]);
                } else {
                    return;
                }
            } else {
                return;
            }
        }
    }

    //swap items
    void Swap(T itemA, T itemB) {
        //swap items in the array
        items[itemA.HeapIndex] = itemB;
        items[itemB.HeapIndex] = itemA;

        //create a temporary reference to the first item's index
        int itemAIndex = itemA.HeapIndex;
        //swap indexes
        itemA.HeapIndex = itemB.HeapIndex;
        itemB.HeapIndex = itemAIndex;
    }

    public void UpdateItem(T item) {
        SortUp(item);
    }

    public bool Contains(T item) {
        return Equals(items[item.HeapIndex], item);
    }

    public int Count {
        get { return currentItemCount; }
    }
}

//so that items in the heap are aware of their index
public interface IHeapItem<T> : IComparable<T> {
    int HeapIndex {
        get;
        set;
    }
}