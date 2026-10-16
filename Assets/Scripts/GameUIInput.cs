using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// НОВОЕ: Это "UI Input". Он отвечает ТОЛЬКО за чтение ввода игрока.
// Он ничего не знает про кубики, очки и физику. 
public class GameUIInput : MonoBehaviour
{
    [Header("Ссылки на поля ввода")]
    [SerializeField] private TMP_InputField diceCountInput;
    [SerializeField] private TMP_InputField drawScoreInput;
    [SerializeField] private TMP_InputField winScoreInput;
    [SerializeField] private TMP_InputField lossScoreInput;

    [Header("Ссылка на кнопку")]
    [SerializeField] private Button rollButton;

    [Header("Настройки ввода (Input System)")]
    [SerializeField] private InputActionReference throwActionRef;

    [Header("Значения по умолчанию")]
    [SerializeField] private int defaultDrawTarget = 10;
    [SerializeField] private int defaultWinTarget = 20;
    [SerializeField] private int defaultLossTarget = 5;

    // Событие "игрок запросил бросок". На него подписывается GameManager.
    public event Action OnRollRequested;

    private void OnEnable()
    {
        // Подписываемся на кнопку через код 
        if (rollButton != null) rollButton.onClick.AddListener(HandleRollButton);

        // Подписываемся на Space через Input System
        if (throwActionRef != null && throwActionRef.action != null)
        {
            throwActionRef.action.Enable();
            throwActionRef.action.performed += HandleSpacePressed;
        }
    }

    private void OnDisable()
    {
        if (rollButton != null) rollButton.onClick.RemoveListener(HandleRollButton);

        if (throwActionRef != null && throwActionRef.action != null)
        {
            throwActionRef.action.performed -= HandleSpacePressed;
            throwActionRef.action.Disable();
        }
    }

    public void HandleRollButton()
    {
        OnRollRequested?.Invoke();
    }

    private void HandleSpacePressed(InputAction.CallbackContext context)
    {
        OnRollRequested?.Invoke();
    }

    // МЕТОДЫ ЧТЕНИЯ ЗНАЧЕНИЙ

    public int GetDiceCount(int maxDices)
    {
        if (diceCountInput != null && int.TryParse(diceCountInput.text, out int count))
        {
            return Mathf.Clamp(count, 1, maxDices);
        }
        return maxDices;
    }

    // Считывает очки для ничьей из InputField. Если пусто, то вернёт defaultDrawTarget
    public int GetDrawTarget()
    {
        return ParseInput(drawScoreInput, defaultDrawTarget);
    }

    // Считывает очки для победы из InputField. Если пусто, то вернёт defaultWinTarget
    public int GetWinTarget()
    {
        return ParseInput(winScoreInput, defaultWinTarget);
    }

    // Считывает очки для поражения из InputField. Если пусто, то вернёт defaultLossTarget
    public int GetLossTarget()
    {
        return ParseInput(lossScoreInput, defaultLossTarget);
    }

    // Метод для записи целей в поля ввода (когда GameManager сгенерировал новые)
    public void SetTargetsInInputs(int draw, int win, int loss)
    {
        if (drawScoreInput != null) drawScoreInput.text = draw.ToString();
        if (winScoreInput != null) winScoreInput.text = win.ToString();
        if (lossScoreInput != null) lossScoreInput.text = loss.ToString();
    }

    private int ParseInput(TMP_InputField input, int defaultValue)
    {
        if (input != null && int.TryParse(input.text, out int value))
        {
            return value;
        }
        return defaultValue;
    }
}