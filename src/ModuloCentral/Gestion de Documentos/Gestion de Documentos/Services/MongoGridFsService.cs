using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
<<<<<<< HEAD
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Gestion_de_Documentos.Services
{
    public class MongoGridFsService : IMongoGridFsService
    {
        private readonly IGridFSBucket _gridFS;

        public MongoGridFsService(IConfiguration configuration)
        {
            var mongoUri = configuration["MONGO_URI"] ?? configuration.GetConnectionString("MongoConnection") ?? "mongodb://root:rootpassword@mongodb:27017/?authSource=admin";
            var client = new MongoClient(mongoUri);
            var database = client.GetDatabase("sigd_archivos");
            _gridFS = new GridFSBucket(database);
        }

        public async Task<string> SubirArchivoAsync(Stream archivoStream, string nombreArchivo, string contentType)
=======

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
>>>>>>> development
        {
            var options = new GridFSUploadOptions
            {
                Metadata = new BsonDocument
                {
<<<<<<< HEAD
                    { "contentType", contentType }
                }
            };

            var id = await _gridFS.UploadFromStreamAsync(nombreArchivo, archivoStream, options);
            return id.ToString();
        }

        public async Task<(Stream Stream, string NombreArchivo, string ContentType)> DescargarArchivoAsync(string objectIdStr)
        {
            var id = new ObjectId(objectIdStr);
            var filter = Builders<GridFSFileInfo>.Filter.Eq(x => x.Id, id);
            var fileInfo = await _gridFS.FindAsync(filter).Result.FirstOrDefaultAsync();

            if (fileInfo == null)
            {
                throw new FileNotFoundException("Archivo no encontrado en GridFS");
            }

            var stream = new MemoryStream();
            await _gridFS.DownloadToStreamAsync(id, stream);
            stream.Position = 0;

            var contentType = fileInfo.Metadata != null && fileInfo.Metadata.Contains("contentType")
                ? fileInfo.Metadata["contentType"].AsString
                : "application/octet-stream";

=======
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

>>>>>>> development
            return (stream, fileInfo.Filename, contentType);
        }
    }
}
