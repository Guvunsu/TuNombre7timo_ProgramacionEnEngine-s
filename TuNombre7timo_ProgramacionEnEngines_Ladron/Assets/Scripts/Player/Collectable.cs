
using UnityEngine;

public class Collectable : MonoBehaviour
{
    [Header("Referencias")]
    public ScoreManager script_scoreManager;

    // Recoge el objeto.
    public void Collect()
    {
        script_scoreManager.AcomulativeScore();
        gameObject.SetActive(false);
    }
}
