using System;

namespace RecursionAlgorithm.Models;

public class KnapsackModel
{
    // Входные параметры
    private int _maxWeight;
    private int _nSize;

    // Сгенерированные массивы
    private double[] _weight = [];
    private double[] _cost = [];

    // Результаты работы алгоритма (доступны для чтения извне)
    public double BestCost { get; private set; }
    public long RecursionSteps { get; private set; } // long, так как шагов может быть очень много

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
            _cost[i] = (min * 1000) + random.NextDouble() * (1000 * (max - min));
        }
    }

    // Публичный метод запуска алгоритма
    public void RunAlgorithm()
    {
        // Сбрасываем показатели перед новым запуском
        BestCost = 0;
        RecursionSteps = 0;

        // Запускаем рекурсию с 0-го предмета, с нулевым текущим весом и стоимостью
        SolveRecursive(0, 0, 0);
    }

    // Приватный рекурсивный метод перебора подмножеств
    private void SolveRecursive(int index, double currentWeight, double currentCost)
    {
        // Фиксируем каждый заход в рекурсивный метод
        RecursionSteps++;

        // Базовый случай: дошли до конца массива предметов
        if (index == _nSize)
        {
            if (currentCost > BestCost)
            {
                BestCost = currentCost;
            }
            return;
        }

        // Ветка 1: НЕ берем текущий предмет и идем дальше
        SolveRecursive(index + 1, currentWeight, currentCost);

        // Ветка 2: БЕРЕМ текущий предмет (только если он влезает в рюкзак по весу)
        if (currentWeight + _weight[index] <= _maxWeight)
        {
            SolveRecursive(index + 1, currentWeight + _weight[index], currentCost + _cost[index]);
        }
    }
}
