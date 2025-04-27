using ApiBookly.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using BooklyNugget.Models;
using ApiBookly.Helper;
using Azure.Storage.Blobs;
using ApiBookly.Services;

namespace ApiBookly.Repositories
{
    public class RepositoryLibros : IRepositoryLibros
    {
        private StoryContext context;
        private ServiceStorageBlobs service;

        public RepositoryLibros(StoryContext context, ServiceStorageBlobs service)
        {
            this.context = context;
            this.service = service;
        }
        public Task AddAsync(Libros libros)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<Libros> FindLibros(int id)
        {
            var consulta = from datos in context.Libros
                           where datos.Id == id
                           select datos;
            return consulta.FirstOrDefault();
        }
        public async Task<List<Etiquetas>> ObtenerEtiquetasLibro(int libroId)
        {
            var etiquetas = await this.context.LibrosEtiquetas
                .Where(le => le.LibroId == libroId)
                .Include(le => le.Etiqueta)
                .Select(le => le.Etiqueta)
                .ToListAsync();
            return etiquetas;
        }

        public async Task<List<LibrosDTO>> GetLibrosAsync(int? usuarioId)
        {
            var consulta = from l in context.Libros
                           join ul in context.IdUsuariosListasPredefinidas
                           on new { LibroId = l.Id, UsuarioId = usuarioId ?? 0 }
                           equals new { ul.LibroId, ul.UsuarioId } into listaJoin
                           from ul in listaJoin.DefaultIfEmpty()
                           select new LibrosDTO
                           {
                               Id = l.Id,
                               Titulo = l.Titulo,
                               Saga = l.Saga,
                               PosicionSaga = l.PosicionSaga,
                               FechaPublicacion = l.FechaPublicacion,
                               NumeroPaginas = l.NumeroPaginas,
                               Sinopsis = l.Sinopsis,
                               ImagenPortada = l.ImagenPortada,
                               CalificacionPromedio = l.CalificacionPromedio,
                               AutorId = l.AutorId,
                               NombreAutor = l.NombreAutor,
                               idLista = ul != null ? ul.ListaPredefinidaId : 0,
                               EtiquetaId = l.EtiquetaId
                           };
            return consulta
                   .AsNoTracking()
                   .AsEnumerable()
                   .DistinctBy(l => l.Id)
                   .ToList();
        }


        public Task UpdateAsync(Libros libros)
        {
            throw new NotImplementedException();
        }


        public async Task<List<Libros>> BuscarLibrosAsync(string query)
        {
            return await this.context.Libros
                .Where(l => l.Titulo.Contains(query) || l.NombreAutor.Contains(query))
                .Take(5)
                .ToListAsync();
        }

        public async Task<List<LibrosLeyendo>> LibrosLeyendo(int idUsuario)
        {
            var consulta = await this.context.LibrosLeyendo
                             .Where(datos => datos.IdUsuario == idUsuario)
                             .AsNoTracking()
                             .ToListAsync();
            return consulta;
        }


        public async Task MoverLibrosLista(int idUsuario, int idLibro, int origen, int destino)
        {
            string sql = "EXEC sp_MoverLibroEntreListas @UsuarioID, @LibroID, @ListaOrigenID, @ListaDestinoID";

            SqlParameter pamidUsuario = new SqlParameter("@UsuarioID", idUsuario);
            SqlParameter pamidLibro = new SqlParameter("@LibroID", idLibro);
            SqlParameter pamOrigen = new SqlParameter("@ListaOrigenID", origen);
            SqlParameter pamDestino = new SqlParameter("@ListaDestinoID", destino);

            await this.context.Database.ExecuteSqlRawAsync(sql, pamidUsuario, pamidLibro, pamOrigen, pamDestino);
        }

        public async Task<List<Autores>> GetAutoresAsync()
        {
            var consulta = from datos in context.Autores
                           select datos;
            return consulta.ToList();
        }

        public async Task<Usuarios> Login(string email, string password)
        {
            var consulta = from datos in context.Usuarios
                           where datos.email == email
                           select datos;
            Usuarios usuario = await consulta.FirstOrDefaultAsync();
            if (usuario == null)
            {
                return null;
            }
            else
            {

                string salt = usuario.Salt;
                byte[] pass_temp = HelperCryptography.EncryptPassword(password, salt);
                byte[] pass_encrypt = usuario.Password_hash;
                bool resultado = HelperCryptography.CompararArrays(pass_temp, pass_encrypt);
                if (resultado == true)
                {
                    return usuario;

                }
                else
                {
                    return null;
                }
            }
        }

        public async Task Register(Register user)
        {
            Usuarios usuario = new Usuarios();
            int nuevoId = (context.Usuarios.Max(u => (int?)u.Id) ?? 0) + 1;
            usuario.Id = nuevoId;
            usuario.Nombre = user.Nombre;
            usuario.email = user.Email;
            usuario.Password = user.Password;
            usuario.Salt = HelperCryptography.GenerateSalt();
            usuario.Password_hash = HelperCryptography.EncryptPassword(user.Password, usuario.Salt);
            usuario.ImagenPerfil = "default.jpg";
            usuario.FechaRegistro = DateTime.Now;
            usuario.TipoUsuario = "lector";

            this.context.Usuarios.Add(usuario);
            await context.SaveChangesAsync();
        }


        public async Task<Usuarios> GetUsuario(int id)
        {
            var usuario = await this.context.Usuarios.Where(x => x.Id == id).FirstOrDefaultAsync();
            return usuario;
        }

        public async Task<List<CountLibrosListasPredefinidas>> ObtenerCountListas(int idUsuario)
        {
            var consulta = await this.context.CountLibrosListasPredefinidas
                               .Where(datos => datos.Id == idUsuario)
                               .AsNoTracking()
                               .ToListAsync();
            return consulta;
        }

        public async Task<List<LibrosListasPredefinidas>> LibrosEnPredefinidos(int idUsuario)
        {
            var consulta = await this.context.LibrosListasPredefinidas
                           .Where(datos => datos.Id == idUsuario)
                           .AsNoTracking()
                           .ToListAsync();
            return consulta;
        }

        public async Task<List<LibrosListasPredefinidas>> FindLibrosEnPredefinidos(int idUsuario, int idlista)
        {
            var consulta = await this.context.LibrosListasPredefinidas
                           .Where(datos => datos.Id == idUsuario && datos.ListaId == idlista)
                           .AsNoTracking()
                           .ToListAsync();
            return consulta;
        }

        public async Task<List<ObjetivosUsuarios>> ObjetivosUsuarios(int idUsuario)
        {
            var consulta = await this.context.ObjetivosUsuarios
                .Where(datos => datos.IdUsuario == idUsuario)
                .AsNoTracking()
                .ToListAsync();
            return consulta;
        }

        public async Task<List<ReseñaDTO>> Reseñas(int idLibro)
        {
            var resenas = await this.context.Reseñas
        .Where(r => r.idLibro == idLibro)
        .OrderByDescending(r => r.fecha)
        .ToListAsync();

            var usuarioIds = resenas.Select(r => r.UsuarioId).Distinct().ToList();

            var usuarios = await this.context.Usuarios
                .Where(u => usuarioIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u);

            var resenasDto = resenas.Select(r => new ReseñaDTO
            {
                Id = r.Id,
                UsuarioId = r.UsuarioId,
                NombreUsuario = usuarios.TryGetValue(r.UsuarioId, out var u) ? u.Nombre : "Anónimo",
                ImagenPerfil = usuarios.TryGetValue(r.UsuarioId, out var u2) ? u2.ImagenPerfil : null,
                Calificacion = r.calificacion,
                Texto = r.texto,
                Fecha = r.fecha
            }).ToList();

            return resenasDto;
        }

        public async Task<DetallesAutor> FindAutorAsync(int idAutor)
        {
            var autor = await this.context.Autores.FindAsync(idAutor);
            var libros = await this.context.LibrosAutor.Where(x => x.IdAutor == idAutor).ToListAsync();

            return new DetallesAutor
            {
                Libros = libros,
                Autores = autor
            };

        }

        public async Task<int> LibrosListaDetalle(int idLibro, int? idUsuario)
        {
            return (await context.IdUsuariosListasPredefinidas
                .Where(ul => ul.LibroId == idLibro && ul.UsuarioId == idUsuario)
                .Select(ul => (int?)ul.ListaPredefinidaId)
                .FirstOrDefaultAsync()) ?? 0;
        }

        public async Task<ReseñaDTO> UpdateReseña(int idReseña, int idUsuario, int nuevaCalificacion, string nuevoTexto)
        {
            var reseña = await this.context.Reseñas.Where(x => x.Id == idReseña && x.UsuarioId == idUsuario).FirstOrDefaultAsync();
            if (reseña == null)
            {
                throw new Exception("No se encontró la reseña o no tienes permisos para editarla.");
            }
            reseña.calificacion = nuevaCalificacion;
            reseña.texto = nuevoTexto;
            reseña.fecha = DateTime.Now;
            this.context.Reseñas.Update(reseña);
            await this.context.SaveChangesAsync();
            ReseñaDTO dto = new ReseñaDTO
            {
                Id = reseña.Id,
                UsuarioId = reseña.UsuarioId,
                IdLibro = reseña.idLibro,
                Calificacion = reseña.calificacion,
                Texto = reseña.texto,
                Fecha = reseña.fecha
            };

            return dto;
        }

        public async Task InsertReseña(int usuario_id, int libro_id, int calificacion, string texto)
        {
            //var sql = "EXEC INSERT_RESENA @usuario_id, @libro_id, @calificacion, @texto";

            await this.context.Database.ExecuteSqlRawAsync(
                "EXEC INSERT_RESENA @usuario_id, @libro_id, @calificacion, @texto",
                new SqlParameter("@usuario_id", usuario_id),
                new SqlParameter("@libro_id", libro_id),
                new SqlParameter("@calificacion", calificacion),
                new SqlParameter("@texto", texto)
            );
        }

        public async Task InsertObjetivo(int idUsuario, string titulo, DateTime fin, string tipo, int meta)
        {
            await this.context.Database.ExecuteSqlRawAsync(
                "EXEC INSERT_OBJETIVO @usuario_id, @titulo, @fecha_fin, @tipo, @meta",
                new SqlParameter("@usuario_id", idUsuario),
                new SqlParameter("@titulo", titulo),
                new SqlParameter("@fecha_fin", fin),
                new SqlParameter("@tipo", tipo),
                new SqlParameter("@meta", meta)
            );

        }

        public async Task<ProgresoLectura> GetProgresoLectura(int idUsuario, int idLibro)
        {
            var consulta = await this.context.ProgresoLecturas
                .Where(z => z.idUsuario == idUsuario && z.idLibro == idLibro)
                .FirstOrDefaultAsync();
            return consulta;
        }

        public async Task<List<Etiquetas>> GetEtiquetas()
        {
            var consulta = from datos in this.context.Etiquetas
                           select datos;
            return await consulta.ToListAsync();
        }

        public async Task<List<LibroEtiquetas>> GetEtiquetasLibroByUsuario(int? idUsuario)
        {
            var librosIds = await this.context.Libros
                          .Where(l => l.Id == idUsuario)
                          .Select(l => l.Id)
                          .ToListAsync();

            var consulta = from datos in this.context.LibrosEtiquetas
                           where librosIds.Contains(datos.LibroId)
                           select datos;
            return await consulta.ToListAsync();
        }

        public async Task DeleteObjetivo(int idObjetivo)
        {
            var objetivo = await this.context.ObjetivosUsuarios.FindAsync(idObjetivo);
            this.context.ObjetivosUsuarios.Remove(objetivo);
            await this.context.SaveChangesAsync();
        }
        public async Task<ObjetivosUsuarios> UpdateObjetivo(int idObjetivo, int idUsuario, int progreso)
        {
            var objetivo = await this.context.ObjetivosUsuarios.Where(x => x.idObjetivo == idObjetivo && x.IdUsuario == idUsuario).FirstOrDefaultAsync();
            objetivo.ProgresoActual = progreso;
            this.context.ObjetivosUsuarios.Update(objetivo);
            await this.context.SaveChangesAsync();
            return objetivo;
        }

        public async Task InsertProgreso(int UsuarioId, int LibroId)
        {
            string sql = "EXEC InsertarProgresoLectura @UsuarioId ,@LibroId";
            SqlParameter pamUsuario = new SqlParameter("@UsuarioId", UsuarioId);
            SqlParameter pamIdLibro = new SqlParameter("@LibroId", LibroId);

            await this.context.Database.ExecuteSqlRawAsync(sql, pamUsuario, pamIdLibro);
        }

        public async Task DeleteProgreso(int idProgreso, int idUsuario)
        {
            var progresos = await this.context.ProgresoLecturas.Where(z => z.ID == idProgreso && z.idUsuario == idUsuario).FirstOrDefaultAsync();
            this.context.ProgresoLecturas.Remove(progresos);
            await this.context.SaveChangesAsync();
        }
        public async Task<int?> FindProgreso(int idUsuario, int idLibro)
        {
            return await this.context.ProgresoLecturas
            .Where(p => p.idUsuario == idUsuario && p.idLibro == idLibro)
            .Select(p => (int?)p.ID)
            .FirstOrDefaultAsync();
        }

        public async Task<ProgresoLectura> UpdateProgreso(int idProgreso, int idUsuario, int pagina)
        {
            var progreso = await this.context.ProgresoLecturas.Where(x => x.ID == idProgreso && x.idUsuario == idUsuario).FirstOrDefaultAsync();
            var libro = await this.FindLibros(progreso.idLibro);
            if (progreso == null)
            {
                return null;
            }

            if (progreso.Pagina == libro.NumeroPaginas)
            {
                await this.MoverLibrosLista(idUsuario, progreso.idLibro, 1, 2);
                return progreso;
            }
            else
            {
                progreso.Pagina = pagina;
                progreso.Actualizacion = DateTime.Now;

                this.context.ProgresoLecturas.Update(progreso);
                await this.context.SaveChangesAsync();
                return progreso;
            }
        }


        public async Task<List<Libros>> FiltrarPorEtiquetas(int idEtiqueta)
        {
            var consulta = from datos in context.LibrosEtiquetas
                           where datos.EtiquetaId == idEtiqueta
                           select datos;

            var librosIds = consulta.Select(x => x.LibroId).ToList();

            return this.context.Libros
                .Where(x => librosIds.Contains(x.Id))
                .AsEnumerable()
                .DistinctBy(x => x.Id)
                .ToList();
        }

        public async Task<Usuarios> UpdateUsuarios(Usuarios usuarios)
        {
            var usuarioExistente = await this.context.Usuarios.FindAsync(usuarios.Id);

            if (usuarioExistente != null)
            {
                usuarioExistente.Nombre = usuarios.Nombre;
                usuarioExistente.email = usuarios.email;
                usuarioExistente.ImagenPerfil = usuarios.ImagenPerfil;

                if (!string.IsNullOrEmpty(usuarios.Password))
                {
                    usuarioExistente.Password = usuarios.Password;
                }

                this.context.Update(usuarioExistente);
                await this.context.SaveChangesAsync();
                return usuarioExistente;
            }
            else
            {
                throw new Exception("Usuario no encontrado.");
            }
        }

        public async Task UpdateLibro(Libros libro)
        {
            this.context.Libros.Update(libro);
            await this.context.SaveChangesAsync();
        }

        public async Task<string> GetFotoUsuario(int idUsuario)
        {
            var foto = await this.context.Usuarios
                .Where(u => u.Id == idUsuario)
                .Select(u => u.ImagenPerfil)
                .FirstOrDefaultAsync();

            return foto;
        }

        public async Task<bool> UpdateFotoUsuario(int idUsuario, string fileName)
        {
            try
            {
                var usuario = await this.GetUsuario(idUsuario);
                if (usuario == null) return false;

                usuario.ImagenPerfil = fileName;
                await this.context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al actualizar foto de perfil: {ex.Message}");
                return false;
            }
        }


        public async Task<Etiquetas> FindEtiqueta(int idEtiqueta)
        {
            var etiqueta = await this.context.Etiquetas.FindAsync(idEtiqueta);
            return etiqueta;
        }
    }
    
}
