using System.Collections.Generic;
using UnityEngine;

namespace Gitmanik.FOV2D
{
    public class FOV : MonoBehaviour
    {
        [Header("FOV Configuration")]
        public float viewRadius = 5;
        public float viewAngle = 135;

        [Header("Layer Configuration")]
        public LayerMask obstacleMask;
        public LayerMask playerMask;

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

        public List<Transform> FindVisiblePlayer()
        {
            List<Transform> visiblePlayers = new List<Transform>();
            Collider2D[] playersInRadius = Physics2D.OverlapCircleAll(transform.position, viewRadius, playerMask);
            visiblePlayers.Clear();
            for (int i = 0; i < playersInRadius.Length; i++)
            {
                Transform p = playersInRadius[i].transform;
                Vector2 dirTarget = new Vector2(p.position.x - transform.position.x, p.position.y - transform.position.y);
                if (Vector2.Angle(dirTarget, transform.right) < viewAngle / 2)
                {
                    float dist = Vector2.Distance(transform.position, p.position);


                    if (!Physics2D.Raycast(transform.position, dirTarget, dist, obstacleMask
                        ))
                    {
                        visiblePlayers.Add(p);
                    }
                }
            }
            return visiblePlayers;
        }
    }
}