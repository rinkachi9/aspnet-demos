using Grpc.Core;
using Grpc.Core.Interceptors;

namespace AdvancedRealTime.Interceptors;

public class GlobalExceptionInterceptor : Interceptor
{
    private readonly ILogger<GlobalExceptionInterceptor> _logger;

    public GlobalExceptionInterceptor(ILogger<GlobalExceptionInterceptor> logger)
    {
        _logger = logger;
    }

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
            return await continuation(request, context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "gRPC Unary Error");
            throw HandleException(ex);
        }
    }

    public override async Task DuplexStreamingServerHandler<TRequest, TResponse>(IAsyncStreamReader<TRequest> requestStream, IServerStreamWriter<TResponse> responseStream, ServerCallContext context, DuplexStreamingServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
             await continuation(requestStream, responseStream, context);
        }
        catch (Exception ex)
        {
             _logger.LogError(ex, "gRPC Duplex Error");
             throw HandleException(ex);
        }
    }

    private RpcException HandleException(Exception ex)
    {
        var status = ex switch
        {
            ArgumentException => new Status(StatusCode.InvalidArgument, ex.Message),
            TimeoutException => new Status(StatusCode.DeadlineExceeded, "Operation timed out"),
            _ => new Status(StatusCode.Internal, "Internal Server Error")
        };

        return new RpcException(status);
    }
}
