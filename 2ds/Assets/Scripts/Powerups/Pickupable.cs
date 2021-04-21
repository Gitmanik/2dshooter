using Photon.Pun;
using UnityEngine;

public abstract class Pickupable : MonoBehaviourPun
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
        sprite.SetActive(false);
        audioSource.Play();
        pickedup = true;
        photonView.RPC("RpcPickedUp", RpcTarget.MasterClient);
    }

    [PunRPC]
    public void RpcPickedUp()
    {
        parent?.Pickedup();
        LeanTween.delayedCall(2.5f, () => PhotonNetwork.Destroy(gameObject));
    }
}
