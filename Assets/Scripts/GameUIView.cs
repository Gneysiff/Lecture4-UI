using TMPro;             
using UnityEngine;       
using UnityEngine.UI;     

public class GameUIView : MonoBehaviour
{
    [Header("Ссылки на тексты")] 
    [SerializeField] private TextMeshProUGUI scoreText; // Ссылка на текст, куда выводится сумма выпавших очков 
    [SerializeField] private TextMeshProUGUI resultText; // Ссылка на текст, куда выводится результат раунда 
    [SerializeField] private TextMeshProUGUI targetsText; // Ссылка на текст, куда выводятся цели раунда 
    [SerializeField] private TextMeshProUGUI statusText; // Ссылка на текст, куда выводятся подсказки

    [Header("Ссылки на поля ввода")]
    [SerializeField] private TMP_InputField diceCountInput; // Сколько кубиков бросить
    [SerializeField] private TMP_InputField drawScoreInput; // Скока очков надо для ничьей
    [SerializeField] private TMP_InputField winScoreInput; // Скока очков надо для победы
    [SerializeField] private TMP_InputField lossScoreInput; // Скока очков надо для поражения

    [Header("Ссылка на кнопку")] 
    [SerializeField] private Button rollButton; // Ссылка на кнопку, по нажатию на которую бросаются кубики (через Unity Event)

    [Header("Значения по умолчанию (если поле ввода пустое)")]
    [SerializeField] private int defaultDrawTarget = 10;
    [SerializeField] private int defaultWinTarget = 20;
    [SerializeField] private int defaultLossTarget = 5;

    // Публичное свойство, чтобы GameManager мог подписаться на кнопку через код.
    public Button RollButton => rollButton;

    // Показывает сумму выпавших очков в тексте ScoreText
    public void ShowScore(int score)
    {
        // Проверка на null: если ссылка не привязана в инспекторе, не будет ошибки
        if (scoreText != null)
        {
            scoreText.text = $"Сумма очков: {score}";
        }
    }

    // Показывает результат раунда в тексте ResultText и красит его в нужный цвет
    public void ShowResult(GameResult result)
    {
        // Если текст не привязан — выходим, чтобы не было ошибки
        if (resultText == null) return;

        switch (result)
        {
            case GameResult.Win:                          // Если результат — Победа
                resultText.text = "ПОБЕДА!";              
                resultText.color = Color.green;           
                break;                                    
            case GameResult.Draw:                         // Если результат — Ничья
                resultText.text = "НИЧЬЯ!";               
                resultText.color = Color.yellow;          
                break;                                     
            case GameResult.Loss:                         // Если результат — Поражение
                resultText.text = "ПОРАЖЕНИЕ!";           
                resultText.color = Color.red;           
                break;                                     
            default:                                      // Случай None — раунд не завершён
                resultText.text = "";                     // Очищаем текст
                break;                                    
        }
    }

    // Показывает цели текущего раунда в тексте TargetsText
    public void ShowTargets(int draw, int win, int loss)
    {
        // Проверка на null
        if (targetsText != null)
        {
            targetsText.text = $"Цели раунда:\nНичья: {draw}\nПобеда: {win}\nПоражение: {loss}";
        }
    }

    // Показывает статусную строку в тексте StatusText (подсказки)
    public void ShowStatus(string status)
    {
        // Проверка на null
        if (statusText != null)
        {
            // Записываем переданный текст в поле статуса
            statusText.text = status;
        }
    }

    // Очищает поле суммы очков перед новым броском
    public void ClearScore()
    {
        // Проверка на null
        if (scoreText != null)
        {
            scoreText.text = "Сумма очков: ...";
        }
    }

    // Считывает количество кубиков из InputField.
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

    // Парсинга чисел из InputField
    private int ParseInput(TMP_InputField input, int defaultValue)
    {
        // Проверяем: поле существует и в нём введено корректное число?
        if (input != null && int.TryParse(input.text, out int value))
        {
            return value;
        }

        return defaultValue;
    }

    // Запись целей в поля ввода
    public void SetTargetsInInputs(int draw, int win, int loss)
    {
        if (drawScoreInput != null) drawScoreInput.text = draw.ToString();

        if (winScoreInput != null) winScoreInput.text = win.ToString();

        if (lossScoreInput != null) lossScoreInput.text = loss.ToString();
    }
}