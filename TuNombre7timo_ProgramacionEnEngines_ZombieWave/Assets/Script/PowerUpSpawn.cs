using System.Collections;
using UnityEngine;

public class PowerUpSpawn : MonoBehaviour
{
    #region Variables
    [SerializeField] private Transform[] powerUpsSpawners; 
    [SerializeField] private GameObject powerUpShotgun; 
    [SerializeField] private GameObject powerUpLife;  

    [SerializeField] private float spawnInterval = 13.13f;  
    #endregion

    void Start()
    {
        StartCoroutine(SpawnPowerUpRoutine());
    }

    IEnumerator SpawnPowerUpRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            SpawnPowerUp();
        }
    }

    private void SpawnPowerUp()
    {
        if (powerUpsSpawners.Length == 0) return;
        // aparicion random
        Transform spawnPoint = powerUpsSpawners[Random.Range(0, powerUpsSpawners.Length)];
        // Elegir entre escopeta o vida (50/50)
        GameObject powerUpToSpawn = Random.Range(0, 2) == 0 ? powerUpShotgun : powerUpLife;
        Instantiate(powerUpToSpawn, spawnPoint.position, spawnPoint.rotation);
    }
}