namespace RecursionAlgorithm.ViewModels.Интерфейс_для_карточки;

public interface IFractalCard : ICard
{
    float Angle { get; init; }
    float Size { get; init; }
    int RecursionDeep { get; init; }
}