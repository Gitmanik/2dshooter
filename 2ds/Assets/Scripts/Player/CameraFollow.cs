using System.Collections;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public static CameraFollow instance;

    public Transform targetTransform;

    public bool smooth = true;

    [SerializeField] private Vector2 MaxOffset = new Vector2();
    [SerializeField] private Vector2 OffsetMult = new Vector2();
    [SerializeField] private float ZoomSpeed = 1f;

    private Camera targetCamera;
    private Coroutine routine;

    private void Awake()
    {
        targetCamera = GetComponent<Camera>();
        instance = this;
    }

    private void Update()
    {
        if (targetTransform != null && (Player.Local != null && !Player.Local.LockMovement))
        {
            Vector3 vec = targetTransform.position;
            vec.z = transform.position.z;

            if (smooth)
            {
                Vector3 relativePos = targetCamera.ScreenToWorldPoint(Input.mousePosition) - vec;
                vec.x += Mathf.Clamp(relativePos.x * OffsetMult.x, -MaxOffset.x, MaxOffset.x);
                vec.y += Mathf.Clamp(relativePos.y * OffsetMult.y, -MaxOffset.y, MaxOffset.y);
            }

            transform.position = vec;
        }
    }

    public void Zoom(float target)
    {
        if (routine != null)
        {
            StopCoroutine(routine);
        }
        routine = StartCoroutine(InternalZoom(target));
    }

    private IEnumerator InternalZoom(float target)
    {
        float timeElapsed = 0f;
        float start = targetCamera.orthographicSize;

        while (target != targetCamera.orthographicSize)
        {
            targetCamera.orthographicSize = Mathf.Lerp(start, target, timeElapsed / ZoomSpeed);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
    }
}
