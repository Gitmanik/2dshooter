using UnityEngine;

public class Player : MonoBehaviour, Target
{
    public float speed;
    private Rigidbody2D rb;

    public GameObject muzzleSmoke;
    public Transform gunStart;
    private Gun currentgun;
    private float shootDelay;
    [SerializeField] private IngameHUDManager hudman;

    private FOV fov;

    private Vector3 change = Vector3.zero;
    public float health;

    public void SetGun(Gun newgun)
    {
        currentgun = newgun;
        shootDelay = 0f;
        fov.viewAngle = newgun.viewAngle;
        fov.viewRadius = newgun.viewRadius;
        hudman.UpdateAmmo(newgun);
    }

    private void Start()
    {
        Game.Instance.player = this;
        fov = GetComponent<FOV>();
        SetGun(Game.Instance.Guns[0]);
        hudman.UpdateAmmo(currentgun);
        hudman.UpdateHealth(health);
    }

    private void OnValidate()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        shootDelay -= Time.deltaTime;
        if (Game.Instance.LockInput)
            return;


        if (shootDelay <= 0f && Input.GetKey(KeyCode.Mouse0) && currentgun.currentAmmoInMagazine > 0)
            Shoot();

        if (Input.GetKeyDown(KeyCode.R) && currentgun.magazineCount > 0)
            Reload();

        if (Input.GetKeyDown(KeyCode.T))
            Game.Instance.GunSelector.Toggle();

        change.x = Input.GetAxisRaw("Horizontal");
        change.y = Input.GetAxisRaw("Vertical");

        Vector3 dir = Input.mousePosition - Camera.main.WorldToScreenPoint(transform.position);
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    private void Reload()
    {
        SoundManager.Play(Game.Instance.reloadSound);
        currentgun.magazineCount--;
        currentgun.currentAmmoInMagazine = currentgun.ammoInMagazine;
        hudman.UpdateAmmo(currentgun);
    }

    private void Shoot()
    {
        Instantiate(muzzleSmoke, transform);
        shootDelay = 1f / currentgun.firerate;
        SoundManager.Play(Game.Instance.gunshot);
        currentgun.currentAmmoInMagazine--;
        hudman.UpdateAmmo(currentgun);

        RaycastHit2D hit = Physics2D.Raycast(gunStart.position, Input.mousePosition - Camera.main.WorldToScreenPoint(transform.position), 99f);
        Target t;
        if (hit.collider != null)
        {
            t = hit.transform.GetComponent<Target>();
            if (t != null)
            {
                Debug.Log($"Distance: {hit.distance}");
                t.Shot(gameObject, currentgun.damageCurve.Evaluate(hit.distance) * currentgun.damage);
            }
        }
    }

    private void FixedUpdate()
    {
        if (Game.Instance.LockInput)
            return;
        rb.MovePosition(transform.position + change.normalized * speed * Time.fixedDeltaTime);
    }

    public void Shot(GameObject x, float damage)
    {
        health -= damage;
        hudman.UpdateHealth(health);
    }
}
