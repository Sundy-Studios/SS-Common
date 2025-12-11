using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Moq;

namespace Common.Testing.Mongo;

public abstract class MongoDaoTestsBase<TModel, TDao>
    where TModel : class, new()
    where TDao : class
{
    protected readonly Mock<IMongoCollection<TModel>> _mockCollection;
    protected readonly Mock<IMongoIndexManager<TModel>> _mockIndexes;
    protected readonly Mock<IMongoDatabase> _mockDatabase;
    protected readonly Mock<ILogger<TDao>> _mockLogger;
    protected readonly TDao _dao;

    protected MongoDaoTestsBase(string collectionName, Func<ILogger<TDao>, IMongoDatabase, TDao> daoFactory)
    {
        _mockCollection = new Mock<IMongoCollection<TModel>>();

        // Needed so the driver doesn't throw on CollectionNamespace / DocumentSerializer
        _mockCollection.SetupGet(c => c.CollectionNamespace)
            .Returns(new CollectionNamespace(new DatabaseNamespace("testDb"), collectionName));
        _mockCollection.SetupGet(c => c.DocumentSerializer)
            .Returns(MongoDB.Bson.Serialization.BsonSerializer.SerializerRegistry.GetSerializer<TModel>());
        _mockCollection.SetupGet(c => c.Settings)
            .Returns(new MongoCollectionSettings());

        // Mock Indexes
        _mockIndexes = new Mock<IMongoIndexManager<TModel>>();
        _mockIndexes
            .Setup(i => i.CreateOne(It.IsAny<CreateIndexModel<TModel>>(), It.IsAny<CreateOneIndexOptions>(), It.IsAny<CancellationToken>()))
            .Returns("ok");
        _mockCollection.SetupGet(c => c.Indexes).Returns(_mockIndexes.Object);

        _mockDatabase = new Mock<IMongoDatabase>();
        _mockDatabase
            .Setup(d => d.GetCollection<TModel>(collectionName, null))
            .Returns(_mockCollection.Object);

        _mockLogger = new Mock<ILogger<TDao>>();

        // Create DAO instance via factory
        _dao = daoFactory(_mockLogger.Object, _mockDatabase.Object);
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
