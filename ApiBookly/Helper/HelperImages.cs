using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Hosting.Server;

namespace ApiBookly.Helper
{
    public enum Folders { Images, Users }
    public class HelperImages
    {
            private IWebHostEnvironment hostEnvironment;
            private IHttpContextAccessor httpContextAccessor;
            private IServer server;

            public HelperImages(IWebHostEnvironment hostEnvironment, IHttpContextAccessor httpContextAccessor, IServer server)
            {
                this.hostEnvironment = hostEnvironment;
                this.httpContextAccessor = httpContextAccessor;
                this.server = server;
            }

            private string GetFolderName(Folders folder)
            {
                return folder switch
                {
                    Folders.Images => "images",
                    Folders.Users => "users",
                    _ => "images"
                };
            }

            public string MapPath(string fileName, Folders folder)
            {
                string carpeta = GetFolderName(folder);
                string rootPath = this.hostEnvironment.WebRootPath;
                string fullPath = Path.Combine(rootPath, carpeta);

                // Crear carpeta si no existe
                if (!Directory.Exists(fullPath))
                {
                    Directory.CreateDirectory(fullPath);
                }

                return Path.Combine(fullPath, fileName);
            }

            public string MapUrlPath(string fileName, Folders folder)
            {
                string carpeta = GetFolderName(folder);
                var request = httpContextAccessor.HttpContext.Request;
                var baseUrl = $"{request.Scheme}://{request.Host}{request.PathBase}";
                return $"{baseUrl}/{carpeta}/{fileName}";
            }

            public string MapUrlPathServer(string fileName, Folders folder)
            {
                string carpeta = GetFolderName(folder);
                var addresses = this.server.Features.Get<IServerAddressesFeature>().Addresses;
                string serverUrl = addresses.FirstOrDefault();
                return $"{serverUrl}/{carpeta}/{fileName}";
            }
    }
}
