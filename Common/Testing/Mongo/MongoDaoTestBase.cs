namespace Common.Testing.Mongo;

using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Moq;

public abstract class MongoDaoTestsBase<TModel, TDao>
    where TModel : class, new()
    where TDao : class
{
    protected Mock<IMongoCollection<TModel>> MockCollection { get; }
    protected Mock<IMongoIndexManager<TModel>> MockIndexes { get; }
    protected Mock<IMongoDatabase> MockDatabase { get; }
    protected Mock<ILogger<TDao>> MockLogger { get; }
    protected TDao Dao { get; }

    protected MongoDaoTestsBase(
        string collectionName,
        Func<ILogger<TDao>, IMongoDatabase, TDao> daoFactory)
    {
        MockCollection = new Mock<IMongoCollection<TModel>>();

        // Needed so the driver doesn't throw on CollectionNamespace / DocumentSerializer
        MockCollection.SetupGet(c => c.CollectionNamespace)
            .Returns(new CollectionNamespace(new DatabaseNamespace("testDb"), collectionName));
        MockCollection.SetupGet(c => c.DocumentSerializer)
            .Returns(MongoDB.Bson.Serialization.BsonSerializer.SerializerRegistry.GetSerializer<TModel>());
        MockCollection.SetupGet(c => c.Settings)
            .Returns(new MongoCollectionSettings());

        // Mock Indexes
        MockIndexes = new Mock<IMongoIndexManager<TModel>>();
        MockIndexes
            .Setup(i => i.CreateOne(
                It.IsAny<CreateIndexModel<TModel>>(),
                It.IsAny<CreateOneIndexOptions>(),
                It.IsAny<CancellationToken>()))
            .Returns("ok");

        MockCollection.SetupGet(c => c.Indexes).Returns(MockIndexes.Object);

        MockDatabase = new Mock<IMongoDatabase>();
        MockDatabase
            .Setup(d => d.GetCollection<TModel>(collectionName, null))
            .Returns(MockCollection.Object);

        MockLogger = new Mock<ILogger<TDao>>();

        Dao = daoFactory(MockLogger.Object, MockDatabase.Object);
    }

    protected static Mock<IAsyncCursor<TModel>> CreateMockCursor(List<TModel> models)
    {
        var mockCursor = new Mock<IAsyncCursor<TModel>>();

        mockCursor.Setup(_ => _.Current).Returns(models);
        mockCursor
            .SetupSequence(_ => _.MoveNext(It.IsAny<CancellationToken>()))
            .Returns(true)
            .Returns(false);
        mockCursor
            .SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true)
            .ReturnsAsync(false);

        return mockCursor;
    }
}
