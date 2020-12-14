using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DurableTask.Web.Contracts.Handler {
    public interface IRequest {
    }
    public interface IResponse {
    }
    public interface IRequestContext {
    }
    public interface IRequestHandler<TRequest, TResponse>
        where TRequest : class, IRequest
        where TResponse : class, IResponse {
        Task<TResponse> Handle(
            TRequest request,
            IRequestContext context,
            CancellationToken cancellationToken);
    }
}
