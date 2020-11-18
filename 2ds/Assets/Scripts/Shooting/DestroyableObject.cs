using Mirror;
using UnityEngine;

public class DestroyableObject : NetworkBehaviour, Target
{
    public float health;

    [Server]
    public void Damage(GameObject x, float damage)
    {
        health -= damage;

        if (health <= 0f)
        {
            Destroy(gameObject);
        }
    }
}
