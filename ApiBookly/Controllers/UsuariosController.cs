using ApiBookly.Helper;
using ApiBookly.Repositories;
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
        //[HttpPost]
        //public async Task<ActionResult> Register(string nombre, string email, string password)
        //{
        //    await this.repo.Register(nombre, email, password);
        //    return RedirectToAction("Index", "Home");
        //}

        //public IActionResult Logout()
        //{
        //    HttpContext.Session.Clear();
        //    return RedirectToAction("Index", "Home");
        //}
    }
}
