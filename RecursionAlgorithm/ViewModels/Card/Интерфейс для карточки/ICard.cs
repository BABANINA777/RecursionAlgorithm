namespace RecursionAlgorithm.ViewModels.Интерфейс_для_карточки;

public interface ICard
{
    string Title { get; init; }
    string[] Names { get; init; }
    string[] Difficult { get; init; }
    string AccentColor { get; init; }
    string RangeText { get; }
}
