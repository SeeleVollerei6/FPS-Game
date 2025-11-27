using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolManager : MonoBehaviour
{
    public static ObjectPoolManager Instance { get; private set; }

    [System.Serializable]
    public struct Pool
    {
        public string tag;
        public GameObject prefab;
        public int size;
    }

    public List<Pool> pools;
    public Dictionary<string, Queue<GameObject>> poolDictionary;
    private Dictionary<int, Coroutine> activeReturnCoroutines = new Dictionary<int, Coroutine>();

    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializePools();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializePools()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach (Pool pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab);
                obj.SetActive(false);
                objectPool.Enqueue(obj);

                obj.transform.SetParent(transform); 
            }

            poolDictionary.Add(pool.tag, objectPool);
        }
    }

    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        GameObject objectToSpawn;

        if (poolDictionary[tag].Count > 0)
        {
            objectToSpawn = poolDictionary[tag].Dequeue();
        }
        else
        {
            Pool pool = pools.Find(p => p.tag == tag);
            if (pool.prefab != null)
            {
                objectToSpawn = Instantiate(pool.prefab);
                objectToSpawn.transform.SetParent(transform);
            }
            else
            {
                return null;
            }
        }

        objectToSpawn.SetActive(true);

        var agent = objectToSpawn.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null)
        {
            agent.Warp(position);
        }
        else
        {
            objectToSpawn.transform.position = position;
            objectToSpawn.transform.rotation = rotation;
        }

        return objectToSpawn;
    }

    public void ReturnToPool(string tag, GameObject obj, float time)
    {
        CancelReturn(obj);

        Coroutine coroutine = StartCoroutine(ReturnToPoolCoroutine(tag, obj, time));
        activeReturnCoroutines.Add(obj.GetInstanceID(), coroutine);
    }

    private IEnumerator<WaitForSeconds> ReturnToPoolCoroutine(string tag, GameObject obj, float time)
    {
        if (obj.GetComponent<Enemy>())
        {
            Debug.Log("enemy recycle");
            obj.GetComponent<Enemy>().HP = 100;
            obj.GetComponent<Enemy>().isDead = false;
        }
        yield return new WaitForSeconds(time);
        int id = obj.GetInstanceID();
        if (activeReturnCoroutines.ContainsKey(id))
        {
            activeReturnCoroutines.Remove(id);
        }
        if (obj.transform.parent == transform || obj.transform.parent == null)
        {
            obj.SetActive(false);
            poolDictionary[tag].Enqueue(obj);
        }
    }

    public void CancelReturn(GameObject obj)
    {
        int id = obj.GetInstanceID();
        if (activeReturnCoroutines.ContainsKey(id))
        {
            if (activeReturnCoroutines[id] != null)
            {
                StopCoroutine(activeReturnCoroutines[id]);
            }
            activeReturnCoroutines.Remove(id);
        }
    }
}
