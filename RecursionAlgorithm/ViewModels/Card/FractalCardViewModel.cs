using System;
using RecursionAlgorithm.ViewModels.Интерфейс_для_карточки;

namespace RecursionAlgorithm.ViewModels;

public partial class FractalCardViewModel : CardBaseViewModel, IFractalCard
{
    public float Angle { get; init; }
    public float Size { get; init; }
    public int RecursionDeep { get; init; }

    public override string RangeText => $"Угол: {Angle:F0}°, Размер: {Size:F1}, Глубина: {RecursionDeep}";

    public FractalCardViewModel(
        string title,
        string[] names,
        string[] difficult,
        string accentColor,
        float angle,
        float size,
        int recursionDeep,
        Action<CardBaseViewModel>? onOpen = null)
        : base(title, names, difficult, accentColor, onOpen)
    {
        Angle = angle;
        Size = size;
        RecursionDeep = recursionDeep;
    }
}