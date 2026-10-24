using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Media;

namespace RecursionAlgorithm.Models.FractalLogic;

/// <summary>
/// Чистый класс модели фрактала (никаких наследований от Control/Visual).
/// Здесь реализуется алгоритм и вся геометрия.
/// </summary>
public class PythagorasTreeModel
{
    //список рекурсивных вызовов
    private int _currentId = 0;
    public List<string> RecursionCalls = new List<string>(); 
    
    // Параметры дерева
    public float Angle { get; set; } = 45f;
    public int Depth { get; set; } = 10;
    IBrush Brush = Brushes.SaddleBrown;
    private static readonly IPen BorderPen = new Pen(Brushes.Black, 0.5);

    /// <summary>
    /// Точка входа для отрисовки, которую вызывает контрол-прослойка (PythagorasTreeControl).
    /// </summary>
    public void Draw(DrawingContext context, Rect bounds)
    {
        // 1. Длина стороны базового квадрата (ствола):
        double side = Math.Min(bounds.Height * 0.17, bounds.Width * 0.15);

        // 2. Точки основания A и B:
         double centerX = bounds.Width / 2.0;
        double groundY = bounds.Height - 35;
        Point a = new Point(centerX - side / 2.0, groundY);
        Point b = new Point(centerX + side / 2.0, groundY);
        RecursionCalls.Clear();// отчиста списка логов
        _currentId = 0;
        DrawTree(context, a, b, Depth, 0, "Root");
        
        
    }

    /// <summary>
    /// Метод отрисовки квадрата по базовым точкам основания
    /// </summary>
    public (Point d, Point c) DrawingSquare(Point a, Point b, DrawingContext context)
    {
        // 1. Вектор основания от A к B
        double dx = b.X - a.X;                                              // Смещение по X
        double dy = b.Y - a.Y;                                              // Смещение по Y

        // 2. Вектор перпендикуляра "вверх" (поворот на 90 градусов)
        Point d = new Point(a.X + dy, a.Y - dx);                            // Верхний левый угол квадрата
        Point c = new Point(b.X + dy, b.Y - dx);                            // Верхний правый угол квадрата

        // 3. Построение контура квадрата (A -> B -> C -> D)
        StreamGeometry geometry = new StreamGeometry();
        using (StreamGeometryContext ctx = geometry.Open())
        {
            ctx.BeginFigure(a, isFilled: true);                             // Встаем в точку A
            ctx.LineTo(b);                                                  // Отрезок в точку B
            ctx.LineTo(c);                                                  // Отрезок вверх в точку C
            ctx.LineTo(d);                                                  // Отрезок влево в точку D
            ctx.EndFigure(isClosed: true);                                  // Замыкаем обратно в A
        }

        // 4. Отрисовка в контекст
        context.DrawGeometry(Brush, BorderPen, geometry);                   // Рисуем заливку и контур пера из шага 5

        return (d, c);                                                      // Возвращаем верхние точки для треугольника
    }

    /// <summary>
    /// Метод отрисовки прямоугольного треугольника на крыше квадрата
    /// </summary>
    public void DrawingTriangle(Point d, Point c, Point e, DrawingContext context)
    {
        // 1. Построение контура треугольника (D -> C -> E)
        StreamGeometry geometry = new StreamGeometry();
        using (StreamGeometryContext ctx = geometry.Open())
        {
            ctx.BeginFigure(d, isFilled: true);                             // Встаем в левый угол гипотенузы D
            ctx.LineTo(c);                                                  // Линия по гипотенузе в точку C
            ctx.LineTo(e);                                                  // Линия к вершине прямого угла E
            ctx.EndFigure(isClosed: true);                                  // Замыкаем обратно в D
        }

        // 2. Отрисовка в контекст
        context.DrawGeometry(Brush, BorderPen, geometry);                   // Заливаем треугольник
    }


    private void DrawTree(DrawingContext context, Point a, Point b, int depth, int parant_id, string branch)
    {
        
        
        int my_id = ++_currentId;
        RecursionCalls.Add($"{my_id};{parant_id};{depth};{branch}");
        //устанавливаем цвет кистьи
        SetBrush(depth);
        if (depth <= 0)
        {
            return;
        }
        //рисуем квадрат
        var (d, c) = DrawingSquare(a, b, context);
        if (depth <= 1) return;
        // 3. Вычисляем вершину E треугольника над гипотенузой DC
        double dx = c.X - d.X;                                              // Вектор гипотенузы по X
        double dy = c.Y - d.Y;                             // Вектор гипотенузы по Y
        double rad = Angle * Math.PI / 180.0;                               // Перевод угла в радианы
        double cosA = Math.Cos(rad);                                        // Косинус
        double sinA = Math.Sin(rad);                                        // Синус
        Point e = new Point(
            d.X + (cosA * cosA) * dx + (cosA * sinA) * dy,                  // Координата X вершины E
            d.Y + (cosA * cosA) * dy + (cosA * sinA) * (-dx)                // Координата Y вершины E
        );
        // 4. Отрисовываем треугольник
        DrawingTriangle(d, c, e, context);
        // 5. Рекурсивное ветвление на катетах треугольника
        DrawTree(context, d, e, depth - 1, my_id,"Left");                                 // Левая ветвь на отрезке DE
        DrawTree(context, e, c, depth - 1, my_id, "Right");                                 // Правая ветвь на отрезке EC
        
    }
    private void SetBrush(int depth)// блок настройки кисти под текущую глубину
    {
        
        double progress = Math.Clamp(1.0 - ((double)depth / Depth), 0.0, 1.0);
        byte rStart = 139, gStart = 69, bStart = 19;// Начальный цвет: Коричневый (SaddleBrown)
        byte rEnd = 34, gEnd = 139, bEnd = 34;// Конечный цвет: Лесной зеленый (ForestGreen)
        byte red = (byte)(rStart + (rEnd - rStart) * progress);
        byte green = (byte)(gStart + (gEnd - gStart) * progress);
        byte blue = (byte)(bStart + (bEnd - bStart) * progress);
        Brush = new SolidColorBrush(Color.FromRgb(red, green, blue));
    }
}