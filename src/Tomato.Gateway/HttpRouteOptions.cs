using System;
using System.Collections.Generic;
using System.Reflection;

namespace Tomato.Gateway
{
    public class HttpRouteOptions
    {
        public List<RouteItem> Items { get; } = new List<RouteItem>();
    }

    public class RouteItem
    {
        public int ServiceId { get; set; }
        public ushort MessageId { get; set; }
        public string Category { get; set; }

        public RestfulVerb AcceptVerb { get; set; }
        public string Path { get; set; }
        public string Version { get; set; } = "1.0.0";

        internal IHttpPlugin Plugin { get; set; }
        public MethodInfo InvokeMethod { get; set; }
        public object InvokeService { get; set; }
    }
}
