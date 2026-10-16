using IoTTelemetryLab.Models;

namespace IoTTelemetryLab.Monitoring.Subscribers;

public class CriticalIntervalSubscriber
{
    private DateTime? _previousCriticalTimestamp;

    public TimeSpan? LastInterval { get; private set; }

    public void OnStatusEvaluated(TelemetryReading reading, SafetyStatus status)
    {
        if (status != SafetyStatus.Critical)
        {
            return;
        }

        if (_previousCriticalTimestamp is DateTime previous)
        {
            LastInterval = reading.Timestamp - previous;
            Console.WriteLine(
                $"[ІНТЕРВАЛ] {reading.DeviceId}: між {previous:HH:mm:ss} і {reading.Timestamp:HH:mm:ss} — {LastInterval}");
        }

        _previousCriticalTimestamp = reading.Timestamp;
    }
}
