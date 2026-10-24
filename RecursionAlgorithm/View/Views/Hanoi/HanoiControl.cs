using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using RecursionAlgorithm.Models;

namespace RecursionAlgorithm.View.Views;

/// <summary>Визуальный адаптер модели Ханойских башен.</summary>
public sealed class HanoiControl : Control
{
    public string[] RecursionCalls = [];//лог выполнения
    public static readonly StyledProperty<int> DiskCountProperty =
        AvaloniaProperty.Register<HanoiControl, int>(nameof(DiskCount), 5);

    public static readonly StyledProperty<int> CurrentMoveProperty =
        AvaloniaProperty.Register<HanoiControl, int>(nameof(CurrentMove), 0);

    public int DiskCount
    {
        get => GetValue(DiskCountProperty);
        set => SetValue(DiskCountProperty, value);
    }

    public int CurrentMove
    {
        get => GetValue(CurrentMoveProperty);
        private set => SetValue(CurrentMoveProperty, value);
    }

    public HanoiModel Model { get; } = new(5);
    public event EventHandler? StateChanged;

    private readonly DispatcherTimer _timer;

    public HanoiControl()
    {
        ClipToBounds = true;
        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(380) };
        _timer.Tick += (_, _) => Advance();
    }

    static HanoiControl()
    {
        DiskCountProperty.Changed.AddClassHandler<HanoiControl>((control, _) => control.Rebuild());
        CurrentMoveProperty.Changed.AddClassHandler<HanoiControl>((control, _) =>
        {
            control.InvalidateVisual();
            control.StateChanged?.Invoke(control, EventArgs.Empty);
        });
    }

    public void Play()
    {
        if (CurrentMove >= Model.Moves.Count)
            CurrentMove = 0;
        _timer.Start();
    }

    public void Pause() => _timer.Stop();

    public void Reset()
    {
        _timer.Stop();
        CurrentMove = 0;
    }

    private void Rebuild()
    {
        _timer.Stop();
        Model.SetDiskCount(Math.Clamp(DiskCount, 1, 12));
        CurrentMove = 0;
        InvalidateVisual();
    }

    private void Advance()
    {
        if (CurrentMove >= Model.Moves.Count)
        {
            _timer.Stop();
            return;
        }

        CurrentMove++;
    }

    public event Action? RenderCompleted;
    public override void Render(DrawingContext context)
    {
        base.Render(context);
        context.FillRectangle(new SolidColorBrush(Color.Parse("#141418")), new Rect(Bounds.Size));

        double width = Bounds.Width;
        double height = Bounds.Height;
        if (width <= 0 || height <= 0)
            return;

        double groundY = height - 38;
        double rodTop = 30;
        double[] centers = [width / 6, width / 2, width * 5 / 6];
        var rodPen = new Pen(new SolidColorBrush(Color.Parse("#A0A0B2")), 5);
        var groundPen = new Pen(new SolidColorBrush(Color.Parse("#53536A")), 4);
        context.DrawLine(groundPen, new Point(24, groundY + 3), new Point(width - 24, groundY + 3));

        for (int i = 0; i < centers.Length; i++)
        {
            context.DrawLine(rodPen, new Point(centers[i], rodTop), new Point(centers[i], groundY));
            var label = new FormattedText(
                ((char)('A' + i)).ToString(),
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                Typeface.Default,
                14,
                new SolidColorBrush(Color.Parse("#A0A0B2")));
            context.DrawText(label, new Point(centers[i] - label.Width / 2, groundY + 12));
        }

        var towers = Model.GetTowerStates(CurrentMove);
        double diskHeight = Math.Clamp((height - 95) / Math.Max(Model.DiskCount, 1), 12, 30);
        double maxDiskWidth = Math.Min(width / 3.6, 190);
        double minDiskWidth = Math.Max(28, maxDiskWidth * 0.28);

        for (int tower = 0; tower < towers.Length; tower++)
        {
            for (int level = 0; level < towers[tower].Count; level++)
            {
                int disk = towers[tower][level];
                double ratio = (double)disk / Model.DiskCount;
                double diskWidth = minDiskWidth + (maxDiskWidth - minDiskWidth) * ratio;
                double x = centers[tower] - diskWidth / 2;
                double y = groundY - (level + 1) * diskHeight;
                byte red = (byte)(124 + disk * 8 % 100);
                byte green = (byte)(58 + disk * 11 % 90);
                byte blue = (byte)(237 - disk * 5 % 80);
                context.DrawRectangle(new SolidColorBrush(Color.FromRgb(red, green, blue)), null,
                    new Rect(x, y, diskWidth, diskHeight - 2), 7, 7);
            }
        }
        
        RecursionCalls = Model.RecursionCalls.ToArray();
        // Говорим диспетчеру: вызови событие ТОЛЬКО ПОСЛЕ того, как кадр полностью отрисуется
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            RenderCompleted?.Invoke();
        }, Avalonia.Threading.DispatcherPriority.Background);
    }
}
