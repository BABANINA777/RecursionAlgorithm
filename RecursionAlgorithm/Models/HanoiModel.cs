using System;
using System.Collections.Generic;

namespace RecursionAlgorithm.Models;

public readonly record struct HanoiMove(int Disk, int From, int To);

/// <summary>Решение задачи о Ханойских башнях и получение состояния на заданном ходе.</summary>
public sealed class HanoiModel
{
    private int _currentId = 1;
    public List<string> RecursionCalls { get; } = new();
    private readonly List<HanoiMove> _moves = [];

    public int DiskCount { get; private set; }
    public IReadOnlyList<HanoiMove> Moves => _moves;
    public long MinimumMoveCount => _moves.Count;
    public long RecursiveCalls { get; private set; }

    public HanoiModel(int diskCount = 5) => SetDiskCount(diskCount);

    public void SetDiskCount(int diskCount)
    {
        DiskCount = Math.Clamp(diskCount, 1, 12);
        _moves.Clear();
        RecursiveCalls = 0;

        // 1. Очищаем историю и сбрасываем счетчик ID
        RecursionCalls.Clear();
        _currentId = 1;

        // 2. Запускаем корень: parentId = 0, ветка "Root"
        Solve(DiskCount, from: 0, to: 2, spare: 1, parentId: 0, branch: "Root");
    }

    /// <summary>Возвращает диски на трёх башнях после указанного количества ходов.</summary>
    public IReadOnlyList<int>[] GetTowerStates(int completedMoves)
    {
        completedMoves = Math.Clamp(completedMoves, 0, _moves.Count);
        var towers = new List<int>[] { [], [], [] };
        for (int disk = DiskCount; disk >= 1; disk--)
            towers[0].Add(disk);

        for (int i = 0; i < completedMoves; i++)
        {
            HanoiMove move = _moves[i];
            towers[move.From].RemoveAt(towers[move.From].Count - 1);
            towers[move.To].Add(move.Disk);
        }

        return [towers[0].AsReadOnly(), towers[1].AsReadOnly(), towers[2].AsReadOnly()];
    }

    private void Solve(int disks, int from, int to, int spare, int parentId, string branch)
    {
        // Не логируем пустые базовые вызовы без дисков
        if (disks == 0)
            return;

        RecursiveCalls++;

        // Присваиваем вызову текущий ID и увеличиваем счетчик для следующих
        int myId = _currentId++;

        // Превращаем 0, 1, 2 в A, B, C
        char fromPeg = (char)('A' + from);
        char toPeg = (char)('A' + to);
        string customInfo = $"H({disks}, {fromPeg}→{toPeg})";

        // Формируем строку: ID;ParentID;Depth;Branch;CustomInfo
        RecursionCalls.Add($"{myId};{parentId};{disks};{branch};{customInfo}");

        // Левая ветка: перекладываем n-1 дисков на запасной шпиль
        Solve(disks - 1, from, spare, to, myId, "Left");

        // Фиксация перемещения текущего диска
        _moves.Add(new HanoiMove(disks, from, to));

        // Правая ветка: перекладываем n-1 дисков с запасного на целевой
        Solve(disks - 1, spare, to, from, myId, "Right");
    }
}