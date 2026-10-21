using System;
using System.Collections.Generic;

namespace RecursionAlgorithm.Models;

public readonly record struct HanoiMove(int Disk, int From, int To);

/// <summary>Решение задачи о Ханойских башнях и получение состояния на заданном ходе.</summary>
public sealed class HanoiModel
{
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
        Solve(DiskCount, 0, 2, 1);
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

    private void Solve(int disks, int from, int to, int spare)
    {
        RecursiveCalls++;
        if (disks == 0)
            return;

        Solve(disks - 1, from, spare, to);
        _moves.Add(new HanoiMove(disks, from, to));
        Solve(disks - 1, spare, to, from);
    }
}
