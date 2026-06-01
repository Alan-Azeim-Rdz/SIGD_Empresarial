using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
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
        {
            var options = new GridFSUploadOptions
            {
                Metadata = new BsonDocument
                {
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

            return (stream, fileInfo.Filename, contentType);
        }
    }
}
