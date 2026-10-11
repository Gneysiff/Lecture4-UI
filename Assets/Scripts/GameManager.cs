using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    [Header("Ссылки на объекты")]
    [SerializeField] private List<Dice> dices = new List<Dice>(); // Список всех кубиков НААА СТОЛЕЕЕ
    [SerializeField] private Dice dicePrefab; // Ссылка на Prefabы кубика для создания копий

    [Header("UI и Настройки")]
    [SerializeField] private GameUIView uiView; // Ссылка на наш View

    [Header("Настройки ввода (Input System)")]
    [SerializeField] private InputActionReference throwActionRef; // Ссылка на бросок 

    private RoundSettings currentRound; // Данные о раунде

    private bool isCalculatingScore; // Флаг, идёт ли сейчас процесс подсчёта очков

    // Публичные свойства
    public List<Dice> Dices => dices;
    public bool IsCalculatingScore => isCalculatingScore;

    private void Awake()
    {
        // Инициализация UI и Модели в Awake 
        currentRound = new RoundSettings();

        if (uiView != null)
        {
            uiView.ShowScore(0);
            uiView.ShowStatus("Тыкай SPACE для броска");
            uiView.ShowResult(GameResult.None);

            // Генерируем случайные цели на первый раунд
            GenerateRandomTargets();
        }
    }

    private void OnEnable()
    {
        // Отслеживаем нажатие кнопки 
        if (throwActionRef != null && throwActionRef.action != null)
        {
            throwActionRef.action.Enable();
            throwActionRef.action.performed += OnThrowPressed;
        }

        // Если хотим привязать не через инспектор, а кодом
        //if (uiView != null && uiView.RollButton != null)
        //{
        //    uiView.RollButton.onClick.AddListener(OnThrowButtonClicked);
        //}
    }

    private void OnDisable()
    {
        if (throwActionRef != null && throwActionRef.action != null)
        {
            throwActionRef.action.performed -= OnThrowPressed;
            throwActionRef.action.Disable();
        }

        // Если хотим привязать не через инспектор, а кодом
        // Отписываемся от кнопки
        //if (uiView != null && uiView.RollButton != null)
        //{
        //    uiView.RollButton.onClick.RemoveListener(OnThrowButtonClicked);
        //}
    }

    // Метод для кнопки (Unity Event)
    public void OnThrowButtonClicked()
    {
        StartRollingProcess();
    }

    private void OnThrowPressed(InputAction.CallbackContext context)
    {
        Debug.Log("Клавиша Space нажата! Начинаем вращать!"); // для проверки
        StartRollingProcess();
    }

    // Логика запуска броска в один метод (вызывается и от Space, и от кнопки)
    private void StartRollingProcess()
    {
        if (isCalculatingScore || uiView == null) return;

        // Считываем данные из UI 
        var requestedDiceCount = uiView.GetDiceCount(100); // условный максимум на всякий пожарный
        Debug.Log($"[INPUT] Игрок запросил {requestedDiceCount} кубиков"); // ВРЕМЕННО
        currentRound.DiceCount = requestedDiceCount;
        currentRound.DrawTarget = uiView.GetDrawTarget();
        currentRound.WinTarget = uiView.GetWinTarget();
        currentRound.LossTarget = uiView.GetLossTarget();

        // Обновляем UI
        uiView.ShowTargets(currentRound.DrawTarget, currentRound.WinTarget, currentRound.LossTarget);
        uiView.ShowStatus("Кубики вращаются...");
        uiView.ClearScore();
        uiView.ShowResult(GameResult.None);

        // Очищаем старые кубики и создаем новые
        ClearOldDices();
        SpawnDices(currentRound.DiceCount);

        // Запускаем бросок для нужного количества кубиков
        foreach (var dice in dices)
        {
            if (dice != null)
            {
                dice.Roll();
            }
        }

        // Запускаем корутину ожидания остановки кубиков
        StartCoroutine(WaitAndCalculateScoreRoutine());
    }

    // Удаляет все кубики, оставшиеся от предыдущего броска
    private void ClearOldDices()
    {
        foreach (var dice in dices)
        {
            if (dice != null)
            {
                Destroy(dice.gameObject);
            }
        }
        dices.Clear();
    }

    // Создает нужное число кубиков из Prefabа
    private void SpawnDices(int count)
    {
        Debug.Log($"[SPAWN] Пытаюсь создать {count} кубиков"); // ВРЕМЕННО
        if (dicePrefab == null)
        {
            Debug.LogError("Dice Prefab не назначен в инспекторе! Как мы тебе кубики наделаем?!");
            return;
        }

        for (int i = 0; i < count; i++)
        {
            // Создаем копию префаба 
            var newDice = Instantiate(dicePrefab, new Vector3(0, 3f, 0), Quaternion.identity);
            dices.Add(newDice);
        }
        Debug.Log($"[SPAWN] Реально создано: {dices.Count} кубиков"); // ВРЕМЕННО
    }

    // Генерация случайных целей
    private void GenerateRandomTargets()
    {
        // Генерируем случайные числа. Логика: Поражение < Ничья < Победа
        currentRound.LossTarget = Random.Range(1, 10);
        currentRound.DrawTarget = Random.Range(10, 20);
        currentRound.WinTarget = Random.Range(20, 35);

        // Обновляем UI
        uiView.SetTargetsInInputs(currentRound.DrawTarget, currentRound.WinTarget, currentRound.LossTarget);
        uiView.ShowTargets(currentRound.DrawTarget, currentRound.WinTarget, currentRound.LossTarget);
    }

    private IEnumerator WaitAndCalculateScoreRoutine()
    {
        isCalculatingScore = true;

        // Небольшая задержка, чтобы физика успела придать кубикам скорость
        yield return new WaitForSeconds(0.2f);

        var waitInterval = new WaitForSeconds(0.1f); // исправил от прошлой вариации из лекции 3, как будет лучше для оптимизации

        // Ждём в цикле, пока ВСЕ кубики перестанут флексить
        var allStopped = false;
        while (!allStopped)
        {
            allStopped = true;
            for (int i = 0; i < currentRound.DiceCount; i++) // Проверяем только активные кубики
            {
                if (dices[i] != null && dices[i].IsRolling)
                {
                    allStopped = false; // Хотя бы один кубик ещё движется
                    break;
                }
            }

            // ОПТИМИЗАЦИЯ: Если кубики остановились, не ждем следующий интервал, а сразу выходим
            if (!allStopped)
            {
                yield return waitInterval; // Ждём следующий интервал, а не кадр
            }
        }

        // Сумма выпавших очков
        var totalScore = 0;
        for (int i = 0; i < currentRound.DiceCount; i++) // Считаем только активные кубики
        {
            if (dices[i] != null)
            {
                totalScore += dices[i].GetUpwardValue();
            }
        }

        // Обновляем Модель
        currentRound.LastScore = totalScore;
        currentRound.Result = currentRound.EvaluateResult();

        // Отображаем данные
        uiView.ShowScore(totalScore);
        uiView.ShowResult(currentRound.Result);
        uiView.ShowStatus("Бросок завершён! Можешь бросить снова.");

        Debug.Log($"Выпало очков: {totalScore}, Результат: {currentRound.Result}");
        isCalculatingScore = false;
    }
}