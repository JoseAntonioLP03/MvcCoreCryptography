using Microsoft.EntityFrameworkCore;
using MvcCoreCryptography.Data;
using MvcCoreCryptography.Helpers;
using MvcCoreCryptography.Models;

namespace MvcCoreCryptography.Repositories
{
    public class RepositoryUsuarios
    {
        private UsuariosContext context;

        public RepositoryUsuarios(UsuariosContext context)
        {
            this.context = context;
        }

        private async Task<int> GetMaxIdUsuarioAsync()
        {
            if(this.context.Usuario.Count() == 0)
            {
                return 1;
            }
            else
            {
                return await this.context.Usuario.MaxAsync(z => z.IdUsuario) + 1;
            }
        }

        public async Task RegisterUserAsync(string nombre , string email , string imagen , string password)
        {
            Usuario user = new Usuario();
            user.IdUsuario = await this.GetMaxIdUsuarioAsync();
            user.Nombre = nombre;
            user.Imagen = imagen;
            user.Email = email;

            user.Salt = HelperTools.GenerateSalt();
            user.Password = HelperCryptography.EncryptPassword(password, user.Salt);
            await this.context.Usuario.AddAsync(user);
            await this.context.SaveChangesAsync();

        }

        public async Task<Usuario> LogInUserAsync(string email , string password)
        {
            var consulta = from datos in this.context.Usuario
                           where datos.Email == email
                           select datos;
            Usuario user =  await consulta.FirstOrDefaultAsync();
            if(user == null)
            {
                return null;
            }
            else
            {
                string salt = user.Salt;
                byte[] temp = HelperCryptography.EncryptPassword(password, salt);
                byte[] passBytes = user.Password;
                bool response = HelperTools.CompareArrays(temp, passBytes);
                if(response == true)
                {
                    return user;
                }
                else
                {
                    return null;
                }
            }
        }
    }
}
