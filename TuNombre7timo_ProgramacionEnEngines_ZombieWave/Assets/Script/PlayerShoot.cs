using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShoot : MonoBehaviour
{
    #region Referencias
    public PlayerComprobation script_playerComprobation;
    [SerializeField] Transform firePoint;      
    [SerializeField] Transform firePointShotgunA;
    [SerializeField] Transform firePointShotgunB;
    [SerializeField] GameObject bulletPrefab;
    #endregion

    #region Variables
    [SerializeField] float fireRate = 0.2f;  
    [SerializeField] float bulletSpeed = 150f;
    private float nextFireTime = 0.33f;
    private bool isShooting = false;

    private bool isShotgunActive = false;
    #endregion

    #region Input System
    private PlayerInput playerInput;
    private InputAction shootAction;
    #endregion

    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        shootAction = playerInput.actions["Shoot"];
    }

    void OnEnable()
    {
        shootAction.performed += OnShootPerformed;
        shootAction.canceled += OnShootCanceled;
    }

    void OnDisable()
    {
        shootAction.performed -= OnShootPerformed;
        shootAction.canceled -= OnShootCanceled;
    }

    void Update()
    {
        if (!script_playerComprobation.isAlive) return;

        if (isShooting && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    #region Input Callbacks
    private void OnShootPerformed(InputAction.CallbackContext context)
    {
        isShooting = true;
    }

    private void OnShootCanceled(InputAction.CallbackContext context)
    {
        isShooting = false;
    }
    #endregion

    #region Disparo
    private void Shoot()
    {
        if (isShotgunActive)
        {
            ShootBulletFrom(firePointShotgunA);
            ShootBulletFrom(firePointShotgunB);
        } else
        {
            ShootBulletFrom(firePoint);
        }
    }

    private void ShootBulletFrom(Transform point)
    {
        if (bulletPrefab == null || point == null) return;

        GameObject bullet = Instantiate(bulletPrefab, point.position, point.rotation);
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = point.up * bulletSpeed;
        }
    }
    #endregion

    #region Power Up Escopeta
    public bool IsShotgunActive()
    {
        return isShotgunActive;
    }

    public void SetShotgunActive(bool active)
    {
        isShotgunActive = active;
    }
    #endregion
}