using System;
using System.Numerics;

namespace RecursionAlgorithm.Models;

public readonly record struct FastPowResult(BigInteger Value, long RecursiveCalls);

/// <summary>Бинарное быстрое возведение в степень с подсчётом рекурсивных вызовов.</summary>
public sealed class FastPowModel
{
    public FastPowResult Calculate(BigInteger basis, int exponent)
    {
        if (exponent < 0)
            throw new ArgumentOutOfRangeException(nameof(exponent), "Показатель должен быть неотрицательным.");

        long calls = 0;
        BigInteger value = Pow(basis, exponent, ref calls);
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

    private static BigInteger Pow(BigInteger basis, int exponent, ref long calls)
    {
        calls++;
        if (exponent == 0)
            return BigInteger.One;

        BigInteger half = Pow(basis, exponent / 2, ref calls);
        BigInteger square = half * half;
        return exponent % 2 == 0 ? square : square * basis;
    }
}
