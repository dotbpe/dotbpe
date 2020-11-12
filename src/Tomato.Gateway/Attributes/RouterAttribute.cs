using System;

namespace Tomato.Gateway
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RouterAttribute:Attribute
    {

      public RouterAttribute(string path,RestfulVerb acceptVerb= RestfulVerb.Any,string version ="1.0.0")
              {
                  Path = path;
                  AcceptVerb = acceptVerb;
                  Version = version;
              }


              public string Category { get; set; } = "default";
              public string Path { get; }

              public string Version { get; } = "1.0.0";

              public RestfulVerb AcceptVerb { get;  }

              public string PluginName { get; set; }


        private Type _PluginType;
        public Type PluginType
        {
            get
            {
                if (!string.IsNullOrEmpty(PluginName))
                {
                    if (_PluginType != null)
                    {
                        return _PluginType;
                    }
                    _PluginType =Type.GetType(PluginName);
                    return _PluginType;
                }

                return null;
            }
        }
    }
}
