using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using BooklyNugget.Models;

namespace ApiBookly.Repositories
{
    public interface IRepositoryLibros
    {
        Task<List<LibrosDTO>> GetLibrosAsync(int? usuarioId);
        Task<Libros> FindLibros(int id);
        Task AddAsync(Libros libros);
        Task DeleteAsync(int id);
        Task UpdateAsync(Libros libros);
        Task<List<Libros>> BuscarLibrosAsync(string query);
        Task<List<LibrosLeyendo>> LibrosLeyendo(int idUsuario);
        Task<List<Etiquetas>> ObtenerEtiquetasLibro(int libroId);
        Task<List<LibroEtiquetas>> GetEtiquetasLibroByUsuario (int? idUsuario);
        Task MoverLibrosLista(int idUsuario, int idLibro, int origen, int destino);
        Task<List<Autores>> GetAutoresAsync();
        Task<DetallesAutor> FindAutorAsync(int idAutor);
        Task<Usuarios> Login(string email, string password);
        Task Register(string nombre, string email, string password);
        Task<Usuarios> GetUsuario(int id);
        Task<List<CountLibrosListasPredefinidas>> ObtenerCountListas(int idUsuario);
        Task<List<LibrosListasPredefinidas>> LibrosEnPredefinidos(int idUsuario);
        Task<List<ObjetivosUsuarios>> ObjetivosUsuarios(int idUsuario);
        Task<List<LibrosListasPredefinidas>> FindLibrosEnPredefinidos(int idUsuario, int idlista);
        Task<List<Resenas>> Reseñas(int idUsuario);
        Task<Resenas> UpdateReseña(int idReseña, int idUsuario, int nuevaCalificacion, string nuevoTexto);
        Task InsertReseña(int idUsuario, int idLibro, int calificacion, string texto);
        Task<int> LibrosListaDetalle(int idLibro, int? idUsuario);

        Task InsertObjetivo(int idUsuario, string titulo, DateTime fin, string tipo, int meta);
        Task <ProgresoLectura> GetProgresoLectura(int idUsuario, int idLibro);
        Task<List<Etiquetas>> GetEtiquetas();
        Task<ObjetivosUsuarios> UpdateObjetivo(int idObjetivo, int idUsuario, int progreso);
        Task DeleteObjetivo(int idObjetivo);

        Task InsertProgreso(int UsuarioId, int LibroId);
        Task DeleteProgreso(int idProgreso, int idUsuario);
        Task<int?> FindProgreso(int idUsuario, int idLibro);
        Task<ProgresoLectura> UpdateProgreso(int idProgreso, int idUsuario, int pagina);
        Task<List<Libros>> FiltrarPorEtiquetas(int idEtiqueta);
        Task<Usuarios> UpdateUsuarios(Usuarios usuarios);

        Task UpdateLibro(Libros libro);

        Task<string> GetFotoUsuario(int idUsuario);
        Task<string> UpdateFotoUsuario(int idUsuario, string foto);
    }
}
