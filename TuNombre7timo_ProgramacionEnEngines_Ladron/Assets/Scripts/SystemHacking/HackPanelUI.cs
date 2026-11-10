using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
public class HackPanelUI : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private TMP_Text questionText;
    [SerializeField] private TMP_InputField answerInput;
    [SerializeField] private Button submitButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private TMP_Text feedbackText;

    [Header("Mensajes")]
    [SerializeField]
    private string successMessage = "ACCESO CONCEDIDO - Sistema hackeado";

    [SerializeField]
    private string failMessage = "ACCESO DENEGADO - Respuesta incorrecta";

    public event Action Hacked;
    public event Action Closed;

    public bool IsOpen => gameObject.activeSelf;

    private string correctAnswer;
    private bool hacked;

    private void OnEnable()
    {
        if (submitButton != null)
            submitButton.onClick.AddListener(Submit);

        if (closeButton != null)
            closeButton.onClick.AddListener(Close);

        if (answerInput != null)
            answerInput.onSubmit.AddListener(OnInputSubmit);
    }

    private void OnDisable()
    {
        if (submitButton != null)
            submitButton.onClick.RemoveListener(Submit);

        if (closeButton != null)
            closeButton.onClick.RemoveListener(Close);

        if (answerInput != null)
            answerInput.onSubmit.RemoveListener(OnInputSubmit);
    }

    private void Update()
    {
        bool cancel = (Keyboard.current != null &&  Keyboard.current.escapeKey.wasPressedThisFrame) || (Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame);
        if (cancel && IsOpen)
            Close();
    }

    public void Open(string question, string answer, bool alreadyHacked)
    {
        correctAnswer = answer;
        hacked = alreadyHacked;

        if (questionText != null)
            questionText.text = question;

        if (answerInput != null)
            answerInput.text = "";

        if (feedbackText != null)
        {
            feedbackText.text = hacked ? successMessage : "";

            if (hacked)
                feedbackText.color = Color.green;
        }

        SetInputEnabled(!hacked);

        gameObject.SetActive(true);

        if (!hacked)
            StartCoroutine(FocusInput());
    }

    public void Close()
    {
        if (!IsOpen)
            return;

        gameObject.SetActive(false);
        Closed?.Invoke();
    }

    private void OnInputSubmit(string text)
    {
        Submit();
    }

    public void Submit()
    {
        if (hacked || answerInput == null) return;

        string typed = answerInput.text.Trim();

        if (string.Equals( typed, correctAnswer?.Trim(),StringComparison.OrdinalIgnoreCase))
        {
            hacked = true;

            if (feedbackText != null)
            {
                feedbackText.text = successMessage;
                feedbackText.color = Color.green;
            }

            SetInputEnabled(false);
            Hacked?.Invoke();
        } else
        {
            if (feedbackText != null)
            {
                feedbackText.text = failMessage;
                feedbackText.color = Color.red;
            }

            answerInput.text = "";
            answerInput.ActivateInputField();
        }
    }

    private void SetInputEnabled(bool enabled)
    {
        if (answerInput != null)
            answerInput.interactable = enabled;

        if (submitButton != null)
            submitButton.interactable = enabled;
    }

    private IEnumerator FocusInput()
    {
        yield return null;

        if (answerInput != null && IsOpen && answerInput.interactable)
        {
            answerInput.Select();
            answerInput.ActivateInputField();
        }
    }
}