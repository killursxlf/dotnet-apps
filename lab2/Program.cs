using System.Text;
using IoTTelemetryLab.Data;
using IoTTelemetryLab.Evaluation;
using IoTTelemetryLab.Models;
using IoTTelemetryLab.Monitoring;
using IoTTelemetryLab.Monitoring.Subscribers;
using IoTTelemetryLab.Processing;

Console.OutputEncoding = Encoding.UTF8;

const string deviceId = "sensor-08";
const SensorType sensorType = SensorType.Ph;

var registry = EvaluatorSetup.CreateDefaultRegistry();
var classEvaluator = new PhEvaluator();
registry.Register(sensorType, classEvaluator);

var rawData = TelemetryGenerator.GenerateRawBatch(deviceId, count: 50, minValue: 4, maxValue: 10);
var cleaned = TelemetryCleaner.RemoveInvalid(rawData).ToList();
var classStatuses = cleaned.Select(r => registry.Evaluate(sensorType, r.Value)).ToArray();

registry.Register(sensorType, value => value switch
{
    < 5.5 or > 8.5 => SafetyStatus.Critical,
    < 6.0 or > 8.0 => SafetyStatus.Warning,
    _ => SafetyStatus.Normal
});

Console.WriteLine($"Варіант №8: pH рідини ({deviceId}, {sensorType})");
Console.WriteLine("Critical: pH < 5.5 або pH > 8.5; Warning: 5.5 <= pH < 6.0 або 8.0 < pH <= 8.5; Normal: 6.0 <= pH <= 8.0");
Console.WriteLine("\npH | Клас | Func");
foreach (double value in new[] { 5.4, 5.5, 5.9, 6.0, 7.0, 8.0, 8.1, 8.5, 8.6 })
{
    Console.WriteLine($"{value:F1} | {classEvaluator.Evaluate(value)} | {registry.Evaluate(sensorType, value)}");
}

bool sameStatuses = classStatuses.SequenceEqual(
    cleaned.Select(r => registry.Evaluate(sensorType, r.Value)));
Console.WriteLine($"\nКлас і Func дають однакові результати на всіх очищених показах: {sameStatuses}");
Console.WriteLine($"Отримано показів: {rawData.Count}; після очищення: {cleaned.Count}; відкинуто: {rawData.Count - cleaned.Count}\n");

var monitor = new TelemetryMonitor(registry, sensorType);
var counter = new CriticalCounterSubscriber();
var intervals = new CriticalIntervalSubscriber();

monitor.OnStatusEvaluated += ConsoleAlertSubscriber.OnStatusEvaluated;
monitor.OnStatusEvaluated += counter.OnStatusEvaluated;
monitor.OnStatusEvaluated += intervals.OnStatusEvaluated;

foreach (var reading in cleaned)
{
    monitor.Process(reading);
}

Console.WriteLine($"\nВсього критичних станів зафіксовано лічильником: {counter.CriticalCount}");
Console.WriteLine($"Критичних станів за аналітикою: {TelemetryAnalytics.CountByStatus(cleaned, sensorType, SafetyStatus.Critical, registry)}");
