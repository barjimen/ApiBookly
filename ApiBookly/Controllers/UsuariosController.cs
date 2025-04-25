using ApiBookly.Helper;
using ApiBookly.Repositories;
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
        private HelperImages helperImages;
        public UsuariosController(IRepositoryLibros repo, IWebHostEnvironment hostingEnvironment, HelperImages helperImages)
        {
            _hostingEnvironment = hostingEnvironment;
            this.repo = repo;
            this.helperImages = helperImages;
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

    }
}
