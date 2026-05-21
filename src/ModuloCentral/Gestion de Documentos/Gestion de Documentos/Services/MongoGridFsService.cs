using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;

namespace Gestion_de_Documentos.Services
{
    public interface IMongoGridFsService
    {
        Task<string> SubirArchivoAsync(Stream stream, string fileName, string contentType);
        Task<(Stream Stream, string FileName, string ContentType)> DescargarArchivoAsync(string objectId);
    }

    public class MongoGridFsService : IMongoGridFsService
    {
        private readonly IGridFSBucket _gridFSBucket;

        public MongoGridFsService(IConfiguration configuration)
        {
            var mongoUri = configuration["MONGO_URI"] ?? "mongodb://admin:admin@mongodb:27017/?authSource=admin";
            var databaseName = configuration["MONGO_DB_NAME"] ?? "sigd_busqueda";

            var client = new MongoClient(mongoUri);
            var database = client.GetDatabase(databaseName);
            
            _gridFSBucket = new GridFSBucket(database);
        }

        public async Task<string> SubirArchivoAsync(Stream stream, string fileName, string contentType)
        {
            var options = new GridFSUploadOptions
            {
                Metadata = new BsonDocument
                {
                    { "ContentType", contentType }
                }
            };

            var objectId = await _gridFSBucket.UploadFromStreamAsync(fileName, stream, options);
            return $"gridfs:{objectId}";
        }

        public async Task<(Stream Stream, string FileName, string ContentType)> DescargarArchivoAsync(string idString)
        {
            // Remover el prefijo "gridfs:" si existe
            if (idString.StartsWith("gridfs:"))
                idString = idString.Substring(7);

            var objectId = new ObjectId(idString);
            
            // Obtener la información del archivo para recuperar el nombre y tipo
            var filter = Builders<GridFSFileInfo>.Filter.Eq(x => x.Id, objectId);
            var fileInfo = await (await _gridFSBucket.FindAsync(filter)).FirstOrDefaultAsync();
            
            if (fileInfo == null)
                throw new FileNotFoundException("El archivo no se encontró en MongoDB GridFS.");

            var contentType = fileInfo.Metadata != null && fileInfo.Metadata.Contains("ContentType") 
                ? fileInfo.Metadata["ContentType"].AsString 
                : "application/octet-stream";

            var stream = new MemoryStream();
            await _gridFSBucket.DownloadToStreamAsync(objectId, stream);
            stream.Position = 0;

            return (stream, fileInfo.Filename, contentType);
        }
    }
}
