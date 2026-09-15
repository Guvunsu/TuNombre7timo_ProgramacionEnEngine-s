using UnityEngine;

public class HeartPowerUp : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerComprobation player = other.GetComponent<PlayerComprobation>();

        if (player == null) return;

        // Si ya tiene la vida máxima, no hace nada y no se destruye
        if (player.GetCurrentLives() >= player.GetMaxLives())
            return;

        player.AddLife();
        Destroy(gameObject);
    }
}