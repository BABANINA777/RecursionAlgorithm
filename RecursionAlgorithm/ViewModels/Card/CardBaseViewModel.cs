using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RecursionAlgorithm.ViewModels.Интерфейс_для_карточки;

namespace RecursionAlgorithm.ViewModels;

public abstract partial class CardBaseViewModel : ObservableObject, ICard
{
    public string Title { get; init; }
    public string[] Names { get; init; }
    public string[] Difficult { get; init; }
    public string AccentColor { get; init; }
    public abstract string RangeText { get; }

    private readonly Action<CardBaseViewModel>? _onOpen;

    protected CardBaseViewModel(string title, string[] names, string[] difficult, string accentColor, Action<CardBaseViewModel>? onOpen = null)
    {
        Title = title;
        Names = names;
        Difficult = difficult;
        AccentColor = accentColor;
        _onOpen = onOpen;
    }

    [RelayCommand]
    public void Open() => _onOpen?.Invoke(this);
}
