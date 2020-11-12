using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Tomato.Gateway.Swagger.Generator
{
    [DataContract]
    public class SwaggerMethod
    {
        [DataMember(Name = "tags")]
        public List<string> Tags { get; set; }

        [DataMember(Name = "summary")]
        public  string Summary { get; set; }

        [DataMember(Name = "description")]
        public  string Description { get; set; }


        [DataMember(Name = "operationId")]
        public  string OperationId { get; set; }

        [DataMember(Name = "consumes")]
        public  List<string> Consumes { get; set; }

        [DataMember(Name = "produces")]
        public  List<string> Produces { get; set; } = new List<string> { "application/json"};


        [DataMember(Name = "parameters")]
        public  List<SwaggerApiParameters> Parameters { get; set; }

        [DataMember(Name = "responses")]
        public Dictionary<string,SwaggerApiResponse> Responses { get; set; }

        [DataMember(Name = "version")]
        public string Version { get; set; }

    }


    [DataContract]
    public class SwaggerGetMethod:SwaggerMethod
    {


    }

    [DataContract]
    public class SwaggerPostMethod:SwaggerMethod
    {

    }
    [DataContract]
    public class SwaggerPutMethod:SwaggerMethod
    {


    }
    [DataContract]
    public class SwaggerDeleteMethod:SwaggerMethod
    {


    }
    [DataContract]
    public class SwaggerPatchMethod:SwaggerMethod
    {


    }

}
