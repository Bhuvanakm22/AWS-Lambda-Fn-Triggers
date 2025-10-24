using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.Annotations;
using Amazon.Lambda.Annotations.APIGateway;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Runtime.Internal;
using System.Text.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace AWSLambda_annotation;

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
    [LambdaFunction]
    [HttpApi(LambdaHttpMethod.Get, "/users/{userId}")]
    public async Task<User> FunctionHandler(string userId, ILambdaContext context)
    {
        Guid.TryParse(userId, out var id);
        return await _dynamoDBContect.LoadAsync<User>(id);
    }

    [LambdaFunction]
    [HttpApi(LambdaHttpMethod.Post, "/users")]
    public async Task PostFunctionHandler([FromBody] User user, ILambdaContext context)
    {
        await _dynamoDBContect.SaveAsync(user);
    }
}
public class User
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

