using Mirror;
using UnityEngine;

public abstract class Pickupable : NetworkBehaviour
{
    public PrefabSpawner parent;
    [SerializeField] private AudioClip playOnPickup;
    [SerializeField] private GameObject sprite;
    private AudioSource audioSource;

    bool pickedup = false;

    public void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = playOnPickup;
        if (!NetworkServer.active)
        {
            Destroy(GetComponent<Collider2D>());
            return;
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (pickedup)
            return;

        Player p = collision.transform.parent.parent.gameObject.GetComponent<Player>();
        if (p != null)
            OnTrigger(p);
    }

    public virtual void OnTrigger(Player player)
    {
        Debug.Log($"{player.Nickname} just triggered with {name}!");
        RpcPickedUp();
        parent?.Pickedup();
        Destroy(GetComponent<Collider2D>());
        pickedup = true;
        LeanTween.delayedCall(2.5f, () => NetworkServer.Destroy(gameObject));
    }

    [ClientRpc]
    private void RpcPickedUp()
    {
        sprite.SetActive(false);
        audioSource.Play();
    }
}
