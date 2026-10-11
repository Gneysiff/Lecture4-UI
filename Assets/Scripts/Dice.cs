using System.Collections.Generic;
using UnityEngine;

public class Dice : MonoBehaviour
{
    // Приватные поля для настройки силы и вращения нашего кубика через инспектор
    [Header("Настройки силы броска")]
    [SerializeField] private float minForce = 3f; // Нижняя граница силы подбрасывания
    [SerializeField] private float maxForce = 6f; // Верхняя граница силы подбрасывания

    [Header("Настройки вращения")]
    [SerializeField] private float minTorque = 10f; // мин крутящий момент
    [SerializeField] private float maxTorque = 25f; // макс крутящий момент

    [Header("Привязка граней кубика")]
    [SerializeField] private List<Transform> sideTriggers = new List<Transform>(); // Список 6 граней кубика (Наши те самые 6 Side_x)

    private Rigidbody rb; // Ссылка на компонент физики для кубика
    private bool isRolling; // Состояние кубика в данный момент времени

    // Публичные свойства с инкапсуляцией и проверкой значений, чтоб не полетело всё
    public bool IsRolling => isRolling;

    public float MinForce
    {
        get => minForce;
        set
        {
            if (value < 0)
            {
                Debug.LogWarning("Дофига физик что ли? Тут сила броска не может быть отрицательной! Устанавливаем её в нуль.");
                minForce = 0;
                return;
            }
            minForce = value;
        }
    }

    public float MaxForce
    {
        get => maxForce;
        set
        {
            if (value < minForce)
            {
                Debug.LogWarning("Ну если так, то будет дичь, ведт максимальная сила не может быть меньше минимальной! Уравниваем их.");
                maxForce = minForce;
                return;
            }
            maxForce = value;
        }
    }

    public float MinTorque
    {
        get => minTorque;
        set => minTorque = Mathf.Max(0f, value);
    }

    public float MaxTorque
    {
        get => maxTorque;
        set => maxTorque = Mathf.Max(minTorque, value);
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>(); // Кэшируем риджитбади при инициализации игры
    }

    // Метод рандомно подбрасывает кубик
    public void Roll()
    {
        isRolling = true; // Крутится
        rb.isKinematic = false; // Включаем физ расчёт

        var forceDirection = new Vector3(
            Random.Range(-0.2f, 0.2f),
            2f,
            Random.Range(-0.2f, 0.2f)
            ).normalized;

        // Применяем к кубику рандомный импульс
        var randomForce = Random.Range(MinForce, MaxForce);
        rb.AddForce(forceDirection * randomForce, ForceMode.Impulse);

        // А тут применяем рандомное вращение кубику
        var randomTorque = new Vector3(
            Random.Range(-maxTorque, maxTorque),
            Random.Range(-maxTorque, maxTorque),
            Random.Range(-maxTorque, maxTorque)
            );
        rb.AddTorque(randomTorque, ForceMode.Impulse);
    }

    public int GetUpwardValue()
    {
        if (sideTriggers == null || sideTriggers.Count == 0)
        {
            Debug.LogError("Список граней не заполнен в инспекторе!");
            return 0;
        }

        var highestSideIndex = 0;
        var maxPosY = float.MinValue;

        // Находим грань с наибольшей координатой y (самая верхняя грань сейчас)
        for (int i = 0; i < sideTriggers.Count; i++)
        {
            if (sideTriggers[i] != null)
            {
                var currentPosY = sideTriggers[i].position.y;
                if (currentPosY > maxPosY)
                {
                    maxPosY = currentPosY;
                    highestSideIndex = i + 1; // Индекс + 1 дает значение от 1 до 6
                }
            }
        }

        return highestSideIndex;
    }

    void Update()
    {
        // Проверка, что кубик остановился (практически не двигается)
        if (isRolling && rb.linearVelocity.sqrMagnitude < 0.001f && rb.angularVelocity.sqrMagnitude < 0.001f)
        { 
            isRolling = false;
        }
    }
}