using Grpc.Core.Interceptors;
using Grpc.Core;
using LeafletAlarms.Authentication;

namespace LeafletAlarms.Grpc.Implementation
{
  public class GrpcContextInterceptor : Interceptor
  {
    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
      try
      {
        GrpcRequestContextProvider.SetContext(context);
        return await continuation(request, context);
      }
      finally
      {
        GrpcRequestContextProvider.ClearContext();
      }
    }
  }
}
