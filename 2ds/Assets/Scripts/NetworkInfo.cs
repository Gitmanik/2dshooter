using Mirror;
using TMPro;
using UnityEngine;

public class NetworkInfo : MonoBehaviour
{
    public TMP_Text t;

    // Update is called once per frame
    void Update()
    {
        t.text = string.Format("ping: {0}ms\nvar: {0}ms", (int)(NetworkTime.rtt * 1000), NetworkTime.rttVar * 1000d);
    }
}
