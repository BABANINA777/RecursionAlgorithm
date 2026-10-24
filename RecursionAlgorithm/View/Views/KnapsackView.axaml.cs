using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using RecursionAlgorithm.Models;
using ScottPlot;

namespace RecursionAlgorithm.View.Views;

public partial class KnapsackView : UserControl
{
    private ScottPlot.Plottables.Crosshair? _crosshair;
    private bool _isInitialized = false;
    private KnapsackModel _model = new KnapsackModel(50, 15);

    public static readonly StyledProperty<int> NSizeProperty =
        AvaloniaProperty.Register<KnapsackView, int>(nameof(NSize), defaultValue: 15);

    public static readonly StyledProperty<int> MaxWeightProperty =
        AvaloniaProperty.Register<KnapsackView, int>(nameof(MaxWeight), defaultValue: 50);

    public int NSize
    {
        get => GetValue(NSizeProperty);
        set => SetValue(NSizeProperty, value);
    }

    public int MaxWeight
    {
        get => GetValue(MaxWeightProperty);
        set => SetValue(MaxWeightProperty, value);
    }

    static KnapsackView()
    {
        NSizeProperty.Changed.AddClassHandler<KnapsackView>((control, e) => control.OnParameterChanged());
        MaxWeightProperty.Changed.AddClassHandler<KnapsackView>((control, e) => control.OnParameterChanged());
    }

    public KnapsackView()
    {
        InitializeComponent();

        // Подписываемся на перемещение мыши для движения перекрестия
        BenchmarkPlot.PointerMoved += (sender, e) =>
        {
            if (_crosshair == null) return;
            var pixelPosition = e.GetPosition(BenchmarkPlot);
            Pixel mousePixel = new((float)pixelPosition.X, (float)pixelPosition.Y);
            Coordinates coords = BenchmarkPlot.Plot.GetCoordinates(mousePixel);
            _crosshair.Position = coords;
            BenchmarkPlot.Refresh();
        };

        Loaded += (s, e) =>
        {
            if (!_isInitialized)
            {
                ShowGrafic();
                DrawGrafic();
                _isInitialized = true;
            }
        };
    }

    private void ShowGrafic()
    {
        BenchmarkPlot.Plot.Clear();
        BenchmarkPlot.Plot.Axes.Rules.Clear();

        // Тёмная тема графика
        BenchmarkPlot.Plot.FigureBackground.Color = Color.FromHex("#50505A");
        BenchmarkPlot.Plot.DataBackground.Color = Color.FromHex("#3B3B44");
        BenchmarkPlot.Plot.Axes.Color(Color.FromHex("#F3F4F6"));
        BenchmarkPlot.Plot.Grid.MajorLineColor = Color.FromHex("#44FFFFFF");

        // Подписи осей
        BenchmarkPlot.Plot.XLabel("Размерность входных данных (N)");
        BenchmarkPlot.Plot.YLabel("Количество шагов рекурсии");

        // Перекрестие
        _crosshair = BenchmarkPlot.Plot.Add.Crosshair(0, 0);
        _crosshair.LineWidth = 1;
        _crosshair.LinePattern = LinePattern.Dashed;
        _crosshair.LineColor = Color.FromHex("#77FFFFFF");
    }

    private void DrawGrafic()
    {
        // Валидация входных данных (ограничение до 24, чтобы не подвесить UI)
        int n = Math.Clamp(NSize, 1, 24);
        int w = Math.Max(1, MaxWeight);

        double[] xs = new double[n];
        double[] ys = new double[n];

        // Выполняем расчёт для каждого размера от 1 до N для построения кривой сложности
        for (int i = 1; i <= n; i++)
        {
            var stepModel = new KnapsackModel(w, i);
            stepModel.RunAlgorithm();
            xs[i - 1] = i;
            ys[i - 1] = stepModel.RecursionSteps;

            if (i == n)
            {
                _model = stepModel;
            }
        }

        // Обновляем плашки с результатами
        if (StepsResultText != null)
            StepsResultText.Text = _model.RecursionSteps.ToString("N0");

        if (BestCostResultText != null)
            BestCostResultText.Text = $"{_model.BestCost:N0} ₽";

        // Отрисовка графика
        ShowGrafic();
        var scatter = BenchmarkPlot.Plot.Add.Scatter(xs, ys);
        scatter.Color = Color.FromHex("#F59E0B"); // Янтарный оттенок под цвет карточки
        scatter.LineWidth = 2.5f;
        scatter.MarkerSize = 7;

        BenchmarkPlot.Plot.Axes.AutoScale();
        BenchmarkPlot.Refresh();
        DrawRecursionTree();
    }

    private void OnParameterChanged()
    {
        if (!_isInitialized) return;
        DrawGrafic();
    }

    private void RunButton_Click(object? sender, RoutedEventArgs e)
    {
        if (int.TryParse(NSizeInput?.Text, out int n))
            NSize = n;
        if (int.TryParse(MaxWeightInput?.Text, out int w))
            MaxWeight = w;

        DrawGrafic();
    }

    private void OnInputKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            RunButton_Click(sender, e);
        }
    }
    
    public void DrawRecursionTree()
    {
        RecursionControl.RecursionCalls = _model.RecursionCalls.ToArray();
        RecursionControl.RenderRecursionTree();

    }
}
