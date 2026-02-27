using DevAndrew.Calculator.Core.Models;

namespace DevAndrew.Calculator.Core.Interfaces
{
    public interface IStateRepository
    {
        CalculatorState Load();

        bool TrySave(CalculatorState state);
    }
}
