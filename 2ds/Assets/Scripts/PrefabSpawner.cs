using Mirror;
using UnityEngine;

public class PrefabSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject[] prefabs;
    [SerializeField] private float spawnEvery;

    private bool containsPrefab;
    private float passed;

    private void Start()
    {
        passed = spawnEvery;
        if (!NetworkServer.active)
            Destroy(this);
    }

    void Update()
    {
        if (containsPrefab)
            return;

        passed += Time.deltaTime;

        if (passed >= spawnEvery)
        {
            passed = 0f;
            containsPrefab = true;

            GameObject x = Instantiate(prefabs[Random.Range(0, prefabs.Length)], transform.position, Quaternion.identity);
            x.GetComponent<Pickupable>().parent = this;
            NetworkServer.Spawn(x);
        }
    }

    public void Pickedup()
    {
        containsPrefab = false;
    }
}