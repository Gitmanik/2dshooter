using System.Collections.Generic;
using UnityEngine;

public class FOV : MonoBehaviour
{
    public float viewRadius = 5;
    public float viewAngle = 135;
    //Collider2D[] playersInRadius;
    public List<Transform> visiblePlayer = new List<Transform>();
    public LayerMask obstacleMask;
    //public LayerMask playerMask;
    public Vector2 DirFromAngle(float angle, bool g)
    {
        if (!g)
        {
            angle += transform.eulerAngles.z;
        }
        return new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewRadius);
    }

    //private void FindVisiblePlayer()
    //{
    //    playersInRadius = Physics2D.OverlapCircleAll(transform.position, viewRadius, playerMask);
    //    visiblePlayer.Clear();
    //    for (int i = 0; i < playersInRadius.Length; i++)
    //    {
    //        Transform p = playersInRadius[i].transform;
    //        Vector2 dirTarget = new Vector2(p.position.x - transform.position.x, p.position.y - transform.position.y);
    //        if (Vector2.Angle(dirTarget, transform.right) < viewAngle / 2)
    //        {
    //            float dist = Vector2.Distance(transform.position, p.position);


    //            if (!Physics2D.Raycast(transform.position, dirTarget, dist, obstacleMask
    //                ))
    //            {
    //                visiblePlayer.Add(p);
    //            }
    //        }
    //    }
    //}
}
