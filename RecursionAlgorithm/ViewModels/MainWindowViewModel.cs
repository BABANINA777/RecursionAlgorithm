using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace RecursionAlgorithm.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    public ObservableCollection<CardBaseViewModel> Cards { get; } = new();

    [ObservableProperty]
    private bool _isCardsVisible = true;

    [ObservableProperty]
    private CardBaseViewModel? _selectedCard;

    [ObservableProperty]
    private bool _isHanoiVisible;

    [ObservableProperty]
    private bool _isTreeVisible;

    [ObservableProperty]
    private bool _isKnapsackVisible;

    [ObservableProperty]
    private bool _isFastPowVisible;

    public MainWindowViewModel()
    {
        // 1. Ханойские башни (Алгоритм)
        Cards.Add(new AlgorithmCardViewModel(
            "Ханойские башни",
            ["Рекурсивный", "Стек"],
            ["O(2ⁿ)", "O(2ⁿ)"],
            "#7C3AED",
            1, 10,
            OpenCard));

        // 2. Дерево Пифагора (Фрактал)
        Cards.Add(new FractalCardViewModel(
            "Дерево Пифагора",
            ["Ветвление"],
            ["O(2ⁿ)"],
            "#059669",
            28f, 1f, 12,
            OpenCard));

        // 3. Задача о рюкзаке (Алгоритм)
        Cards.Add(new AlgorithmCardViewModel(
            "Задача о рюкзаке",
            ["Рекурсивный", "Хвостовой"],
            ["O(2ⁿ)", "O(n)"],
            "#D97706",
            1, 45,
            OpenCard));

        // 4. Быстрое возведение в степень (Алгоритм)
        Cards.Add(new AlgorithmCardViewModel(
            "Быстрое возведение в степень",
            ["Бинарное возведение"],
            ["O(log n)"],
            "#2563EB",
            1, 1000,
            OpenCard));
    }

    public void OpenCard(CardBaseViewModel card)
    {
        SelectedCard = card;
        isAlghoritmVisible();
    }

    private void isAlghoritmVisible()
    {
        // Сначала скрываем все контейнеры
        IsHanoiVisible = false;
        IsTreeVisible = false;
        IsKnapsackVisible = false;
        IsFastPowVisible = false;

        // Если ничего не выбрано — показываем главный экран с карточками
        if (SelectedCard == null)
        {
            IsCardsVisible = true;
            return;
        }

        // Карточки скрываем, а нужный алгоритм показываем
        IsCardsVisible = false;

        switch (SelectedCard.Title)
        {
            case "Ханойские башни":
                IsHanoiVisible = true;
                break;
            case "Дерево Пифагора":
                IsTreeVisible = true;
                break;
            case "Задача о рюкзаке":
                IsKnapsackVisible = true;
                break;
            case "Быстрое возведение в степень":
                IsFastPowVisible = true;
                break;
        }
    }

    [RelayCommand]
    public void GoBack()
    {
        SelectedCard = null;
        isAlghoritmVisible();
    }
}
