using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using RecursionAlgorithm.Models;



namespace RecursionAlgorithm.View.Views.PythagorasTree;

public partial class RecursionTreePT : UserControl
{
    public string[] RecursionCalls;
    public RecursionTreePT()
    {
        InitializeComponent();
        ClipToBounds = true;// для правильной обработки мыши
    }

    public void RenderRecursionTree()
    {
        TreeCanvas.Children.Clear();
        
        //подготовка данных
        var nodes = RecursionCalls.Select(ParseText).ToList();
        var dict_nodes = new Dictionary<int, Point>();
        double baseStep = 400;   // Базовый размах веток для первого уровня
        double levelHeight = 70; // Расстояние между уровнями по вертикали
        int maxDepth = nodes[0].depth;// Максимальная глубина (глубина корня)
        
        
        // создание первого блока
        var textBlock = CreateTextBlock(nodes[0].description);
        double parantTop = 120;
        double parantLeft = 1000;
        Canvas.SetTop(textBlock, parantTop);
        Canvas.SetLeft(textBlock, parantLeft);
        TreeCanvas.Children.Add(textBlock);
        dict_nodes.Add(nodes[0].my_id, new Point(parantLeft, parantTop));
        
        
        foreach (var item in nodes.Skip(1))
        {
            // ВЫЧИСЛЯЕМ КООРДИНАТЫ
            int level = maxDepth - item.depth;
            if (level > 4 ) continue;//ограничение отрисовуи
            
            // Если у элемента нет родителя (parent_id == 0) или родитель не найден — не трогаем словарь
            if (item.parent_id == 0 || !dict_nodes.TryGetValue(item.parent_id, out Point parant_point))
            {
                continue;
            }
            

            // Координата Y всегда смещается вниз от родителя
            double y = parant_point.Y + levelHeight;
            double x;

            // Координата X зависит от стороны ветвления
            double stepX = 80 * Math.Pow(2, 4 - level);
            if (item.branch == "Left")
            {
                x = parant_point.X - stepX;
            }
            else // Right
            {
                x = parant_point.X + stepX;
            }
            dict_nodes.Add(item.my_id, new Point(x, y));
            
            //СОЗДАЕМ БЛОК
            var text_Block = CreateTextBlock(item.description);
            Canvas.SetTop(text_Block, y);
            Canvas.SetLeft(text_Block, x);
            TreeCanvas.Children.Add(text_Block);
            
            //СОЗДАЕМ ЛИНИЮ
            var line = CreateArrow(parant_point, new Point(x, y));
            TreeCanvas.Children.Add(line);
        }
        
        
    }

    private Control CreateTextBlock(string text)
    {
        return new Border
        {
            // 1. Цвета фона и тонкой рамки
            Background = new SolidColorBrush(Avalonia.Media.Color.Parse("#1E222D")),
            BorderBrush = new SolidColorBrush(Avalonia.Media.Color.Parse("#2E3445")),
            BorderThickness = new Thickness(1),

            // 2. Скругление углов
            CornerRadius = new CornerRadius(6),

            // 3. Внутренние отступы, чтобы текст не лип к краям
            Padding = new Thickness(10, 5),

            // 4. Мягкая современная тень: смещение X, смещение Y, размытие, сжатие, цвет
            BoxShadow = BoxShadows.Parse("0 4 12 0 #35000000"),

            // 5. Сам текст внутри плашки
            Child = new TextBlock
            {
                Text = text,
                Foreground = new SolidColorBrush(Avalonia.Media.Color.Parse("#E2E8F0")),
                FontSize = 11,
                FontWeight = Avalonia.Media.FontWeight.Medium,
                TextAlignment = TextAlignment.Center,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
            }
        };
    }

    private Path CreateArrow(Point start, Point end, double arrowHeadLength = 10, double arrowHeadWidth = 6)
    {
        // Вектор направления линии от старта к концу
        double dx = end.X - start.X;
        double dy = end.Y - start.Y;
        double length = Math.Sqrt(dx * dx + dy * dy);

        // Если точки совпадают, возвращаем пустой путь во избежание деления на ноль
        if (length < 0.001)
        {
            return new Path();
        }

        // Единичный вектор вдоль линии и перпендикулярный ему вектор
        double uX = dx / length;
        double uY = dy / length;

        double pX = -uY;
        double pY = uX;

        // Базовая точка у основания стрелки
        double baseX = end.X - uX * arrowHeadLength;
        double baseY = end.Y - uY * arrowHeadLength;

        // Две боковые вершины треугольника стрелки
        Point arrowWing1 = new Point(baseX + pX * (arrowHeadWidth / 2), baseY + pY * (arrowHeadWidth / 2));
        Point arrowWing2 = new Point(baseX - pX * (arrowHeadWidth / 2), baseY - pY * (arrowHeadWidth / 2));

        // Описываем геометрию: основная линия + треугольник на конце
        var geometry = new PathGeometry();

        // 1. Основной отрезок (от start до основания стрелки)
        var lineFigure = new PathFigure { StartPoint = start, IsClosed = false };
        lineFigure.Segments.Add(new LineSegment { Point = new Point(baseX, baseY) });
        geometry.Figures.Add(lineFigure);

        // 2. Треугольный наконечник стрелки
        var headFigure = new PathFigure { StartPoint = end, IsClosed = true, IsFilled = true };
        headFigure.Segments.Add(new LineSegment { Point = arrowWing1 });
        headFigure.Segments.Add(new LineSegment { Point = arrowWing2 });
        geometry.Figures.Add(headFigure);

        return new Path
        {
            Data = geometry,
            Stroke = Brushes.Gray,
            Fill = Brushes.Gray, // Заливка треугольника наконечника
            StrokeThickness = 2
        };
    }

    private ParseLogText ParseText(string text)
    {
        string[] textarray = text.Split(";");
        int my_id = int.Parse(textarray[0]);
        int parent_id = int.Parse(textarray[1]);
        int depth = int.Parse(textarray[2]);
        string branch = textarray[3];
        return new ParseLogText(my_id, parent_id, depth, branch);
    }
    
    #region Обоаботка мышкой прибляжения и перетаскивания
// Переменные состояния масштабирования и перемещения
    private double _zoom = 1.0;                                             // Масштаб (1.0 = 100%)
    private Point _panOffset = new Point(0, 0);                             // Смещение холста (панорамирование)
    private Point _lastPointerPosition;                                     // Предыдущая точка курсора при перетаскивании
    private bool _isPanning = false;                                        // Флаг зажатия кнопки мыши

    protected override void OnPointerPressed(PointerPressedEventArgs e)//метод обработки кликов
    {
        base.OnPointerPressed(e);
        var point = e.GetCurrentPoint(this);

        // Двойной клик ЛКМ: сбрасываем масштаб и смещение в исходное состояние
        if (e.ClickCount == 2 && point.Properties.IsLeftButtonPressed)
        {
            _zoom = 1.0;
            _panOffset = new Point(0, 0);
            ApplyTransform();
            e.Handled = true;
            return;
        }

        if (point.Properties.IsLeftButtonPressed)
        {
            _isPanning = true;
            _lastPointerPosition = e.GetPosition(this);

            e.Pointer.Capture(this);

            Cursor  = new Avalonia.Input.Cursor(StandardCursorType.SizeAll);
            
            e.Handled = true;// Помечаем событие обработанным, чтобы родительские контейнеры не реагировали
        }
    }
    protected override void OnPointerMoved(PointerEventArgs e)//метод обработки перетаскивания
    {
        base.OnPointerMoved(e);

        if (_isPanning)
        {
            Point currentPosition = e.GetPosition(this);
            
            // Вычисляем вектор смещения мыши с предыдущего кадра
            Avalonia.Vector delta = currentPosition - _lastPointerPosition;
            
            // Сдвигаем полотно
            _panOffset += delta;
            _lastPointerPosition = currentPosition;

            // Запрашиваем перерисовку кадра
            ApplyTransform();

            e.Handled = true;
        }
    }
    protected override void OnPointerReleased(PointerReleasedEventArgs e)//метод обработки отжатия
    {
        base.OnPointerReleased(e);

        if (_isPanning && e.InitialPressMouseButton == MouseButton.Left)
        {
            _isPanning = false;
            // ОБЯЗАТЕЛЬНО: Освобождаем захват мыши!
            e.Pointer.Capture(null);
            // Возвращаем стандартный курсор
            Cursor = null;
            e.Handled = true;
        }
    }
    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)// Если операционная система принудительно отобрала мышь (Alt+Tab, модальное окно) — аварийно сбрасываем состояние перетаскивания!
    {
        base.OnPointerCaptureLost(e);
        
        _isPanning = false;
        Cursor = null;
    }
    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);

        // Точка под курсором в локальных координатах
        Point mousePos = e.GetPosition(this);

        // Коэффициент изменения масштаба (e.Delta.Y > 0 — колесико вверх, приближение)
        double zoomFactor = e.Delta.Y > 0 ? 1.15 : (1.0 / 1.15);
        double newZoom = Math.Clamp(_zoom * zoomFactor, min: 0.1, max: 20.0);

        if (Math.Abs(newZoom - _zoom) > 0.001)
        {
            // МАТЕМАТИКА ЗУМА В ТОЧКУ:
            // Чтобы точка под курсором осталась на том же месте на экране:
            // Смещаем PanOffset пропорционально новому масштабу
            double scaleRatio = newZoom / _zoom;
            _panOffset = new Point(
                mousePos.X - (mousePos.X - _panOffset.X) * scaleRatio,
                mousePos.Y - (mousePos.Y - _panOffset.Y) * scaleRatio
            );

            _zoom = newZoom;

            // Перерисовываем холст
            ApplyTransform();
        }

        e.Handled = true;
    }
    private void ApplyTransform()
    {
        if (TreeCanvas != null)
        {
            // Создаем матрицу масштабирования и смещения
            var matrix = Matrix.CreateScale(_zoom, _zoom) * Matrix.CreateTranslation(_panOffset.X, _panOffset.Y);
        
            // Напрямую двигаем Canvas аппаратным ускорением
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
    
    public ParseLogText(int myId,int parentId, int depth,string text)
    {
        my_id = myId;
        parent_id = parentId;
        this.depth = depth;
        branch = text;
        description = $"Глубина: {depth}, №{myId}";
    }
    
    
}

