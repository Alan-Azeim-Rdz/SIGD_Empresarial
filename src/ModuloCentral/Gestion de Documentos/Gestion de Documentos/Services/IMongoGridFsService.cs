using System.IO;
using System.Threading.Tasks;

namespace Gestion_de_Documentos.Services
{
    public interface IMongoGridFsService
    {
        Task<string> SubirArchivoAsync(Stream archivoStream, string nombreArchivo, string contentType);
        Task<(Stream Stream, string NombreArchivo, string ContentType)> DescargarArchivoAsync(string objectIdStr);
    }
}
