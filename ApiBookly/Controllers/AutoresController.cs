using ApiBookly.Repositories;
using BooklyNugget.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApiBookly.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AutoresController : ControllerBase
    {
        private IRepositoryLibros repo;
        public AutoresController(IRepositoryLibros repo)
        {
            this.repo = repo;
        }
        [HttpGet]
        public async Task<ActionResult<List<Autores>>> GetAutores()
        {
            List<Autores> autores =
                await this.repo.GetAutoresAsync();
            return autores;
        }
        [HttpGet("{idAutor}")]
        public async Task<ActionResult<DetallesAutor>> GetAutoresDetails(int idAutor)
        {
            DetallesAutor detalles = await this.repo.FindAutorAsync(idAutor);
            return detalles;
        }
    }
}
