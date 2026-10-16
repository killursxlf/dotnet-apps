using IoTTelemetryLab.Models;

namespace IoTTelemetryLab.Evaluation;

public class PhEvaluator : ISensorEvaluator
{
    public SafetyStatus Evaluate(double value) => value switch
    {
        < 5.5 or > 8.5 => SafetyStatus.Critical,
        < 6.0 or > 8.0 => SafetyStatus.Warning,
        _ => SafetyStatus.Normal
    };
}
