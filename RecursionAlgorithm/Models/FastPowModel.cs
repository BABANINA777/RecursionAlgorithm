using System;
using System.Collections.Generic;
using System.Numerics;

namespace RecursionAlgorithm.Models;

public readonly record struct FastPowResult(BigInteger Value, long RecursiveCalls);

/// <summary>Бинарное быстрое возведение в степень с подсчётом рекурсивных вызовов.</summary>
public sealed class FastPowModel
{
    //логи
    private int _currentId = 1;
    public List<string> RecursionCalls { get; } = new();

    public FastPowResult Calculate(BigInteger basis, int exponent)
    {
        if (exponent < 0)
            throw new ArgumentOutOfRangeException(nameof(exponent), "Показатель должен быть неотрицательным.");

        // 1. Сброс состояния перед каждым расчетом
        RecursionCalls.Clear();
        _currentId = 1;
        long calls = 0;

        // 2. Считаем общее число шагов деления пополам, чтобы depth уменьшался строго на 1
        int initialDepth = CalculateTotalSteps(exponent);

        // 3. Запуск рекурсии
        BigInteger value = Pow(basis, exponent, ref calls, parentId: 0, depth: initialDepth, branch: "Left");
        return new FastPowResult(value, calls);
    }

    /// <summary>Подготавливает точки графика: количество рекурсивных вызовов для показателей 1..maxExponent.</summary>
    public (double[] Exponents, double[] RecursiveCalls) BuildBenchmark(BigInteger basis, int maxExponent)
    {
        maxExponent = Math.Clamp(maxExponent, 1, 10000);
        var exponents = new double[maxExponent];
        var calls = new double[maxExponent];

        for (int exponent = 1; exponent <= maxExponent; exponent++)
        {
            exponents[exponent - 1] = exponent;
            calls[exponent - 1] = Calculate(basis, exponent).RecursiveCalls;
        }

        return (exponents, calls);
    }

    private BigInteger Pow(BigInteger basis, int exponent, ref long calls, int parentId, int depth, string branch)
    {
        calls++;
        int myId = _currentId++;

        // Компактная подпись узла
        string customInfo = $"Pow({basis}, {exponent})";

        // Формируем стандартную строку лога: ID;ParentID;Depth;Branch;CustomInfo
        RecursionCalls.Add($"{myId};{parentId};{depth};{branch};{customInfo}");

        // Базовый случай: x^0 = 1 (узел запишется в дерево как терминальный лист)
        if (exponent == 0)
            return BigInteger.One;

        // Единственный рекурсивный вызов (передаем depth - 1)
        BigInteger half = Pow(basis, exponent / 2, ref calls, parentId: myId, depth: depth - 1, branch: "Left");
        
        BigInteger square = half * half;
        return exponent % 2 == 0 ? square : square * basis;
    }

    /// <summary>Вычисляет количество вызовов до нуля (для правильного масштабирования в UI).</summary>
    private static int CalculateTotalSteps(int exponent)
    {
        int steps = 1; // для вызова с exponent == 0
        while (exponent > 0)
        {
            steps++;
            exponent /= 2;
        }
        return steps;
    }
}