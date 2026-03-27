// PROTOTYPE - NOT FOR PRODUCTION
// Question: Is the core loop fun?
// Date: 2026-03-27

using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public static ObjectPool Instance { get; private set; }

    [SerializeField] private GameObject _enemyPrefab;
    [SerializeField] private GameObject _projectilePrefab;
    [SerializeField] private GameObject _xpGemPrefab;

    private Queue<GameObject> _enemyPool = new();
    private Queue<GameObject> _projectilePool = new();
    private Queue<GameObject> _xpGemPool = new();

    private void Awake()
    {
        Instance = this;
        PreWarm(_enemyPrefab, _enemyPool, 50);
        PreWarm(_projectilePrefab, _projectilePool, 100);
        PreWarm(_xpGemPrefab, _xpGemPool, 100);
    }

    private void PreWarm(GameObject prefab, Queue<GameObject> pool, int count)
    {
        for (int i = 0; i < count; i++)
        {
            var obj = Instantiate(prefab, transform);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    public GameObject GetEnemy(Vector2 position)
    {
        return Get(_enemyPrefab, _enemyPool, position);
    }

    public GameObject GetProjectile(Vector2 position)
    {
        return Get(_projectilePrefab, _projectilePool, position);
    }

    public GameObject GetXPGem(Vector2 position)
    {
        return Get(_xpGemPrefab, _xpGemPool, position);
    }

    public void Return(GameObject obj)
    {
        obj.SetActive(false);
        if (obj.CompareTag("Enemy")) _enemyPool.Enqueue(obj);
        else if (obj.CompareTag("XPGem")) _xpGemPool.Enqueue(obj);
        else _projectilePool.Enqueue(obj);
    }

    private GameObject Get(GameObject prefab, Queue<GameObject> pool, Vector2 pos)
    {
        GameObject obj;
        if (pool.Count > 0)
        {
            obj = pool.Dequeue();
        }
        else
        {
            obj = Instantiate(prefab, transform);
        }
        obj.transform.position = pos;
        obj.SetActive(true);
        return obj;
    }
}
