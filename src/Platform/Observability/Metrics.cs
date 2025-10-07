using System.Diagnostics.Metrics;

namespace Platform.Observability;

public static class BusinessMetrics
{
    private static readonly Meter _meter = new("business.metrics", "1.0.0");
    
    // Order metrics
    public static readonly Counter<long> OrdersCreated = _meter.CreateCounter<long>(
        name: "orders_created_total",
        description: "Total number of orders created");
        
    public static readonly Counter<long> OrdersFulfilled = _meter.CreateCounter<long>(
        name: "orders_fulfilled_total", 
        description: "Total number of orders fulfilled");
        
    public static readonly Counter<long> OrdersCancelled = _meter.CreateCounter<long>(
        name: "orders_cancelled_total",
        description: "Total number of orders cancelled");
    
    public static readonly Histogram<double> OrderValue = _meter.CreateHistogram<double>(
        name: "order_value_amount",
        description: "Distribution of order values",
        unit: "currency");
    
    public static readonly Histogram<double> OrderCancelledValue = _meter.CreateHistogram<double>(
        name: "orders_cancelled_value",
        description: "Total amount of orders cancelled"
    );

    public static readonly Histogram<double> OrderFulFilledValue = _meter.CreateHistogram<double>(
        name: "orders_fulfilled_value",
        description: "Total value of orders fulfilled"
    );
    
    public static readonly Histogram<double> OrderProcessingTime = _meter.CreateHistogram<double>(
        name: "order_processing_duration_seconds",
        description: "Time taken to process orders",
        unit: "seconds");

    
    public static readonly Counter<long> OrderProductsReservation= _meter.CreateCounter<long>(
        name: "order_products_reservation_total",
        description: "Total number of order products reservation");
        
    public static readonly Counter<long> OrderProductsReservationErrors = _meter.CreateCounter<long>(
        name: "order_products_reservation_error_total",
        description: "Total number of failed order products reservation errors");
    
    
    // External service metrics
    public static readonly Counter<long> ExternalServiceCalls = _meter.CreateCounter<long>(
        name: "external_service_calls_total",
        description: "Total number of external service calls");
        
    public static readonly Counter<long> ExternalServiceErrors = _meter.CreateCounter<long>(
        name: "external_service_errors_total",
        description: "Total number of external service errors");
    
    // Database metrics
    public static readonly Counter<long> DatabaseOperations = _meter.CreateCounter<long>(
        name: "database_operations_total",
        description: "Total number of database operations");
    
    
    // Business error metrics
    public static readonly Counter<long> BusinessErrors = _meter.CreateCounter<long>(
        name: "business_errors_total",
        description: "Total number of business errors");
        
    public static class Labels
    {
        public const string Service = "service";
        public const string Operation = "operation";
        public const string Status = "status";
        public const string ErrorType = "error_type";
        public const string Currency = "currency";
        public const string ExternalService = "external_service";
        public const string DatabaseTable = "database_table";
        public const string Repository = "respository";
        public const string UserId = "user_id";
        public const string OrderStatus = "order_status";
    }
}