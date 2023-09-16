using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public partial class ModdablePriorityQueue<T>{
    private List<(T element, int priority)> queue = new List<(T element, int priority)>();

    public void Clear(){
        this.queue.Clear();
    }

    /// <summary>
    /// Add an element to the queue. A higher priority value goes before a lower priority value.
    /// </summary>
    public void AddToQueue(T element, int priority){
        int i = 0;
        while (i < this.queue.Count){
            if (priority <= this.queue[i].priority){
                i++;
            } else {
                break;
            }
        }
        this.queue.Insert(i, (element, priority));
    }

    /// <summary>Returns the next item in the priority queue in the tuple (T element, int priority).<br/>
    /// If no one remains in the queue, return (null, 0) instead.</summary>
    public (T element, int priority) GetNextItem(){
        if (this.queue == null || this.queue.Count <= 0) return (default(T), 0);
        (T element, int priority) nextItem = this.queue[0];
        return nextItem;
    }

    /// <summary>Returns the next item in the priority queue in the tuple (T element, int priority).<br/>
    /// Remove the item from the front of the queue afterwards.<br/>
    /// If no one remains in the queue, return (null, 0) instead.</summary>
    public (T element, int priority) PopNextItem(){
        if (this.queue == null || this.queue.Count <= 0) return (default(T), 0);
        (T element, int priority) nextItem = GetNextItem();
        this.queue.RemoveAt(0);
        return nextItem;
    }

    /// <summary>Find and return the next item in the priority queue in the tuple (T element, int priority).<br/>
    /// If that item does not have any remaining actions in the queue, return (null, 0) instead.</summary>
    public (T element, int priority) GetNextInstanceOfItem(T elementToFind){
        foreach((T element, int priority) pair in this.queue){
            if (pair.element.Equals(elementToFind)){
                return pair;
            }
        }
        return (default(T), 0);
    }

    /// <summary>Return true if the item is in the priority queue, false otherwise.</summary>
    public bool ContainsItem(T elementToFind){
        foreach((T element, int priority) pair in this.queue){
            if (pair.element.Equals(elementToFind)){
                return true;
            }
        }
        return false;
    }

    /// <summary>Removes the next instance of a specified item from the queue (e.g. use when a character clashes).</summary>
    public void RemoveNextInstanceOfItem(T elementToRemove){
        int i = 0;
        while (i < this.queue.Count) {
            if (this.queue[i].element.Equals(elementToRemove)){
                this.queue.RemoveAt(i);
                return;
            }
            i++;
        }
        return;
    }

    /// <summary>Removes all of a character's actions from the queue (use when a character is staggered, killed, stunned, etc.).</summary>
    public void RemoveAllInstancesOfItem(T elementToRemove){
        this.queue.RemoveAll(item => item.Equals(elementToRemove));
    }

    public List<(T element, int priority)> GetQueue(){
        return this.queue;
    }
}