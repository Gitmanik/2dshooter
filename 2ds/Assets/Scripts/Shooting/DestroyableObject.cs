using UnityEngine;

public class DestroyableObject : MonoBehaviour, Target
{
    public float health;

    public void Shot(GameObject x, float damage)
    {
        health -= damage;

        if (health <= 0f)
        {
            Destroy(gameObject);
        }
    }
}
