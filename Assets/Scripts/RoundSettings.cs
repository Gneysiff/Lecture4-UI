using System;

// Храним данные о текущем раунде.
[Serializable] // Чтобы Unity могла отображать этот класс в инспекторе (если понадобится)

// Перечесление результатов игры
public enum GameResult
{
    None,
    Win,
    Draw,
    Loss
}

public class RoundSettings
{
    public int DiceCount;       // Сколько кубиков кидаем
    public int DrawTarget;      // Очки для ничьей
    public int WinTarget;       // Очки для победы
    public int LossTarget;      // Очки для поражения (я... проиграл?!)

    // Результат последнего броска
    public int LastScore;       // Сумма очков кубиков
    public GameResult Result;   // Наша победа/ничья/поражение

    // Конструктор для инициализации
    public RoundSettings()
    {
        DiceCount = 3;
        DrawTarget = 10;
        WinTarget = 20;
        LossTarget = 5;
        LastScore = 0;
        Result = GameResult.None;
    }

    // Оценки результата
    public GameResult EvaluateResult()
    {
        // Защита от некорректного ввода. 
        var loss = LossTarget;
        var draw = Math.Max(DrawTarget, loss);      // Ничья не может быть меньше поражения
        var win = Math.Max(WinTarget, draw);        // Победа не может быть меньше ничьей

        if (LastScore >= win) return GameResult.Win;
        if (LastScore >= draw) return GameResult.Draw;
        return GameResult.Loss;
    }
}

