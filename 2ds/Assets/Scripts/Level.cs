using UnityEngine;

public class Level : MonoBehaviour
{
    public Transform black;
    void Start()
    {
        black.gameObject.SetActive(true);
    }
}
