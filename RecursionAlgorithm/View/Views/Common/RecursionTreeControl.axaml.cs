using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;

namespace RecursionAlgorithm.View.Views.Common;

/// <summary>
/// Универсальный визуальный контрол дерева рекурсивных вызовов.
/// Поддерживает автоматическое позиционирование узлов, соединительные стрелки,
/// панорамирование зажатой ЛКМ и масштабирование колесиком мыши в точку курсора.
/// </summary>
public partial class RecursionTreeControl : UserControl
{
    public string[] RecursionCalls = [];

    public RecursionTreeControl()
    {
        InitializeComponent();
        ClipToBounds = true;
    }

    public void RenderRecursionTree()
    {
        TreeCanvas.Children.Clear();

        if (RecursionCalls == null || RecursionCalls.Length == 0)
            return;

        // Подготовка данных
        var nodes = RecursionCalls.Select(ParseText).ToList();
        if (nodes.Count == 0)
            return;

        var dictNodes = new Dictionary<int, Point>();
        double levelHeight = 70; // Расстояние между уровнями по вертикали
        int maxDepth = nodes[0].depth; // Максимальная глубина (глубина корня)

        // Создание корневого узла
        var rootBlock = CreateTextBlock(nodes[0].description, nodes[0].branch);
        double parentTop = 100;
        double parentLeft = TreeCanvas.Width / 2.0;

        Canvas.SetTop(rootBlock, parentTop);
        Canvas.SetLeft(rootBlock, parentLeft);
        TreeCanvas.Children.Add(rootBlock);
        dictNodes[nodes[0].my_id] = new Point(parentLeft, parentTop);

        foreach (var item in nodes.Skip(1))
        {
            // Ограничение глубины для ветвящихся деревьев (Left/Right), чтобы не перегружать экран
            int level = maxDepth - item.depth;
            if (item.branch != "Center" && level > 4)
                continue;

            // Если родитель не найден — пропускаем
            if (item.parent_id == 0 || !dictNodes.TryGetValue(item.parent_id, out Point parentPoint))
                continue;

            // Координата Y всегда смещается вниз от родителя
            double y = parentPoint.Y + levelHeight;
            double x;

            // Координата X зависит от стороны ветвления
            double stepX = 80 * Math.Pow(2, Math.Max(0, 4 - level));
            if (item.branch == "Left")
            {
                x = parentPoint.X - stepX;
            }
            else if (item.branch == "Right")
            {
                x = parentPoint.X + stepX;
            }
            else // "Center" или по центру (для линейной цепи вызовов, например быстрого возведения)
            {
                x = parentPoint.X;
            }

            dictNodes[item.my_id] = new Point(x, y);

            // Создаем плашку узла
            var textBlock = CreateTextBlock(item.description, item.branch);
            Canvas.SetTop(textBlock, y);
            Canvas.SetLeft(textBlock, x);
            TreeCanvas.Children.Add(textBlock);

            // Создаем соединительную стрелку от родителя к потомку
            var line = CreateArrow(parentPoint, new Point(x, y));
            TreeCanvas.Children.Add(line);
        }
    }

    private Control CreateTextBlock(string text, string branch = "Root")
    {
        return new Border
        {
            Background = new SolidColorBrush(Color.Parse("#1E222D")),
            BorderBrush = new SolidColorBrush(Color.Parse("#2E3445")),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(10, 5),
            BoxShadow = BoxShadows.Parse("0 4 12 0 #35000000"),
            Child = new TextBlock
            {
                Text = text,
                Foreground = new SolidColorBrush(Color.Parse("#E2E8F0")),
                FontSize = 11,
                FontWeight = FontWeight.Medium,
                TextAlignment = TextAlignment.Center,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
            }
        };
    }

    private Path CreateArrow(Point start, Point end, double arrowHeadLength = 10, double arrowHeadWidth = 6)
    {
        double dx = end.X - start.X;
        double dy = end.Y - start.Y;
        double length = Math.Sqrt(dx * dx + dy * dy);

        if (length < 0.001)
        {
            return new Path();
        }

        double uX = dx / length;
        double uY = dy / length;

        double pX = -uY;
        double pY = uX;

        double baseX = end.X - uX * arrowHeadLength;
        double baseY = end.Y - uY * arrowHeadLength;

        Point arrowWing1 = new Point(baseX + pX * (arrowHeadWidth / 2), baseY + pY * (arrowHeadWidth / 2));
        Point arrowWing2 = new Point(baseX - pX * (arrowHeadWidth / 2), baseY - pY * (arrowHeadWidth / 2));

        var lineFigure = new PathFigure
        {
            StartPoint = start,
            IsClosed = false,
            Segments = new PathSegments { new LineSegment { Point = new Point(baseX, baseY) } }
        };

        var headFigure = new PathFigure
        {
            StartPoint = end,
            IsClosed = true,
            IsFilled = true,
            Segments = new PathSegments
            {
                new LineSegment { Point = arrowWing1 },
                new LineSegment { Point = arrowWing2 }
            }
        };

        var geometry = new PathGeometry
        {
            Figures = new PathFigures { lineFigure, headFigure }
        };

        return new Path
        {
            Data = geometry,
            Stroke = Brushes.Gray,
            Fill = Brushes.Gray,
            StrokeThickness = 2
        };
    }

    /// <summary>
    /// Парсит строку лога рекурсии.
    /// Поддерживает как 4-компонентный формат: "myId;parentId;depth;branch",
    /// так и расширенный 5-компонентный формат: "myId;parentId;depth;branch;customInfo"
    /// с полной обратной совместимостью.
    /// </summary>
    private ParseLogText ParseText(string text)
    {
        string[] textarray = text.Split(";");
        int myId = int.Parse(textarray[0]);
        int parentId = int.Parse(textarray[1]);
        int depth = int.Parse(textarray[2]);
        string branch = textarray.Length > 3 ? textarray[3] : "Root";
        string? customInfo = textarray.Length >= 5 ? textarray[4] : null;

        return new ParseLogText(myId, parentId, depth, branch, customInfo);
    }

    #region Обработка мыши: панорамирование и зум в точку курсора

    private double _zoom = 1.0;
    private Point _panOffset = new Point(0, 0);
    private Point _lastPointerPosition;
    private bool _isPanning = false;

    public void ResetView()
    {
        _zoom = 1.0;
        _panOffset = new Point(0, 0);
        ApplyTransform();
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        var point = e.GetCurrentPoint(this);

        // Двойной клик ЛКМ: сброс масштаба и смещения в исходное состояние
        if (e.ClickCount == 2 && point.Properties.IsLeftButtonPressed)
        {
            ResetView();
            e.Handled = true;
            return;
        }

        if (point.Properties.IsLeftButtonPressed)
        {
            _isPanning = true;
            _lastPointerPosition = e.GetPosition(this);
            e.Pointer.Capture(this);
            Cursor = new Cursor(StandardCursorType.SizeAll);
            e.Handled = true;
        }
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        if (_isPanning)
        {
            Point currentPosition = e.GetPosition(this);
            Vector delta = currentPosition - _lastPointerPosition;
            _panOffset += delta;
            _lastPointerPosition = currentPosition;
            ApplyTransform();
            e.Handled = true;
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);

        if (_isPanning && e.InitialPressMouseButton == MouseButton.Left)
        {
            _isPanning = false;
            e.Pointer.Capture(null);
            Cursor = null;
            e.Handled = true;
        }
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        base.OnPointerCaptureLost(e);
        _isPanning = false;
        Cursor = null;
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);

        Point mousePos = e.GetPosition(this);
        double zoomFactor = e.Delta.Y > 0 ? 1.15 : (1.0 / 1.15);
        double newZoom = Math.Clamp(_zoom * zoomFactor, min: 0.1, max: 20.0);

        if (Math.Abs(newZoom - _zoom) > 0.001)
        {
            double scaleRatio = newZoom / _zoom;
            _panOffset = new Point(
                mousePos.X - (mousePos.X - _panOffset.X) * scaleRatio,
                mousePos.Y - (mousePos.Y - _panOffset.Y) * scaleRatio
            );

            _zoom = newZoom;
            ApplyTransform();
        }

        e.Handled = true;
    }

    private void ApplyTransform()
    {
        if (TreeCanvas != null)
        {
            var matrix = Matrix.CreateScale(_zoom, _zoom) * Matrix.CreateTranslation(_panOffset.X, _panOffset.Y);
            TreeCanvas.RenderTransform = new MatrixTransform(matrix);
        }
    }

    #endregion
}

public struct ParseLogText
{
    public int my_id;
    public int parent_id;
    public int depth;
    public string branch;
    public string description;

    public ParseLogText(int myId, int parentId, int depth, string branch, string? customInfo = null)
    {
        my_id = myId;
        parent_id = parentId;
        this.depth = depth;
        this.branch = branch;
        description = string.IsNullOrWhiteSpace(customInfo)
            ? $"Глубина: {depth}, №{myId}"
            : customInfo;
    }
}
