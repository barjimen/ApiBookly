using ApiBookly.Helper;
using ApiBookly.Repositories;
using BooklyNugget.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApiBookly.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly IWebHostEnvironment _hostingEnvironment;
        private IRepositoryLibros repo;
        public UsuariosController(IRepositoryLibros repo, IWebHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
            this.repo = repo;
        }
        [HttpPost("[action]")]
        public async Task<ActionResult> Register(string nombre, string email, string password)
        {
            await this.repo.Register(nombre, email, password);
            return Ok(new { message = "Usuario registrado correctamente" });
        }

        [HttpPost("[action]")]
        [Authorize]
        public IActionResult Logout()
        {
            // No hay nada que hacer en el servidor con JWT
            return Ok(new { message = "Logout exitoso. Borra el token en el cliente." });
        }

        [HttpGet("[action]")]
        [Authorize]
        public async Task<ActionResult<HomeUsuario>> Perfil()
        {
            var idClaim = User.FindFirst("id");
            if (idClaim == null)
            {
                return Unauthorized("El token no contiene el id de usuario.");
            }

            int idUser;
            if (!int.TryParse(idClaim.Value, out idUser))
            {
                return BadRequest("El ID en el token no es válido.");
            }
            var usuario = await this.repo.GetUsuario(idUser);
            var CountLibros = await this.repo.ObtenerCountListas(idUser);
            var librosPredefinidos = await this.repo.LibrosEnPredefinidos(idUser);
            var objetivos = await this.repo.ObjetivosUsuarios(idUser);
            ProgresoLectura progreso = null;
            if (librosPredefinidos.Count > 0)
            {
                int idLibro = librosPredefinidos.First().IdLibro;
                progreso = await this.repo.GetProgresoLectura(idUser, idLibro);
            }

            var homeUsuario = new HomeUsuario
            {
                Usuarios = usuario,
                CountLibrosPred = CountLibros,
                LibrosListasPred = librosPredefinidos,
                ObjetivosUsuarios = objetivos,
                ProgresoLectura = progreso
            };

            return homeUsuario;
        }

        [HttpGet("[action]")]
        [Authorize]
        public async Task<ActionResult<MisLibros>> MisLibros()
        {
            int idUser = int.Parse(User.FindFirst("id")!.Value);
            var CountLibros = await this.repo.ObtenerCountListas(idUser);


            var MisLibros = new MisLibros
            {
                IdUsuario = idUser,
                CountLibrosPred = CountLibros
            };

            return MisLibros;
        }

        //[HttpGet("[action]")]
        //[Authorize]
        //public async Task<ActionResult> FiltrarMisLibros(int idUsuario, int idLista)
        //{
        //    if (idLista == 0)
        //    {
        //        var libros = await this.repo.LibrosEnPredefinidos(idUsuario);
        //        return PartialView("_LibrosPartial", libros);
        //    }
        //    else
        //    {
        //        var libros = await this.repo.FindLibrosEnPredefinidos(idUsuario, idLista);
        //        return PartialView("_LibrosPartial", libros);
        //    }
        //}

        [HttpGet("[action]")]
        [Authorize]
        public async Task<ActionResult<List<ObjetivosUsuarios>>> MisObjetivos()
        {
            int idUser = int.Parse(User.FindFirst("id")!.Value);

            var objetivos = await this.repo.ObjetivosUsuarios(idUser);
            return objetivos;
        }

        [HttpPost("[action]")]
        [Authorize]
        public async Task<ActionResult> InsertObjetivo(ObjetivosUsuarios objetivos)
        {
            int idUser = int.Parse(User.FindFirst("id")!.Value);

            await this.repo.InsertObjetivo(idUser, objetivos.NombreObjetivo, objetivos.Fin, objetivos.TipoObjetivo, objetivos.Meta);
            return Ok();
        }

        [HttpDelete("[action]/{idObjetivo}")]
        [Authorize]
        public async Task<IActionResult> DeleteObjetivo(int idObjetivo)
        {
            int idUser = int.Parse(User.FindFirst("id")!.Value);
            await this.repo.DeleteObjetivo(idObjetivo);
            return Ok();
        }

        [HttpPut("[action]")]
        [Authorize]
        public async Task<IActionResult> UpdateProgreso(ObjetivosUsuarios objetivos)
        {
            int idusuario = (int)HttpContext.Session.GetInt32("id");

            await this.repo.UpdateObjetivo(objetivos.idObjetivo, idusuario, objetivos.ProgresoActual);
            return Ok();
        }

        //[HttpPut("[action]")]
        //[Authorize]
        //public async Task<ActionResult<Usuarios>> UpdateUsuario()
        //{
        //    int idUser = int.Parse(User.FindFirst("id")!.Value);
        //    var usuario = await this.repo.GetUsuario(idUser);
        //    return usuario;
        //}

        [HttpPut("[action]")]
        [Authorize]
        public async Task<ActionResult<Usuarios>> UpdateUsuario(Usuarios usuario, IFormFile ProfileImageFile)
        {
            await this.repo.UpdateUsuarios(usuario);
            return usuario;
        }


        //[HttpPost]
        //[Route("[action]")]
        //[Authorize]
        //public async Task<IActionResult> SubirFichero(IFormFile fichero)
        //{
        //    int idusuario = (int)HttpContext.Session.GetInt32("id");

        //    if (fichero == null || fichero.Length == 0)
        //    {
        //        return BadRequest("No se envió un archivo.");
        //    }

        //    string[] permittedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
        //    string extension = Path.GetExtension(fichero.FileName).ToLowerInvariant();

        //    if (!permittedExtensions.Contains(extension))
        //    {
        //        return BadRequest("Extensión de archivo no permitida.");
        //    }

        //    if (fichero.Length > 2 * 1024 * 1024) // 2 MB
        //    {
        //        return BadRequest("El archivo excede el tamaño máximo permitido.");
        //    }

        //    // Generar un nombre único para el archivo
        //    string fileName = $"usuario_{idusuario}{extension}";

        //    // Ruta física donde guardar el archivo
        //    string path = this.helperImages.MapPath(fileName, Folders.Users);

        //    // Asegurarse de que la carpeta existe
        //    string directory = Path.GetDirectoryName(path);
        //    if (!Directory.Exists(directory))
        //    {
        //        Directory.CreateDirectory(directory);
        //    }

        //    // Guardar el archivo
        //    using (var stream = new FileStream(path, FileMode.Create))
        //    {
        //        await fichero.CopyToAsync(stream);
        //    }

        //    // Guardar solo el nombre del archivo en la base de datos
        //    await this.repo.UpdateFotoUsuario(idusuario, fileName);
        //    var perfil = await this.repo.GetUsuario(idusuario);

        //    return RedirectToAction("Perfil", perfil);
        //}

    }
}
