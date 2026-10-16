using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Ссылки на объекты")]
    [SerializeField] private Dice dicePrefab; // Префаб кубика

    [Header("UI Input (чтение ввода)")]
    [SerializeField] private GameUIInput uiInput; // Ссылка на Input. НЕ НА View!

    // Список кубиков теперь внутренний, снаружи его никто не читает.
    private readonly List<Dice> dices = new List<Dice>();
    private RoundSettings currentRound;
    private bool isCalculatingScore;

    // ПОДПИСКИ НА СОБЫТИЯ (GameUIView)
    public event Action<int> OnScoreChanged;                       // Изменилась сумма очков
    public event Action<GameResult> OnResultChanged;               // Изменился результат (Win/Draw/Loss)
    public event Action<int, int, int> OnTargetsChanged;           // Изменились цели (draw, win, loss)
    public event Action<string> OnStatusChanged;                   // Изменился текст статуса
    public event Action OnRollStarted;                             // Начался бросок (для того чтобы View очистил счёт)

    private void Awake()
    {
        currentRound = new RoundSettings();
        GenerateRandomTargets();
    }

    private void OnEnable()
    {
        // Подписываемся на событие "игрок запросил бросок" от UI Input
        if (uiInput != null)
        {
            uiInput.OnRollRequested += HandleRollRequested;
        }
    }

    private void OnDisable()
    {
        // Отписываемся от этого события
        if (uiInput != null)
        {
            uiInput.OnRollRequested -= HandleRollRequested;
        }
    }

    private void Start()
    {
        // Отправляем начальное состояние
        // Start гарантированно идёт после всех OnEnable
        BroadcastInitialState();
    }

    private void BroadcastInitialState()
    {
        OnScoreChanged?.Invoke(0);
        OnResultChanged?.Invoke(GameResult.None);
        OnTargetsChanged?.Invoke(currentRound.DrawTarget, currentRound.WinTarget, currentRound.LossTarget);
        OnStatusChanged?.Invoke("ТЫКАЙ Space или кнопку, чтобы бросить кубики");
    }

    // Обработчик события "игрок запросил бросок"
    private void HandleRollRequested()
    {
        StartRollingProcess();
    }

    private void StartRollingProcess()
    {
        if (isCalculatingScore || uiInput == null) return;

        // 1. Читаем ввод из UI Input
        currentRound.DiceCount = uiInput.GetDiceCount(100);
        currentRound.DrawTarget = uiInput.GetDrawTarget();
        currentRound.WinTarget = uiInput.GetWinTarget();
        currentRound.LossTarget = uiInput.GetLossTarget();

        // 2. Сообщаем всем, что раунд начался (View очистит счёт и результат)
        OnRollStarted?.Invoke();
        OnTargetsChanged?.Invoke(currentRound.DrawTarget, currentRound.WinTarget, currentRound.LossTarget);
        OnStatusChanged?.Invoke("Кубики вращаются...");

        // 3. Создаём новые кубики и бросаем
        ClearOldDices();
        SpawnDices(currentRound.DiceCount);
        foreach (var dice in dices)
        {
            if (dice != null) dice.Roll();
        }

        StartCoroutine(WaitAndCalculateScoreRoutine());
    }

    private void ClearOldDices()
    {
        foreach (var dice in dices)
        {
            if (dice != null) Destroy(dice.gameObject);
        }
        dices.Clear();
    }

    private void SpawnDices(int count)
    {
        if (dicePrefab == null)
        {
            Debug.LogError("Prefab куубика не назначен в инспекторе!");
            return;
        }

        for (int i = 0; i < count; i++)
        {
            // Смещаем кубики в линию, чтобы не слипались
            var offsetX = (i - (count - 1) / 2f) * 0.6f;
            var offsetZ = UnityEngine.Random.Range(-0.3f, 0.3f);
            var heightY = 5f + UnityEngine.Random.Range(0f, 0.5f);

            var spawnPosition = new Vector3(offsetX, heightY, offsetZ);
            var newDice = Instantiate(dicePrefab, spawnPosition, Quaternion.identity);
            dices.Add(newDice);
        }
    }

    private void GenerateRandomTargets()
    {
        currentRound.LossTarget = UnityEngine.Random.Range(1, 10);
        currentRound.DrawTarget = UnityEngine.Random.Range(10, 20);
        currentRound.WinTarget = UnityEngine.Random.Range(20, 35);

        // Пишем сгенерированные значения в поля ввода, чтобы игрок их видел
        if (uiInput != null)
        {
            uiInput.SetTargetsInInputs(currentRound.DrawTarget, currentRound.WinTarget, currentRound.LossTarget);
        }
    }

    private IEnumerator WaitAndCalculateScoreRoutine()
    {
        isCalculatingScore = true;
        yield return new WaitForSeconds(0.2f);

        // ОПТИМИЗАЦИЯ: Кэшируем WaitForSeconds один раз перед циклом.
        var waitInterval = new WaitForSeconds(0.1f);

        var allStopped = false;
        while (!allStopped)
        {
            allStopped = true;
            for (int i = 0; i < dices.Count; i++)
            {
                if (dices[i] != null && dices[i].IsRolling)
                {
                    allStopped = false;
                    break;
                }
            }

            if (!allStopped) yield return waitInterval;
        }

        // Считаем очки
        var totalScore = 0;
        foreach (var dice in dices)
        {
            if (dice != null) totalScore += dice.GetUpwardValue();
        }

        currentRound.LastScore = totalScore;
        currentRound.Result = currentRound.EvaluateResult();

        OnScoreChanged?.Invoke(totalScore);
        OnResultChanged?.Invoke(currentRound.Result);
        OnStatusChanged?.Invoke("Бросок завершён! Можешь кидать кубика снова.");

        GenerateRandomTargets();
        OnTargetsChanged?.Invoke(currentRound.DrawTarget, currentRound.WinTarget, currentRound.LossTarget);

        Debug.Log($"Выпало очков: {totalScore}, Результат: {currentRound.Result}");
        isCalculatingScore = false;
    }
}