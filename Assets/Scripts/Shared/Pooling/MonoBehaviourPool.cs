using UnityEngine;
using UnityEngine.Pool;

public abstract class MonoBehaviourPool<T> : MonoBehaviour, IPool<T> where T : MonoBehaviour
{
    [SerializeField] private T prefab;
    [SerializeField] private int defaultCapacity = 10;
    [SerializeField] private int maxSize = 100;
    [SerializeField] private Transform poolParent;

    private ObjectPool<T> pool;

    public int CountActive => pool?.CountActive ?? 0;
    public int CountInactive => pool?.CountInactive ?? 0;

    protected virtual void Awake()
    {
        if (poolParent == null)
        {
            poolParent = transform;
        }

        if (prefab == null)
        {
            Debug.LogError($"{GetType().Name} prefab is not assigned.", this);
            return;
        }

        pool = new ObjectPool<T>(
            CreateItem,
            OnGetFromPool,
            OnReleaseToPool,
            OnDestroyPoolItem,
            false,
            defaultCapacity,
            maxSize);
    }

    public T Get()
    {
        if (pool == null)
        {
            Debug.LogError($"{GetType().Name} is not initialized.", this);
            return null;
        }

        return pool.Get();
    }

    public void Release(T item)
    {
        if (item == null)
        {
            return;
        }

        if (pool == null)
        {
            Destroy(item.gameObject);
            return;
        }

        pool.Release(item);
    }

    protected virtual T CreateItem()
    {
        T item = Instantiate(prefab, poolParent);
        item.gameObject.SetActive(false);
        return item;
    }

    protected virtual void OnGetFromPool(T item)
    {
        if (item == null)
        {
            return;
        }

        item.gameObject.SetActive(true);
    }

    protected virtual void OnReleaseToPool(T item)
    {
        if (item == null)
        {
            return;
        }

        item.transform.SetParent(poolParent, false);
        item.gameObject.SetActive(false);
    }

    protected virtual void OnDestroyPoolItem(T item)
    {
        if (item != null)
        {
            Destroy(item.gameObject);
        }
    }
}
