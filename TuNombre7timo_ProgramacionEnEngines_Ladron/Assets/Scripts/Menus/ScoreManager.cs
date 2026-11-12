using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI scoreTextNumber;
    int scoreNumber;
    void Start()
    {
        scoreTextNumber.text = "Collected: " + scoreNumber;
    }

    public void AcomulativeScore()
    {
        scoreNumber++;
        scoreTextNumber.text = "Collected: " + scoreNumber;
    }
}
