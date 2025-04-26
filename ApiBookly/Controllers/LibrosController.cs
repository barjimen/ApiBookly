using ApiBookly.Repositories;
using BooklyNugget.Models;
using Microsoft.AspNetCore.Mvc;
using ApiBookly.Helper;
using Microsoft.AspNetCore.Authorization;
using ApiBookly.Services;

namespace ApiBookly.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LibrosController : ControllerBase
    {
        private IRepositoryLibros repo;
        private ServiceStorageBlobs service;
        public LibrosController(IRepositoryLibros repo, ServiceStorageBlobs service)
        {
            this.repo = repo;
            this.service = service;
        }

        [HttpGet("[action]")]
        public async Task<ActionResult<Biblioteca>> GetIndex()
        {
            int idUsuario = 0;

            var claim = User.FindFirst("id");
            if (claim != null)
            {
                int.TryParse(claim.Value, out idUsuario);
            }
            var libros = await this.repo.GetLibrosAsync(idUsuario);
            var etiquetas = await this.repo.GetEtiquetas();
            var autores = await this.repo.GetAutoresAsync();
            List<LibroEtiquetas> librosetiquetas = new List<LibroEtiquetas>();
            if (idUsuario != 0)
            {
                librosetiquetas = await this.repo.GetEtiquetasLibroByUsuario(idUsuario);
            }
            foreach (LibrosDTO libro in libros)
            {
                string urlBlob = this.service.GetContainerUrl("imagesbookly");
                libro.ImagenPortada = urlBlob + "/books/" + libro.ImagenPortada;

            }
            var datos = new Biblioteca
            {
                Libros = libros,
                Etiquetas = etiquetas,
                Autores = autores,
                LibroEtiquetas = librosetiquetas
            };

            return datos;
        }

        [HttpGet("{idLibro}")]
        public async Task<ActionResult<LibrosDetalles>> GetDetallesLibro(int idLibro)
        {
            int idUsuario = 0;

            var claim = User.FindFirst("id");
            if (claim != null)
            {
                int.TryParse(claim.Value, out idUsuario);
            }
            Libros libro = await this.repo.FindLibros(idLibro);
            var etiquetas = await this.repo.ObtenerEtiquetasLibro(idLibro);
            List<ReseñaDTO> Reseñas = await this.repo.Reseñas(idLibro);
            string urlBlob = this.service.GetContainerUrl("imagesbookly");
            libro.ImagenPortada = urlBlob + "/books/" + libro.ImagenPortada;

            int listaId = 0;
            if (idUsuario != 0)
            {
                listaId = await this.repo.LibrosListaDetalle(idLibro, idUsuario);
            }
            var detallesLibro = new LibrosDetalles
            {
                Libro = libro,
                Etiquetas = etiquetas,
                Resenas = Reseñas,
                ListaLibro = listaId
            };

            return detallesLibro;
        }

        [HttpGet("[action]")]
        public async Task<ActionResult<GenerosDTO>> GetGeneros()
        {
            int idUsuario = 0;

            var claim = User.FindFirst("id");
            if (claim != null)
            {
                int.TryParse(claim.Value, out idUsuario);
            }
            var etiquetas = await this.repo.GetEtiquetas();
            List<LibrosDTO> libros = await this.repo.GetLibrosAsync(idUsuario);
            var generosConLibros = etiquetas.Select(e => new Generos
            {
                Genero = e,
                Libros = libros.Where(l => l.EtiquetaId == e.Id).ToList()
            }).Where(x => x.Libros.Any()).ToList();


            generosConLibros.Shuffle();

            var generarAleatorios = generosConLibros.Take(2).ToList();

            var VistaGeneros = new GenerosDTO
            {
                GenerosDestacados = generarAleatorios,
                TodosLosGeneros = etiquetas
            };

            return VistaGeneros;
        }

        [HttpGet("[action]/{idGenero}")]
        public async Task<ActionResult<Generos>> GetDetalleGenero(int idGenero)
        {
            Etiquetas genero = await this.repo.FindEtiqueta(idGenero);
            if (genero == null)
            {
                return NotFound();
            }

            // Sacamos los libros
            List<Libros> libros = await this.repo.FiltrarPorEtiquetas(idGenero);
            string urlBlob = this.service.GetContainerUrl("imagesbookly");

            // Creamos la lista de DTOs
            List<LibrosDTO> librosDto = new List<LibrosDTO>();
            foreach (Libros libro in libros)
            {
                librosDto.Add(new LibrosDTO
                {
                    Id = libro.Id,
                    Titulo = libro.Titulo,
                    ImagenPortada = urlBlob + "/books/" + libro.ImagenPortada
                });
            }

            Generos model = new Generos
            {
                Genero = genero,
                Libros = librosDto
            };

            return model;
        }
        
        [HttpGet("[action]")]
        public async Task<ActionResult<List<LibrosBusquedaDTO>>> BuscarLibros(string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                return new List<LibrosBusquedaDTO>();
            }

            var libros = await this.repo.BuscarLibrosAsync(query);

            string urlBlob = this.service.GetContainerUrl("imagesbookly");

            var resultado = libros.Select(l => new LibrosBusquedaDTO
            {
                Id = l.Id,
                Titulo = l.Titulo,
                Autor = !string.IsNullOrEmpty(l.NombreAutor) ? l.NombreAutor : "Autor desconocido",
                Imagen = urlBlob + "/books/" + l.ImagenPortada
            }).ToList();

            return resultado;
        }

        [Authorize]
        [HttpGet("[action]")]
        public async Task<ActionResult<LibrosLeyendoProgreso>> Home()
        {
            int idUsuario = int.Parse(User.FindFirst("id")!.Value);

            List<LibrosLeyendo> libro = await this.repo.LibrosLeyendo(idUsuario);
            List<ProgresoLectura> progresoLectura = new List<ProgresoLectura>();
            string urlBlob = this.service.GetContainerUrl("imagesbookly");


            foreach (var lib in libro)
            {
                var progresosLectura = await this.repo.GetProgresoLectura(idUsuario, lib.Id);
                lib.ImagenPortada = urlBlob + "/books/" + lib.ImagenPortada;
                progresoLectura.Add(progresosLectura);
            }

            var libros = new LibrosLeyendoProgreso
            {
                Leyendos = libro,
                ProgresoLectura = progresoLectura
            };

            return libros;
        }

        [HttpPost("[action]/{idlibro}/{origen}/{destino}")]
        [Authorize]
        public async Task<ActionResult> MoverLibrosEntreListas(int idlibro, int origen, int destino)
        {
            int idUsuario = int.Parse(User.FindFirst("id")!.Value);
            await this.repo.MoverLibrosLista(idUsuario, idlibro, origen, destino);
            if (destino == 1)
                await this.repo.InsertProgreso(idUsuario, idlibro);
            else if (destino != 1)
            {
                int id = (int)await this.repo.FindProgreso(idUsuario, idlibro);
                await this.repo.DeleteProgreso(id, idUsuario);
            }
            return Ok(new { message = "Libro movido correctamente", destino = destino });
        }

        [HttpPut("[action]")]
        [Authorize]
        public async Task<ActionResult> ActualizarReseña(ReseñaDTO res)
        {
            try
            {
                int idUsuario = int.Parse(User.FindFirst("id")!.Value);
                int idreseña = res.Id;
                var reseña = await this.repo.UpdateReseña(res.Id, idUsuario, res.Calificacion, res.Texto);

                if (reseña == null)
                {
                    return BadRequest(new { message = "No se encontró la reseña o no tienes permisos para editarla." });
                }

                return Ok(new { message = "Reseña actualizada correctamente", id = res.IdLibro });
            }
            catch (Exception ex)
            {
                // Log del error
                return StatusCode(500, new { message = ex.Message });
            }

        }


        [HttpPost("[action]")]
        [Authorize]
        public async Task<IActionResult> InsertReseña(ReseñaDTO model)
        {
            int idUsuario = int.Parse(User.FindFirst("id")!.Value);

            await this.repo.InsertReseña(idUsuario, model.IdLibro, model.Calificacion, model.Texto);
            return Ok(new { message = "Reseña insertada correctamente", idLibro = model.IdLibro });

        }

        [HttpPut("[action]")]
        [Authorize]
        public async Task<IActionResult> UpdateProgreso(ProgresoLectura progreso)
        {
            int idUsuario = int.Parse(User.FindFirst("id")!.Value);
            int idprogreso = (int)await this.repo.FindProgreso(idUsuario, progreso.idLibro);
            await this.repo.UpdateProgreso(idprogreso, idUsuario, progreso.Pagina);

            return Ok(new { message = "Update correcto", id = progreso.idLibro });
        }
    }
}

