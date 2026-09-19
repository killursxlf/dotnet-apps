using IoTTelemetryLab.Models;

namespace IoTTelemetryLab.Data;

public static class TelemetryGenerator
{
    public static List<TelemetryReading> GenerateRawBatch(
        string deviceId, int count, double minValue, double maxValue)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(deviceId);
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        if (!double.IsFinite(minValue) || !double.IsFinite(maxValue) ||
            minValue > maxValue || !double.IsFinite(maxValue - minValue))
            throw new ArgumentException("Межі мають бути скінченними, minValue <= maxValue.");

        var random = new Random();
        var readings = new List<TelemetryReading>();
        var baseTime = DateTime.UtcNow;
        for (int i = 0; i < count; i++)
        {
            double value = random.NextDouble() < 0.1
                ? (random.Next(2) == 0 ? -999.0 : double.NaN)
                : Math.Round(minValue + random.NextDouble() * (maxValue - minValue), 2);
            readings.Add(new TelemetryReading(deviceId, value, baseTime.AddSeconds(i * 5.0)));
        }
        return readings;
    }
}
