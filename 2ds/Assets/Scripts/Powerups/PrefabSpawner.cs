using Photon.Pun;
using UnityEngine;

public class PrefabSpawner : MonoBehaviourPun
{
    [SerializeField] private GameObject[] prefabs;
    [SerializeField] private float spawnEvery;

    private bool containsPrefab;
    private float passed;

    private void Start()
    {
        passed = spawnEvery;
        if (!PhotonNetwork.IsMasterClient)
            Destroy(this);
    }

    void Update()
    {
        if (containsPrefab)
            return;

        passed += Time.deltaTime;

        if (passed >= spawnEvery)
        {
            containsPrefab = true;
            PhotonNetwork.Instantiate(prefabs[Random.Range(0, prefabs.Length)].name, transform.position, Quaternion.identity).GetComponent<Pickupable>().parent = this;
        }
    }

    public void Pickedup()
    {
        containsPrefab = false;
        passed = 0f;
    }
}