using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using RecursionAlgorithm.Models.FractalLogic;

namespace RecursionAlgorithm.View.Views;

/// <summary>
/// элемент Avalonia UI.
/// Задача этого класса — предоставить Avalonia точку входа в визуальное дерево (через Control)
/// и передать DrawingContext в чистый класс модели по принципу КОМПОЗИЦИИ.
/// Сам контрол не содержит алгоритмов и расчетов геометрии.
/// </summary>
public class PythagorasTreeControl : Control
{
    // Композиция: контрол содержит чистую модель как поле
    public PythagorasTreeModel Model { get; } = new();
    public string[] RecursionCalls;
    public PythagorasTreeControl(){ClipToBounds = true;}
    
    // =========================================================================
    // Обработка мыши
    // =========================================================================
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
            InvalidateVisual();
            e.Handled = true;
            return;
        }

        if (point.Properties.IsLeftButtonPressed)
        {
            _isPanning = true;
            _lastPointerPosition = e.GetPosition(this);

            e.Pointer.Capture(this);

            Cursor = new Cursor(StandardCursorType.SizeAll);
            
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
            Vector delta = currentPosition - _lastPointerPosition;
            
            // Сдвигаем полотно
            _panOffset += delta;
            _lastPointerPosition = currentPosition;

            // Запрашиваем перерисовку кадра
            InvalidateVisual();

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
            Cursor = Cursor.Default;
            e.Handled = true;
        }
    }
    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)// Если операционная система принудительно отобрала мышь (Alt+Tab, модальное окно) — аварийно сбрасываем состояние перетаскивания!
    {
        base.OnPointerCaptureLost(e);
        
        _isPanning = false;
        Cursor = Cursor.Default;
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
            InvalidateVisual();
        }

        e.Handled = true;
    }
    
    
    public static readonly StyledProperty<float> AngleProperty =
        AvaloniaProperty.Register<PythagorasTreeControl, float>(nameof(Angle), defaultValue: 45f);
    public static readonly StyledProperty<int> DepthProperty =
        AvaloniaProperty.Register<PythagorasTreeControl, int>(nameof(Depth), defaultValue: 10);
    public float Angle
    {
        get => GetValue(AngleProperty);
        set => SetValue(AngleProperty, value);
    }
    public int Depth
    {
        get => GetValue(DepthProperty);
        set => SetValue(DepthProperty, value);
    }
    static PythagorasTreeControl()
    {
        AngleProperty.Changed.AddClassHandler<PythagorasTreeControl>((control, e) => control.InvalidateVisual());
        DepthProperty.Changed.AddClassHandler<PythagorasTreeControl>((control, e) => control.InvalidateVisual());
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="context"></param>
    ///
    public event Action RenderCompleted;
    public override void Render(DrawingContext context)
    {
        base.Render(context);
        context.FillRectangle(Brushes.Transparent,  new Rect(Bounds.Size));
        Matrix transform = Matrix.CreateScale(_zoom, _zoom) * Matrix.CreateTranslation(_panOffset.X, _panOffset.Y);
        using (context.PushTransform(transform))
        {
            // Делегируем вызов отрисовки модели
            Model.Angle = Angle;
            Model.Depth = Depth;
            Model.Draw(context, Bounds);
            RecursionCalls = Model.RecursionCalls.ToArray();
        }
        // Говорим диспетчеру: вызови событие ТОЛЬКО ПОСЛЕ того, как кадр полностью отрисуется
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            RenderCompleted?.Invoke();
        }, Avalonia.Threading.DispatcherPriority.Background);
    
    }
    
}