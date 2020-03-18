using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Tomato.Baseline.Extensions;
using Tomato.Gateway.Swagger.Generator;

namespace Tomato.Gateway.Swagger
{
    public class SwaggerApiInfoProvider : ISwaggerApiInfoProvider
    {
        private readonly IHttpServiceScanner _httpScanner;

        private string _swaggerJson = string.Empty;

        private XmlCommentResolver _resolver;

        public SwaggerApiInfoProvider(IHttpServiceScanner httpScanner)
        {
            this._httpScanner = httpScanner;
        }

        public void ScanApiInfo(SwaggerConfig config)
        {
            _resolver = new XmlCommentResolver(config.XmlComments);
            //所有路由信息
            var routeOptions = this._httpScanner.GetRuntimeRouteOptions();
            // config 包含配置和XML的注释信息

            SwaggerInfo swagger = new SwaggerInfo
            {
                Host = config.Host,
                Info = config.ApiInfo ?? new SwaggerApiInfo(),
                BasePath = config.BasePath,
                Paths = new Dictionary<string, Dictionary<string, SwaggerMethod>>(),
                Definitions = new Dictionary<string, SwaggerDefinition>(),
                Tags = new List<SwaggerTag>()
            };

            ProcessPaths(swagger.Paths, swagger.Tags, routeOptions, swagger.Definitions, config);

            var settings = new JsonSerializerSettings
            {
                DefaultValueHandling = DefaultValueHandling.Ignore
            };

            this._swaggerJson = JsonConvert.SerializeObject(swagger, Formatting.Indented, settings);

        }

        private void ProcessPaths(Dictionary<string, Dictionary<string, SwaggerMethod>> swaggerPaths,
            List<SwaggerTag> tags, HttpRouteOptions routeOptions, Dictionary<string, SwaggerDefinition> definitions, SwaggerConfig config)
        {
            routeOptions.Items.ForEach(item =>
            {
                var path = CreateSwaggerMethod(item.AcceptVerb);

                var tagName = item.InvokeMethod.DeclaringType.Name.Substring(1);

                if (!tags.Exists(x => x.Name == tagName))
                {
                    tags.Add(new SwaggerTag
                    {
                        Name = item.InvokeMethod.DeclaringType.Name.Substring(1),
                            Description = this._resolver.GetTypeComment(item.InvokeMethod.DeclaringType)
                    });
                }

                path.Tags = new List<string>
                {
                    //item.Category??"default",
                    //this._resolver.GetTypeComment(item.InvokeMethod.DeclaringType)
                    tagName
                };

                path.Summary = GetSummary(item);

                string verb;
                //path.Summary = item.AcceptVerb.ToString();
                if (item.AcceptVerb == RestfulVerb.Any || item.AcceptVerb == RestfulVerb.Get
                    || item.AcceptVerb == RestfulVerb.UnKnown)
                {

                    //path.Consumes = new List<string> {"application/json"};
                    verb = "get";
                }
                else if (item.AcceptVerb == RestfulVerb.Put || item.AcceptVerb == RestfulVerb.Post
                    || item.AcceptVerb == RestfulVerb.Patch
                    || item.AcceptVerb == RestfulVerb.Delete)
                {
                    path.Consumes = new List<string> { "application/x-www-form-urlencoded", "application/json" };
                    verb = item.AcceptVerb.ToString().ToLower();
                }
                else
                {
                    //path.Consumes = new List<string> {"application/x-www-form-urlencoded","application/json"};
                    verb = item.AcceptVerb.ToString().ToLower();
                }

                string invokeName = item.InvokeMethod.Name.EndsWith("Async")
                    ? item.InvokeMethod.Name.Substring(0, item.InvokeMethod.Name.Length - 5)
                    : item.InvokeMethod.Name;
                path.OperationId = invokeName;

                path.Description = this._resolver.GetMethodComment(item.InvokeMethod);

                path.Parameters = new List<SwaggerApiParameters>();

                ProcessParameters(item.AcceptVerb, path.Parameters, item.InvokeMethod.GetParameters(), definitions, config);

                path.Responses = new Dictionary<string, SwaggerApiResponse>();

                ProcessResponses(path.Responses, item.InvokeMethod.ReturnParameter, definitions, config);

                if (!swaggerPaths.ContainsKey(item.Path))
                {
                    swaggerPaths.Add(item.Path, new Dictionary<string, SwaggerMethod> { { verb, path } });
                }
                else
                {
                    var pathItem = swaggerPaths[item.Path];
                    if (!pathItem.ContainsKey(verb))
                    {
                        pathItem.Add(verb, path);
                    }
                }

            });
        }

        private string GetSummary(RouteItem item)
        {
            return $"{item.ServiceId}.{item.MessageId} - {item.InvokeMethod.DeclaringType.Name.Substring(1)}.{item.InvokeMethod.Name}";
        }

        private void ProcessResponses(Dictionary<string, SwaggerApiResponse> pathResponses,
            ParameterInfo invokeMethodReturnParameter, Dictionary<string, SwaggerDefinition> definitions, SwaggerConfig config)
        {
            //Task<RpcResult>
            if (!invokeMethodReturnParameter.ParameterType.IsGenericType)
            {
                return;
            }
            var rpcResultType = invokeMethodReturnParameter.ParameterType.GenericTypeArguments[0];
            if (!rpcResultType.IsGenericType) //Task<RpcResult>
            {
                return;
            }
            //Task<RpcResult<T>>
            var innerType = rpcResultType.GenericTypeArguments[0];

            SwaggerApiResponse apiResponse = new SwaggerApiResponse
            {
                Description = this._resolver.GetTypeComment(innerType),
                Schema = GetSwaggerItemSchema(innerType, definitions, config)
            };

            pathResponses.Add("200", apiResponse);

            //Console.WriteLine(innerType.FullName);
            //Console.WriteLine("---------------------------------------------------");
            CreateSwaggerDefinition(innerType.Name, innerType, definitions, config);
        }

        private void CreateSwaggerDefinition(string name, Type definitionType, Dictionary<string, SwaggerDefinition> definitions, SwaggerConfig config)
        {
            if (definitions.ContainsKey(name))
            {
                return;
            }

            if ("Type".Equals(name))
            {
                return;
            }

            SwaggerDefinition definition = new SwaggerDefinition
            {
                Type = "object",
                Properties = new Dictionary<string, SwaggerPropertyDefinition>()
            };

            definitions.Add(name, definition);

            var properties = definitionType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);

            properties.ForEach(p =>
            {
                SwaggerPropertyDefinition pd = new SwaggerPropertyDefinition
                {
                    Description = this._resolver.GetMemberInfoComment(p)
                };
            
                if (p.PropertyType == typeof(string) || p.PropertyType.IsValueType)
                {
                    pd.Type = GetSwaggerType(p.PropertyType);
                }
                else if (p.PropertyType.IsArray && p.PropertyType.HasElementType)
                {
                    pd.Type = "array";
                    pd.Items = GetSwaggerItemSchema(p.PropertyType.GetElementType(), definitions, config);
                }
                else if (
                    typeof(System.Collections.ICollection).IsAssignableFrom(p.PropertyType)
                    || typeof(System.Collections.IEnumerable).IsAssignableFrom(p.PropertyType)
                )
                {

                    if (p.PropertyType.IsGenericType)
                    {
                        pd.Type = "array";
                        pd.Items = GetSwaggerItemSchema(p.PropertyType.GenericTypeArguments[0], definitions, config);
                    }
                    else
                    {
                        pd.Type = "array";
                    }
                }
                else
                {
                    //Console.WriteLine(p.PropertyType.Name);
                    pd.Ref = "#/definitions/" + p.PropertyType.Name;
                    CreateSwaggerDefinition(p.PropertyType.Name, p.PropertyType, definitions, config);
                }

                definition.Properties.Add(p.Name.ToCamelCase(), pd);
            });

        }

        private void ProcessParameters(RestfulVerb verb, List<SwaggerApiParameters> pathParameters, ParameterInfo[] getParameters, Dictionary<string, SwaggerDefinition> definitions, SwaggerConfig config)
        {
            var parameter = getParameters[0];
            var properties = parameter.ParameterType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);

            properties.ForEach(p =>
            {
                if (config.IngoreFields.Contains(p.Name))
                {
                    return;
                }

                SwaggerApiParameters apiParameter = new SwaggerApiParameters
                {
                    Name = p.Name.ToCamelCase(),
                    Description = this._resolver.GetMemberInfoComment(p)
                };

                if (p.PropertyType == typeof(string) || p.PropertyType.IsValueType)
                {
                    apiParameter.Type = GetSwaggerType(p.PropertyType);
                    apiParameter.In = verb == RestfulVerb.Any || verb == RestfulVerb.Get ? "query" : "formData";
                }
                else
                {
                    apiParameter.In = verb == RestfulVerb.Any || verb == RestfulVerb.Get ? "query" : "body";
                    apiParameter.Schema = GetSwaggerItemSchema(p.PropertyType, definitions, config);
                }
                pathParameters.Add(apiParameter);
            });

        }

        private SwaggerItemSchema GetSwaggerItemSchema(Type type, Dictionary<string, SwaggerDefinition> definitions, SwaggerConfig config)
        {

            if (type.IsArray && type.HasElementType)
            {
                SwaggerArrayItemSchema arrayItem = new SwaggerArrayItemSchema();
                arrayItem.Items.Add(new SwaggerSingleItemSchema
                {
                    Ref = "#/definitions/" + type.GetElementType().Name
                });
                CreateSwaggerDefinition(type.GetElementType().Name, type.GetElementType(), definitions, config);
                return arrayItem;
            }

            if (type == typeof(string) || type.IsValueType) //字符串或者值类型
            {
                SwaggerSingleItemSchema valueItem = new SwaggerSingleItemSchema { Ref = "#/definitions/" +GetSwaggerType(type) };

                CreateSwaggerDefinition(type.Name, type, definitions, config);

                return valueItem;
            }

            if (
                typeof(System.Collections.ICollection).IsAssignableFrom(type)
                || typeof(System.Collections.IEnumerable).IsAssignableFrom(type)
            )
            {
                SwaggerArrayItemSchema arrayItem = new SwaggerArrayItemSchema();

                if (type.IsGenericType)
                {
                    arrayItem.Items.Add(new SwaggerSingleItemSchema
                    {
                        Ref = "#/definitions/" + type.GenericTypeArguments[0].Name
                    });
                    CreateSwaggerDefinition(type.GenericTypeArguments[0].Name, type.GenericTypeArguments[0], definitions, config);

                }               
                else
                {
                    arrayItem.Items.Add(new SwaggerSingleItemSchema
                    {
                        Ref = "#/definitions/Object"
                    });
                }
                return arrayItem;
            }

            SwaggerSingleItemSchema singleItem = new SwaggerSingleItemSchema { Ref = "#/definitions/" + type.Name };

            CreateSwaggerDefinition(type.Name, type, definitions, config);

            return singleItem;
        }

        private SwaggerMethod CreateSwaggerMethod(RestfulVerb verb)
        {
            SwaggerMethod m;
            switch (verb)
            {
                case RestfulVerb.Get:
                    m = new SwaggerGetMethod();
                    break;
                case RestfulVerb.Post:
                    m = new SwaggerPostMethod();
                    break;
                case RestfulVerb.Put:
                    m = new SwaggerPutMethod();
                    break;
                case RestfulVerb.Delete:
                    m = new SwaggerDeleteMethod();
                    break;
                case RestfulVerb.Patch:
                    m = new SwaggerPatchMethod();
                    break;
                default:
                    m = new SwaggerGetMethod();
                    break;
            }
            return m;
        }
        public string GetSwaggerApiJson()
        {
            return _swaggerJson;
        }

        private string GetSwaggerType(Type type)
        {
            if (type == typeof(string))
            {
                return "string";
            }

            if (type == typeof(int) || type == typeof(long) || type == typeof(short) || type.IsEnum)
            {
                return "integer";
            }
            if (type == typeof(bool))
            {
                return "boolean";
            }

            if (type == typeof(float) || type == typeof(decimal) || type == typeof(long) || type == typeof(short))
            {
                return "number";
            }

            return "string";

        }

    }
}
