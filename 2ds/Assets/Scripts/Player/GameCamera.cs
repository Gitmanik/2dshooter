using System.Collections;
using UnityEngine;

public class GameCamera : MonoBehaviour
{
    public static GameCamera instance;

    public Transform targetTransform;

    public bool smooth = true;

    [SerializeField] private Vector2 MaxOffset = new Vector2();
    [SerializeField] private Vector2 OffsetMult = new Vector2();
    [SerializeField] private float ZoomSpeed = 1f;

    public Vector3 Amount = new Vector3(1f, 1f, 0);
    public float Duration = 1;
    public float Speed = 10;
    public AnimationCurve Curve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    public bool DeltaMovement = true;

    protected float time = 0;
    protected Vector3 lastPos;
    protected Vector3 nextPos;
    protected float lastFoV;
    protected float nextFoV;

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

    public static void ShakeOnce(float duration = 1f, float speed = 10f, Vector3? amount = null)
    {
        instance.Duration = duration;
        instance.Speed = speed;
        if (amount != null)
            instance.Amount = (Vector3)amount;

        instance.ResetCam();
        instance.time = duration;
    }

    private void LateUpdate()
    {
        if (time > 0)
        {
            //do something
            time -= Time.deltaTime;
            if (time > 0)
            {
                //next position based on perlin noise
                nextPos = (Mathf.PerlinNoise(time * Speed, time * Speed * 2) - 0.5f) * Amount.x * transform.right * Curve.Evaluate(1f - time / Duration) +
                          (Mathf.PerlinNoise(time * Speed * 2, time * Speed) - 0.5f) * Amount.y * transform.up * Curve.Evaluate(1f - time / Duration);
                nextFoV = (Mathf.PerlinNoise(time * Speed * 2, time * Speed * 2) - 0.5f) * Amount.z * Curve.Evaluate(1f - time / Duration);

                targetCamera.fieldOfView += (nextFoV - lastFoV);
                transform.Translate(DeltaMovement ? (nextPos - lastPos) : nextPos);

                lastPos = nextPos;
                lastFoV = nextFoV;
            }
            else
            {
                //last frame
                ResetCam();
            }
        }
    }

    private void ResetCam()
    {
        //reset the last delta
        transform.Translate(DeltaMovement ? -lastPos : Vector3.zero);
        targetCamera.fieldOfView -= lastFoV;

        //clear values
        lastPos = nextPos = Vector3.zero;
        lastFoV = nextFoV = 0f;
    }
}
