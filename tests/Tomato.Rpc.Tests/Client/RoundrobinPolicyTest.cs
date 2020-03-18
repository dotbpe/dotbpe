using Tomato.Rpc.Client;
using Tomato.Rpc.Client.RouterPolicy;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Tomato.Rpc.Tests.Client
{
    public class RoundrobinPolicyTest
    {
        [Fact]
        public void Test1()
        {
            var p = new RoundrobinPolicy();

            var listRouters = new List<IRouterPoint>
            {
                new RouterPoint { Weight =1},
                new RouterPoint { Weight =2}
            };

            for(var i =0; i< 100000; i++)
            {
                var route =  p.Select("a", listRouters);
                if( (i % 2) == 0)
                {
                    Assert.Equal(1, route.Weight);
                }
                else
                {
                    Assert.Equal(2, route.Weight);
                }

            }
        }
    }
}
