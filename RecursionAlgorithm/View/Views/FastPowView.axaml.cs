using System;
using System.Numerics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using RecursionAlgorithm.Models;
using ScottPlot;

namespace RecursionAlgorithm.View.Views;

public partial class FastPowView : UserControl
{
    private readonly FastPowModel _model = new();
    private ScottPlot.Plottables.Crosshair? _crosshair;
    private bool _isInitialized;

    public FastPowView()
    {
        InitializeComponent();
        BenchmarkPlot.PointerMoved += OnPlotPointerMoved;
        Loaded += (_, _) =>
        {
            if (_isInitialized) return;
            _isInitialized = true;
            DrawGraph();
        };
        
    }

    private void OnPlotPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_crosshair is null) return;
        var point = e.GetPosition(BenchmarkPlot);
        _crosshair.Position = BenchmarkPlot.Plot.GetCoordinates(new Pixel((float)point.X, (float)point.Y));
        BenchmarkPlot.Refresh();
    }

    private void DrawGraph()
    {
        if (!BigInteger.TryParse(BasisInput?.Text, out BigInteger basis)) basis = 2;
        if (!int.TryParse(ExponentInput?.Text, out int maxExponent)) maxExponent = 64;
        maxExponent = Math.Clamp(maxExponent, 1, 10000);

        FastPowResult result = _model.Calculate(basis, maxExponent);
        var benchmark = _model.BuildBenchmark(basis, maxExponent);

        StepsResultText.Text = result.RecursiveCalls.ToString("N0");
        string value = result.Value.ToString();
        ValueResultText.Text = value.Length > 34 ? $"{value[..30]}…" : value;

        BenchmarkPlot.Plot.Clear();
        BenchmarkPlot.Plot.FigureBackground.Color = Color.FromHex("#50505A");
        BenchmarkPlot.Plot.DataBackground.Color = Color.FromHex("#3B3B44");
        BenchmarkPlot.Plot.Axes.Color(Color.FromHex("#F3F4F6"));
        BenchmarkPlot.Plot.Grid.MajorLineColor = Color.FromHex("#44FFFFFF");
        BenchmarkPlot.Plot.XLabel("Показатель степени (n)");
        BenchmarkPlot.Plot.YLabel("Количество рекурсивных вызовов");
        _crosshair = BenchmarkPlot.Plot.Add.Crosshair(0, 0);
        _crosshair.LineWidth = 1;
        _crosshair.LinePattern = LinePattern.Dashed;
        _crosshair.LineColor = Color.FromHex("#77FFFFFF");

        var scatter = BenchmarkPlot.Plot.Add.Scatter(benchmark.Exponents, benchmark.RecursiveCalls);
        scatter.Color = Color.FromHex("#60A5FA");
        scatter.LineWidth = 2.5f;
        scatter.MarkerSize = maxExponent <= 200 ? 5 : 0;
        BenchmarkPlot.Plot.Axes.AutoScale();
        BenchmarkPlot.Refresh();
        DrawRecursionTree();
    }

    private void RunButton_Click(object? sender, RoutedEventArgs e) => DrawGraph();

    private void OnInputKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            DrawGraph();
    }
    public void DrawRecursionTree()
    {
        RecursionControl.RecursionCalls = _model.RecursionCalls.ToArray();
        RecursionControl.RenderRecursionTree();

    }
}
