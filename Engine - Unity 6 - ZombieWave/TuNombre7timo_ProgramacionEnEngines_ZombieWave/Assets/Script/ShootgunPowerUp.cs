using System.Collections;
using UnityEngine;

public class ShootgunPowerUp : MonoBehaviour
{
    [SerializeField] private float shotgunDuration = 8f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerShoot playerShoot = other.GetComponent<PlayerShoot>();

        if (playerShoot == null) return;

        // Si ya tiene la escopeta activa, no lo puede tomar y no se destruye
        if (playerShoot.IsShotgunActive())
            return;

        playerShoot.StartCoroutine(ActivateShotgunRoutine(playerShoot));
        Destroy(gameObject);
    }

    private IEnumerator ActivateShotgunRoutine(PlayerShoot playerShoot)
    {
        playerShoot.SetShotgunActive(true);

        yield return new WaitForSeconds(shotgunDuration);

        playerShoot.SetShotgunActive(false);
    }
}