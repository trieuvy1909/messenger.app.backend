using MessengerApplication.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace MessengerApplication.Services;

public class DatabaseProviderService
{
    private readonly IMongoDatabase _mongoDatabase;
    
    public DatabaseProviderService(IOptions<MessengerApplicationDatabaseSettings> messengerDatabaseSettings)
    {
        var connectionString = messengerDatabaseSettings.Value.ConnectionString
            .Replace("${CONNECTION_STRING}", Environment.GetEnvironmentVariable("CONNECTION_STRING"));
        var databaseName = messengerDatabaseSettings.Value.DatabaseName.Replace("${DB_NAME}", Environment.GetEnvironmentVariable("DB_NAME"));
        var mongoClient = new MongoClient(connectionString);
        _mongoDatabase = mongoClient.GetDatabase(databaseName);
    }

    public IMongoDatabase GetAccess() => _mongoDatabase;
}