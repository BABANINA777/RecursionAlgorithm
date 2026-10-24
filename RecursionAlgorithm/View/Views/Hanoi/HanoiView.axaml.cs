using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace RecursionAlgorithm.View.Views;

public partial class HanoiView : UserControl
{
    public HanoiView()
    {
        InitializeComponent();
        UpdateProgress();
        HanoiBoard.RenderCompleted += DrawRecursionTree;
    }

    private void OnDiskCountTextChanged(object? sender, TextChangedEventArgs e)
    {
        ApplyDiskCount();
    }

    private void OnDiskInputKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            ApplyDiskCount();
    }

    private void ApplyDiskCount()
    {
        if (int.TryParse(DiskInput?.Text, out int value))
        {
            HanoiBoard.DiskCount = Math.Clamp(value, 1, 12);
            UpdateProgress();
        }
    }

    private void Play_Click(object? sender, RoutedEventArgs e) => HanoiBoard.Play();
    private void Pause_Click(object? sender, RoutedEventArgs e) => HanoiBoard.Pause();
    private void Reset_Click(object? sender, RoutedEventArgs e) => HanoiBoard.Reset();

    private void OnStateChanged(object? sender, EventArgs e) => UpdateProgress();

    private void UpdateProgress()
    {
        if (HanoiBoard is not null)
            ProgressText.Text = $"Ход {HanoiBoard.CurrentMove} из {HanoiBoard.Model.MinimumMoveCount:N0} · рекурсивных вызовов: {HanoiBoard.Model.RecursiveCalls:N0}";
    }
    public void DrawRecursionTree()
    {
        RecursionControl.RecursionCalls = HanoiBoard.RecursionCalls;
        RecursionControl.RenderRecursionTree();

    }
}
