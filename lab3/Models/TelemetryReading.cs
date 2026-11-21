namespace IoTTelemetryLab.Models;

public readonly record struct TelemetryReading(string DeviceId, double Value, DateTime Timestamp);
