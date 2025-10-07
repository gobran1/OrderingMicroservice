using System.Diagnostics;

namespace Platform.Observability;

public static class Tracing
{
    public static class ActivityNames
    {
        public const string OrderCreation = "order.create";
        public const string OrderRetrieval = "order.retrieve";
        public const string OrderCancellation = "order.cancel";
        public const string OrderFulfillment = "order.fulfill";
        public const string GetProductsDetails = "product.GetDetails";
        public const string InventoryCheck = "inventory.check";
        public const string PaymentProcessing = "payment.process";
        public const string DatabaseOperation = "database.operation";
        public const string ExternalServiceCall = "external.service.call";
    }
    
    public static class Tags
    {
        public const string UserId = "user.id";
        public const string OrderId = "order.id";
        public const string ProductId = "product.id";
        public const string ProductIds = "product.ids";
        public const string OrderStatus = "order.status";
        public const string ProductCount = "product.count";
        public const string TotalAmount = "order.total_amount";
        public const string Currency = "order.currency";
        public const string DatabaseTable = "database.table";
        public const string ExternalService = "external.service.name";
        public const string ErrorType = "error.type";
        public const string BusinessError = "business.error";
    }
    
    public static void AddBusinessContext(this Activity? activity, string userId, string orderId, long totalAmount, string currency)
    {
        activity?.SetTag(Tags.UserId, userId);
        activity?.SetTag(Tags.OrderId, orderId);
        activity?.SetTag(Tags.TotalAmount, totalAmount);
        activity?.SetTag(Tags.Currency, currency);
    }
    
    public static void AddProductContext(this Activity? activity, string productId, int quantity)
    {
        activity?.SetTag(Tags.ProductId, productId);
        activity?.SetTag("product.quantity", quantity);
    }
    
    public static void AddDatabaseContext(this Activity? activity, string tableName, string operation)
    {
        activity?.SetTag(Tags.DatabaseTable, tableName);
        activity?.SetTag("database.operation", operation);
    }
    
    public static void AddExternalServiceContext(this Activity? activity, string serviceName, string operation)
    {
        activity?.SetTag(Tags.ExternalService, serviceName);
        activity?.SetTag("external.operation", operation);
    }
    
    public static void RecordBusinessError(this Activity? activity, string errorType, string errorMessage)
    {
        activity?.SetTag(Tags.BusinessError, errorType);
        activity?.SetTag("error.message", errorMessage);
        activity?.SetStatus(ActivityStatusCode.Error, errorMessage);
    }
    
    public static void RecordError(this Activity? activity, string errorType, string errorMessage)
    {
        activity?.SetTag(Tags.ErrorType, errorType);
        activity?.SetTag("error.message", errorMessage);
        activity?.SetStatus(ActivityStatusCode.Error, errorMessage);
    }
    
    public static void RecordSuccess(this Activity? activity, string? successMessage = null)
    {
        activity?.SetStatus(ActivityStatusCode.Ok, successMessage);
    }
    
}