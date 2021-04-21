using Photon.Pun;

public class DestroyableObject : MonoBehaviourPun, Target
{
    public float health;

    public void Damage(int viewID, float damage)
    {
        health -= damage;

        if (health <= 0f)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }
}
