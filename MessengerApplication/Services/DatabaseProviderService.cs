using MessengerApplication.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace MessengerApplication.Services;

public class DatabaseProviderService
{
    private readonly IMongoDatabase _mongoDatabase;
    
    public DatabaseProviderService(IOptions<MessengerApplicationDatabaseSettings> messengerDatabaseSettings)
    {
        var mongoClient = new MongoClient(messengerDatabaseSettings.Value.ConnectionString);
        _mongoDatabase = mongoClient.GetDatabase(messengerDatabaseSettings.Value.DatabaseName);
    }

    public IMongoDatabase GetAccess() => _mongoDatabase;
}