using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;

public class HealthTarget : MonoBehaviour, Target
{
    public UnityEvent onDamage;

    public float health;
    public void Damage(int viewID, float damage)
    {
        Debug.Log($"{gameObject.name} shot by {PhotonView.Find(viewID).name} for {damage}");
        health -= damage;
        onDamage.Invoke();
    }
}
