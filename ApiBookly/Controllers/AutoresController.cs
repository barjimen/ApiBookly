using ApiBookly.Repositories;
using BooklyNugget.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SegundoExamenAzure.Services;

namespace ApiBookly.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AutoresController : ControllerBase
    {
        private IRepositoryLibros repo;
        private ServiceStorageBlobs service;
        public AutoresController(IRepositoryLibros repo, ServiceStorageBlobs service)
        {
            this.repo = repo;
            this.service = service;
        }
        [HttpGet]
        public async Task<ActionResult<List<Autores>>> GetAutores()
        {
            List<Autores> autores =
                await this.repo.GetAutoresAsync();
            foreach (Autores autor in autores)
            {
                string urlBlob = this.service.GetContainerUrl("imagesbookly");
                autor.ImagenPerfil = urlBlob + "/autores/" + autor.ImagenPerfil;

            }
            return autores;
        }
        [HttpGet("{idAutor}")]
        public async Task<ActionResult<DetallesAutor>> GetAutoresDetails(int idAutor)
        {
            DetallesAutor detalles = await this.repo.FindAutorAsync(idAutor);

            string urlBlob = this.service.GetContainerUrl("imagesbookly");
            detalles.Autores.ImagenPerfil = urlBlob + "/autores/" + detalles.Autores.ImagenPerfil;
            return detalles;
        }
    }
}
