using UnityEngine;
using UnityEngine.Events;

public class HealthTarget : MonoBehaviour, Target
{
    public UnityEvent onDamage;

    public float health;
    public void Shot(GameObject x, float damage)
    {
        Debug.Log($"{gameObject.name} shot by {x.name} for {damage}");
        health -= damage;
        onDamage.Invoke();
    }
}
