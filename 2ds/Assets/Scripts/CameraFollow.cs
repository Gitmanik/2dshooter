using UnityEngine;

public class CameraFollow : MonoBehaviour
{

    private void Update()
    {
        Vector3 vec = Game.Instance.player.transform.position;
        vec.z = transform.position.z;
        transform.position = vec;
    }
}
