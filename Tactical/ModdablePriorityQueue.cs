using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public partial class ModdablePriorityQueue<T>{
    private List<(T element, int priority)> queue = new List<(T element, int priority)>();

    public int Count {
        get {return queue.Count();}
    }

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
        if (this.queue == null || this.queue.Count == 0) return (default(T), 0);
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

    /// <summary>
    /// Given a T element, modify *all* inclusions of that element by priority (higher value means it will happen earlier), then reorder the queue.
    /// </summary>
    public void ModifyItemPriority(T elementToModify, int modPriority){
        for (int i = 0; i < this.queue.Count; i++){
            (T element, int priority) = this.queue[i];
            if (element.Equals(elementToModify)){
                this.queue[i] = (element, priority + modPriority);
            }
        }
        this.queue = this.queue.OrderByDescending(x => x.priority).ToList();
    }

    /// <summary>Find and return all items in the priority queue in the tuple (T element, int priority).<br/>
    /// If that item does not have any remaining actions in the queue, returns an empty list.</summary>
    public List<(T element, int priority)> GetAllInstancesOfItem(T elementToFind){
        List<(T element, int priority)> instances = new();
        foreach((T element, int priority) pair in this.queue){
            if (pair.element.Equals(elementToFind)){
                instances.Add(pair);
            }
        }
        return instances;
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

    /// <summary>
    /// Removes or sets to default all of an element's appearances from the queue. Used for character turnlist (when a character is staggered, killed, stunned, etc.) and the event system.
    /// </summary>
    /// <param name="insteadMarkAsNull">
    /// If true, will instead mark set the element to its default (e.g. null). This is useful in cases where the moddable priority queue does NOT want the overall order
    /// to change, but does need to disable behavior of a set of elements. This is used for event handling; in a case where the Haste buff removes itself from RoundStart after use,
    /// we need to mark it null instead of removing it outright from the moddable priority queue, as removing it would cause the *last* subscriber in RoundStart (UI elements) from getting
    /// to trigger.
    /// </param>
    public void RemoveAllInstancesOfItem(T elementToRemove, bool insteadMarkAsNull = false){
        if (insteadMarkAsNull && elementToRemove != null){
            // Use for instead of LINQ's ForEach; https://stackoverflow.com/q/5034537
            for (int i = 0; i < this.queue.Count; i++){
                if (this.queue[i].element != null && this.queue[i].element.Equals(elementToRemove)){
                    this.queue[i] = (default, this.queue[i].priority);
                }
            }
        } else {
            // See https://stackoverflow.com/a/3459796.
            if (elementToRemove != null){
                this.queue.RemoveAll(item => item.element.Equals(elementToRemove));
            } else {
                this.queue.RemoveAll(item => item.element == null);
            }
        }
    }

    public List<(T element, int priority)> GetQueue(){
        return this.queue;
    }

    public (T element, int priority) this[int i]{
        get {return queue[i];}
    }

}