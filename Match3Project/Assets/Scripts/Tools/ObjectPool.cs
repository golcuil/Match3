using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* 
 * This class will allow us to instantiate as many objects as we will need
 * while the scene is being set up so that we don't slow down the game by
 * calling Instantiate during gameplay.
 * 
 * It's preferred, but optional, to call PoolObjects first with a specified amount
 */

public abstract class ObjectPool<T> : Singleton<ObjectPool<T>> where T : MonoBehaviour
{
    [SerializeField] protected T prefab;

    private List<T> pooledObjects;
    private int amount;
    private bool isReady;


    // Create the Pool, with a specified amount of objects
    public void PoolObjects(int amount = 0)
    {
        if (amount < 0)
            throw new ArgumentOutOfRangeException("Amount to pool must be non-negative");

        this.amount = amount;

        // Initialize the list
        pooledObjects = new List<T>(amount);

        //Instantiate a bunch of T's
        GameObject newObject;

        for (int i = 0; i < amount; i++)
        {
            newObject = Instantiate(prefab.gameObject, transform);
            newObject.SetActive(false);

            // Add each object to the list
            pooledObjects.Add(newObject.GetComponent<T>());

        }

        // flag the pool as ready
        isReady = true;
    }

    // Get an object from the pool
    public T GetPooledObject()
    {
        // Check if pool is ready, if not make it ready
        if (!isReady)
            PoolObjects(1);

        // Search through the list for something not in use and return it
        for(int i = 0; i < amount; i++)
        {
            if (!pooledObjects[i].isActiveAndEnabled)
                return pooledObjects[i];
        }
        // If we didn't find anything, make a new one
        GameObject newObject = Instantiate(prefab.gameObject, transform);
        newObject.SetActive(false);
        pooledObjects.Add(newObject.GetComponent<T>());
        amount++;

        return newObject.GetComponent<T>();
    }

    // Return an object back to the pool
    public void ReturnObjectToPool(T toBeReturned)
    {
        //verify the argument
        if (toBeReturned == null)
            return;
        // make sure that the pool is ready, if not make it ready
        if(!isReady)
        {
            PoolObjects();
            pooledObjects.Add(toBeReturned);
        }
        //deactivate the game object
        toBeReturned.gameObject.SetActive(false);
    }
}
