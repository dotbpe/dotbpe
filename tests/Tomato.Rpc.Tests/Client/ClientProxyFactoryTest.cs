using Tomato.Rpc.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Tomato.Rpc.Tests.Client
{
    public class ClientProxyFactoryTest
    {
        [Fact]
        public void AddDependencyServicesTest()
        {
            IServiceCollection container = new ServiceCollection();

            var factory = ClientProxyFactory.Create(container);


            factory.AddDependencyServices(services => services.AddSingleton<IFooService, FooService>());


            var provider = container.BuildServiceProvider();

            var fooService = provider.GetRequiredService<IFooService>();

            Assert.NotNull(fooService);
        }

        [Fact]
        public void BuildFactoryWithoutError()
        {
            var clientProxy = new Mock<IClientProxy>();
            var proxy = ClientProxyFactory.Create()
                .AddDependencyServices(services => { services.AddSingleton(clientProxy.Object); })
                .GetClientProxy();

            Assert.NotNull(proxy);
        }

        [Fact]
        public void ConfigureTest()
        {
            IServiceCollection container = new ServiceCollection();

            var factory = ClientProxyFactory.Create(container);

            factory.Configure<FooOptions>(o =>
            {
                o.Option1 = "111";
                o.Option2 = 222;
            });


            var provider = container.BuildServiceProvider();

            var options = provider.GetRequiredService<IOptions<FooOptions>>();

            Assert.NotNull(options);
            Assert.NotNull(options.Value);

            Assert.Equal("111", options.Value.Option1);
            Assert.Equal(222, options.Value.Option2);
        }
    }
}
