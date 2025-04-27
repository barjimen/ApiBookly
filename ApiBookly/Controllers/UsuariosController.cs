using System.Security.Claims;
using ApiBookly.Helper;
using ApiBookly.Repositories;
using ApiBookly.Services;
using Azure.Storage.Blobs;
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
        private ServiceStorageBlobs service;
        public UsuariosController(IRepositoryLibros repo, IWebHostEnvironment hostingEnvironment, ServiceStorageBlobs service)
        {
            _hostingEnvironment = hostingEnvironment;
            this.repo = repo;
            this.service = service;
        }
        [HttpPost("[action]")]
        public async Task<ActionResult> Register(Register user)
        {
            await this.repo.Register(user);
            return Ok(new { message = "Usuario registrado correctamente" });
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
            string urlBlob = this.service.GetContainerUrl("imagesbookly");

            ProgresoLectura progreso = null;
            if (librosPredefinidos.Count > 0)
            {
                int idLibro = librosPredefinidos.First().IdLibro;
                progreso = await this.repo.GetProgresoLectura(idUser, idLibro);
            }
            usuario.ImagenPerfil = urlBlob + "/Users/" + usuario.ImagenPerfil;
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
            int idUser = int.Parse(User.FindFirst("id")!.Value);
            await this.repo.UpdateObjetivo(objetivos.idObjetivo, idUser, objetivos.ProgresoActual);
            return Ok();
        }

        [HttpPut("[action]")]
        [Authorize]
        public async Task<ActionResult<Usuarios>> UpdateUsuario(Usuarios usuario)
        {
            await this.repo.UpdateUsuarios(usuario);
            return Ok();
        }

        [HttpGet("[action]")]
        [Authorize]
        public async Task<ActionResult<Usuarios>> GetUsuario(int idUsuario)
        {
            var usuario = await this.repo.GetUsuario(idUsuario);
            return usuario;
        }

    }
}
