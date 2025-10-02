using System.Diagnostics;
using Serilog.Core;
using Serilog.Events;

namespace Order.Infrastructure.Monitoring;

public class TraceEnricher : ILogEventEnricher
{
    
    readonly string _traceId = "trace_id";
    readonly string _spanId = "span_id";
    
    
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var activity = Activity.Current;

        if (activity == null)
            return;

        if (!string.IsNullOrEmpty(activity.TraceId.ToString()))
        {
            logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty(_traceId, activity.TraceId.ToString()));
        }

        if (!string.IsNullOrEmpty(activity.SpanId.ToString()))
        {
            logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty(_spanId, activity.SpanId.ToString()));
        }
    }
}