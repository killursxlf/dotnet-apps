using IoTTelemetryLab.Models;

namespace IoTTelemetryLab.Evaluation;

public class SensorEvaluatorRegistry
{
    private readonly Dictionary<SensorType, Func<double, SafetyStatus>> _evaluators = new();

    public void Register(SensorType type, ISensorEvaluator evaluator)
    {
        ArgumentNullException.ThrowIfNull(evaluator);
        _evaluators[type] = evaluator.Evaluate;
    }

    public void Register(SensorType type, Func<double, SafetyStatus> evaluatorFunc)
    {
        ArgumentNullException.ThrowIfNull(evaluatorFunc);
        _evaluators[type] = evaluatorFunc;
    }

    public SafetyStatus Evaluate(SensorType type, double value)
    {
        if (!_evaluators.TryGetValue(type, out var evaluator))
        {
            throw new InvalidOperationException(
                $"Немає зареєстрованого оцінювача для типу {type}");
        }

        return evaluator(value);
    }
}
