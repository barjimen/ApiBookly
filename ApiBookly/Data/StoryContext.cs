using Microsoft.EntityFrameworkCore;

using BooklyNugget.Models;

namespace ApiBookly.Context
{
    public class StoryContext : DbContext
    {
        public StoryContext(DbContextOptions<StoryContext> options) : base(options) { }

        public DbSet<Libros> Libros { get; set; }
        public DbSet<Autores> Autores { get; set; }
        public DbSet<Usuarios> Usuarios { get; set; }
        public DbSet<LibrosLeyendo> LibrosLeyendo { get; set; }
        public DbSet<ObjetivosUsuarios> ObjetivosUsuarios { get; set; }
        public DbSet<LibrosListasPredefinidas> LibrosListasPredefinidas { get; set; }
        public DbSet<CountLibrosListasPredefinidas> CountLibrosListasPredefinidas{ get; set; }
        public DbSet<Etiquetas> Etiquetas { get; set; }
        public DbSet<LibroEtiquetas> LibrosEtiquetas { get; set; }
        public DbSet<usuario_lista_predefinida_libro> IdUsuariosListasPredefinidas { get; set; }
        public DbSet<Resenas> Reseñas { get; set; }
        public DbSet<LibrosAutor> LibrosAutor { get; set; }
        public DbSet<ProgresoLectura> ProgresoLecturas { get; set; }
    }
}
