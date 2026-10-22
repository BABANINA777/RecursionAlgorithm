using System;
using RecursionAlgorithm.ViewModels.Интерфейс_для_карточки;

namespace RecursionAlgorithm.ViewModels;

public partial class AlgorithmCardViewModel : CardBaseViewModel, IAlgorithmCard
{
    public int DefaultStartN { get; init; }
    public int DefaultEndN { get; init; }
    public override string RangeText => $"Число {DefaultStartN}, степень {DefaultEndN}";

    public AlgorithmCardViewModel(
        string title,
        string[] names,
        string[] difficult,
        string accentColor,
        int defaultStartN,
        int defaultEndN,
        Action<CardBaseViewModel>? onOpen = null)
        : base(title, names, difficult, accentColor, onOpen)
    {
        DefaultStartN = defaultStartN;
        DefaultEndN = defaultEndN;
    }
}
