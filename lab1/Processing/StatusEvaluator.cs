using IoTTelemetryLab.Models;

namespace IoTTelemetryLab.Processing;

public enum SafetyStatus { Normal, Warning, Critical }

public static class StatusEvaluator
{
    public static SafetyStatus Evaluate(SensorType type, double value) => type switch
    {
        SensorType.Temperature => value switch
        {
            < 0 or > 80 => SafetyStatus.Critical,
            >= 60 and <= 80 => SafetyStatus.Warning,
            _ => SafetyStatus.Normal
        },
        SensorType.Vibration => value switch
        {
            > 5.0 => SafetyStatus.Critical,
            > 2.5 => SafetyStatus.Warning,
            _ => SafetyStatus.Normal
        },
        SensorType.Voltage => value switch
        {
            < 200 or > 240 => SafetyStatus.Critical,
            < 210 or > 230 => SafetyStatus.Warning,
            _ => SafetyStatus.Normal
        },
        SensorType.WindSpeed => value switch
        {
            > 25 => SafetyStatus.Critical,
            > 15 and <= 25 => SafetyStatus.Warning,
            _ => SafetyStatus.Normal
        },
        _ => throw new ArgumentOutOfRangeException(nameof(type))
    };
}
