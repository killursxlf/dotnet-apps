using IoTTelemetryLab.Models;

namespace IoTTelemetryLab.Evaluation;

public class WindSpeedEvaluator : ISensorEvaluator
{
    public SafetyStatus Evaluate(double value) => value switch
    {
        > 25 => SafetyStatus.Critical,
        > 15 and <= 25 => SafetyStatus.Warning,
        _ => SafetyStatus.Normal
    };
}
