
using UnityEngine;

public class CheckStatusGameLife : MonoBehaviour
{
    [Header("Objetos a recoger")]
    [SerializeField] GameObject[] collectedObjects;
    [SerializeField] int collected = 0;

    [Header("References")]
    public SceneController script_sceneController;

    private bool gameOver;

    // Cuenta los objetos que ya están desactivados.
    private int CountCollected()
    {
        collected = 0;

        for (int i = 0; i < collectedObjects.Length; i++)
        {
            if (collectedObjects[i] == null || !collectedObjects[i].activeSelf)
                collected++;
        }

        return collected;
    }

    public void YouWin()
    {
        if (gameOver) return;

        collected = CountCollected();

        // Si faltan objetos, no gana.
        if (collected < collectedObjects.Length) return;

        gameOver = true;
        script_sceneController.LoadWin();
    }

    public void YouLoose()
    {
        if (gameOver) return;

        gameOver = true;
        script_sceneController.LoadLoose();
    }

    // Comprueba si el jugador llegó al punto final.
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("VictoryPoint"))
        {
            YouWin();
        }
    }
}