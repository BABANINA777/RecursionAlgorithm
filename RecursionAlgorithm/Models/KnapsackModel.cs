using System;
using System.Collections.Generic;

namespace RecursionAlgorithm.Models;

public class KnapsackModel
{
    // Логирование рекурсии
    private int _currentId = 1;
    public List<string> RecursionCalls { get; } = new();

    // Входные параметры
    private int _maxWeight;
    private int _nSize;

    // Сгенерированные массивы
    private double[] _weight = [];
    private double[] _cost = [];

    // Результаты работы алгоритма
    public double BestCost { get; private set; }
    public long RecursionSteps { get; private set; }

    public KnapsackModel(int max_weight, int nSize)
    {
        _maxWeight = max_weight;
        _nSize = nSize;

        GenerateArrays();
    }

    public void SetParameters(int maxWeight, int nSize)
    {
        _maxWeight = maxWeight;
        _nSize = nSize;

        GenerateArrays();
    }

    private void GenerateArrays()
    {
        _weight = new double[_nSize];
        _cost = new double[_nSize];

        double min = _maxWeight / 5.0;
        double max = _maxWeight * 4.0 / 5.0;

        Random random = new Random();
        for (int i = 0; i < _weight.Length; i++)
        {
            _weight[i] = min + random.NextDouble() * (max - min);
            _cost[i] = (min + 1000) + random.NextDouble() * (1000 + (max - min));
        }
    }

    public void RunAlgorithm()
    {
        // 1. Очищаем показатели и сбрасываем счетчик вызовов
        RecursionCalls.Clear();
        _currentId = 1;
        BestCost = 0;
        RecursionSteps = 0;

        // 2. Глубина корня равна общему числу шагов + 1
        int initialDepth = _nSize + 1;

        // 3. Запуск рекурсии с нулевого предмета
        SolveRecursive(
            index: 0,
            currentWeight: 0,
            currentCost: 0,
            parentId: 0,
            depth: initialDepth,
            branch: "Root"
        );
    }

    private void SolveRecursive(int index, double currentWeight, double currentCost, int parentId, int depth, string branch)
    {
        RecursionSteps++;
        int myId = _currentId++;

        // Формируем краткую запись: K(номер_предмета, текущий_вес)
        string customInfo = $"K(i={index}, W={currentWeight:F0})";
        RecursionCalls.Add($"{myId};{parentId};{depth};{branch};{customInfo}");

        // Базовый случай: перебрали все предметы
        if (index == _nSize)
        {
            if (currentCost > BestCost)
            {
                BestCost = currentCost;
            }
            return;
        }

        // Ветка 1 (Left): НЕ берем текущий предмет
        SolveRecursive(index + 1, currentWeight, currentCost, myId, depth - 1, "Left");

        // Ветка 2 (Right): БЕРЕМ текущий предмет (только если влезает по весу)
        if (currentWeight + _weight[index] <= _maxWeight)
        {
            SolveRecursive(index + 1, currentWeight + _weight[index], currentCost + _cost[index], myId, depth - 1, "Right");
        }
    }
}