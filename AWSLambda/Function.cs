using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.APIGatewayEvents;


//using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.Core;
using System.Text.Json;
using System.Threading.Tasks;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace AWSLambda;

public class Function
{
    private DynamoDBContext _dynamoDBContect;

    public Function()
    {
        _dynamoDBContect = new DynamoDBContext(new AmazonDynamoDBClient());
    }

    /// <summary>
    /// A simple function that takes a string and does a ToUpper
    /// </summary>
    /// <param name="input">The event for the Lambda function handler to process.</param>
    /// <param name="context">The ILambdaContext that provides methods for logging and describing the Lambda environment.</param>
    /// <returns></returns>
    //public string FunctionHandler(string input, ILambdaContext context)

    /// Function handler name should match with assembly name in aws-lambda-tools-defaults.json
    //public async Task<User> FunctionHandler(User input, ILambdaContext context)
    public async Task<APIGatewayHttpApiV2ProxyResponse> FunctionHandler(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
    {
        //var contextConfig = new DynamoDBContextConfig
        //{
        //    DisableFetchingTableMetadata = true
        //};

        //return input.Name.ToUpper();
        //var dynamoDBContect = new DynamoDBContext(new AmazonDynamoDBClient(), contextConfig);
        return request.RequestContext.Http.Method.ToUpper() switch
        {
            "GET" => await HandleGetRequest(request),
            "POST" => await HandlePostRequest(request),
            "DELETE" => await HandleDeleteRequest(request)
        };

    }

    private async Task<APIGatewayHttpApiV2ProxyResponse> HandleGetRequest(APIGatewayHttpApiV2ProxyRequest request)
    {
        request.PathParameters.TryGetValue("userid", out var userIdString);
        if (Guid.TryParse(userIdString, out var userId))
        {
            
            var user = await _dynamoDBContect.LoadAsync<User>(userId);
            if (user != null)
            {
                return new APIGatewayHttpApiV2ProxyResponse()
                {
                    Body = JsonSerializer.Serialize(user),
                    StatusCode = 200
                };
            }
        }
        return BadRequest("Invalid userId in request");
    }
    private async Task<APIGatewayHttpApiV2ProxyResponse> HandlePostRequest(APIGatewayHttpApiV2ProxyRequest request)
    {
        var user = JsonSerializer.Deserialize<User>(request.Body);
        if (user == null)
        {
            return BadRequest("Invalid userId in request");
        }
        await _dynamoDBContect.SaveAsync(user);
        return OkResponse();

    }
    private async Task<APIGatewayHttpApiV2ProxyResponse> HandleDeleteRequest(APIGatewayHttpApiV2ProxyRequest request)
    {
        request.PathParameters.TryGetValue("userid", out var userIdString);
        if (Guid.TryParse(userIdString, out var userId))
        {

             await _dynamoDBContect.DeleteAsync<User>(userId);
             return OkResponse(); 
        }
        return BadRequest("Invalid userId in request");

    }
    private APIGatewayHttpApiV2ProxyResponse OkResponse() =>
        new APIGatewayHttpApiV2ProxyResponse()
        {
            StatusCode = 200            
        };
    
    private APIGatewayHttpApiV2ProxyResponse BadRequest(string message)
    {
        return new APIGatewayHttpApiV2ProxyResponse()
        {
            StatusCode = 404,
            Body = message
        };
    }
}
//Custom class
[DynamoDBTable("User")]
public class User
{
    [DynamoDBHashKey]
    public Guid Id { get; set; }
    [DynamoDBProperty]
    public string Name { get; set; } =string.Empty;
}
