using DurableTask.Core;

using Microsoft.Extensions.DependencyInjection;

using System;

namespace DurableTask.DependencyInjection {
    //https://andrewstevens.dev/posts/dependency-injection-durable-task/
    public class ServiceProviderObjectCreator<T> : ObjectCreator<T> {
        readonly IServiceProvider serviceProvider;

        public ServiceProviderObjectCreator(IServiceProvider serviceProvider) {
            this.serviceProvider = serviceProvider;
            Initialize();
        }

        public override T Create() {
            return this.serviceProvider.GetRequiredService<T>();
        }

        void Initialize() {
            Name = NameVersionHelper.GetDefaultName(typeof(T));
            Version = NameVersionHelper.GetDefaultVersion(typeof(T));
        }
    }
}
