using Mirror;
using UnityEngine;

public abstract class Pickupable : NetworkBehaviour
{
    public PrefabSpawner parent;

    public void Start()
    {
        if (!NetworkServer.active)
        {
            Destroy(GetComponent<Collider2D>());
            Destroy(this);
            return;
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Player p = collision.transform.parent.parent.gameObject.GetComponent<Player>();
        if (p != null)
            OnTrigger(p);
    }

    public virtual void OnTrigger(Player player)
    {
        Debug.Log($"{player.info.Nickname} just triggered with {name}!");
        parent?.Pickedup();
    }
}
