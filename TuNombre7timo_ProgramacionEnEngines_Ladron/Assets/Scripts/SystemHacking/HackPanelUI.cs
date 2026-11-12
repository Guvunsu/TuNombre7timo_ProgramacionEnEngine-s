
using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class HackPanelUI : MonoBehaviour
{
    // Elementos de la interfaz.
    [SerializeField] private TMP_Text questionText;
    [SerializeField] private TMP_InputField answerInput;
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private Button submitButton;
    [SerializeField] private Button closeButton;

    // Mensajes.
    [SerializeField] private string successMessage = "ACCESO CONCEDIDO";
    [SerializeField] private string failMessage = "ACCESO DENEGADO";

    // Avisar cuando se responde o se cierra.
    public event Action Hacked;
    public event Action Closed;

    public bool IsOpen => gameObject.activeSelf;

    private string correctAnswer;
    private bool hacked;

    private void Start()
    {
        if (submitButton != null)
            submitButton.onClick.AddListener(Submit);

        if (closeButton != null)
            closeButton.onClick.AddListener(Close);
    }

    private void Update()
    {
        // Cerrar con Escape o B.
        if (IsOpen &&
            ((Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) ||
             (Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame)))
        {
            Close();
        }
    }

    // Abrir el panel y mostrar la pregunta.
    public void Open(string question, string answer, bool alreadyHacked)
    {
        correctAnswer = answer;
        hacked = alreadyHacked;

        if (questionText != null)
            questionText.text = question;

        if (answerInput != null)
        {
            answerInput.text = "";
            answerInput.interactable = !hacked;
        }

        if (submitButton != null)
            submitButton.interactable = !hacked;

        if (feedbackText != null)
            feedbackText.text = "";

        gameObject.SetActive(true);
    }

    // Comprobar la respuesta.
    public void Submit()
    {
        if (hacked || answerInput == null)
            return;

        if (string.Equals(answerInput.text.Trim(), correctAnswer.Trim(),
            StringComparison.OrdinalIgnoreCase))
        {
            hacked = true;

            if (feedbackText != null)
            {
                feedbackText.text = successMessage;
                feedbackText.color = Color.green;
            }

            answerInput.interactable = false;

            if (submitButton != null)
                submitButton.interactable = false;

            Hacked?.Invoke();
        } else
        {
            if (feedbackText != null)
            {
                feedbackText.text = failMessage;
                feedbackText.color = Color.red;
            }

            answerInput.text = "";
        }
    }

    // Cerrar el panel.
    public void Close()
    {
        if (!IsOpen)
            return;

        gameObject.SetActive(false);
        Closed?.Invoke();
    }
}
