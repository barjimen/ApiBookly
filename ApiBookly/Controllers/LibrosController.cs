using ApiBookly.Repositories;
using BooklyNugget.Models;
using Microsoft.AspNetCore.Mvc;
using ApiBookly.Helper;
using Microsoft.AspNetCore.Authorization;

namespace ApiBookly.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LibrosController : ControllerBase
    {
        private IRepositoryLibros repo;
        public LibrosController(IRepositoryLibros repo)
        {
            this.repo = repo;
        }
        [HttpGet("[action]")]
        public async Task<ActionResult<Biblioteca>> GetIndex()
        {
            int idUsuario = int.Parse(User.FindFirst("id")!.Value);
            var libros = await this.repo.GetLibrosAsync(idUsuario);
            var etiquetas = await this.repo.GetEtiquetas();
            var autores = await this.repo.GetAutoresAsync();
            List<LibroEtiquetas> librosetiquetas = new List<LibroEtiquetas>();
            if (idUsuario != 0)
            {
                librosetiquetas = await this.repo.GetEtiquetasLibroByUsuario(idUsuario);
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
            int idUsuario = int.Parse(User.FindFirst("id")!.Value);
            Libros libro = await this.repo.FindLibros(idLibro);
            var etiquetas = await this.repo.ObtenerEtiquetasLibro(idLibro);
            List<Resenas> Reseñas = await this.repo.Reseñas(idLibro);

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
            int idUsuario = int.Parse(User.FindFirst("id")!.Value);
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
        public async Task<ActionResult<List<Libros>>> GetDetalleGenero(int idGenero)
        {
            List<Libros> libros = await this.repo.FiltrarPorEtiquetas(idGenero);
            return libros;
        }
        
        [HttpGet("[action]")]
        public async Task<ActionResult<Object>> BuscarLibros(string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                return Ok(new { results = new List<object>() });
            }

            var libros = await this.repo.BuscarLibrosAsync(query);

            var resultado = libros.Select(l => new
            {
                id = l.Id,
                titulo = l.Titulo,
                autor = l.NombreAutor != null ? l.NombreAutor : "Autor desconocido"
            }).ToList();

            return Ok(new { results = resultado });
        }

        [Authorize]
        [HttpGet("[action]")]
        public async Task<ActionResult<LibrosLeyendoProgreso>> Home()
        {
            int idUsuario = int.Parse(User.FindFirst("id")!.Value);

            List<LibrosLeyendo> libro = await this.repo.LibrosLeyendo(idUsuario);
            List<ProgresoLectura> progresoLectura = new List<ProgresoLectura>();

            foreach (var lib in libro)
            {
                var progresosLectura = await this.repo.GetProgresoLectura(idUsuario, lib.Id);
                progresoLectura.Add(progresosLectura);
            }

            var libros = new LibrosLeyendoProgreso
            {
                Leyendos = libro,
                ProgresoLectura = progresoLectura
            };

            return libros;
        }

        [HttpPost("[action]")]
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

        [HttpPost("[action]")]
        [Authorize]
        public async Task<ActionResult> ActualizarReseña(Resenas model)
        {
            try
            {
                int idUsuario = int.Parse(User.FindFirst("id")!.Value);
                int idreseña = model.Id;
                var reseña = await this.repo.UpdateReseña(model.Id, idUsuario, model.calificacion, model.texto);

                if (reseña == null)
                {
                    return BadRequest(new { message = "No se encontró la reseña o no tienes permisos para editarla." });
                }

                return RedirectToAction("Detalles", new { id = model.idLibro });
            }
            catch (Exception ex)
            {
                // Log del error
                return StatusCode(500, new { message = ex.Message });
            }

        }


        [HttpPost("[action]")]
        [Authorize]
        public async Task<IActionResult> InsertReseña(Resenas model)
        {
            int idUsuario = int.Parse(User.FindFirst("id")!.Value);

            await this.repo.InsertReseña(idUsuario, model.idLibro, model.calificacion, model.texto);
            return RedirectToAction("Detalles", new { id = model.idLibro });

        }

        [HttpPost("[action]")]
        [Authorize]
        public async Task<IActionResult> UpdateProgreso(ProgresoLectura progreso)
        {
            int idUsuario = int.Parse(User.FindFirst("id")!.Value);
            int idprogreso = (int)await this.repo.FindProgreso(idUsuario, progreso.idLibro);
            await this.repo.UpdateProgreso(idprogreso, idUsuario, progreso.Pagina);

            return RedirectToAction("Home", new { id = progreso.idLibro });
        }
    }
}

