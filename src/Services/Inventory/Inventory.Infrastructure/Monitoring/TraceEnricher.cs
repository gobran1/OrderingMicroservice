using System.Diagnostics;
using Serilog.Core;
using Serilog.Events;

namespace Inventory.Infrastructure.Monitoring;

public class TraceEnricher : ILogEventEnricher
{
    private readonly string _traceId = "trace_id";
    private readonly string _spanId = "span_id";
    
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var activity = Activity.Current;
        
        if (activity == null)
            return;
        
        var traceId = activity.TraceId.ToString();
        var spanId = activity.SpanId.ToString();

        if (!string.IsNullOrEmpty(traceId))
        {
            logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty(_traceId, traceId));
        }

        if (!string.IsNullOrEmpty(spanId))
        {
            logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty(_spanId, spanId));
        }
        
    }
}