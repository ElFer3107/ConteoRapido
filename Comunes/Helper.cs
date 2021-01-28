using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreCRUDwithORACLE.Comunes
{
    public class Helper
    {
        public readonly string CodigoEnrutar = "BarcelonaCampeon";
        //public static bool Autenticar(string usuario, string password)
        //{
        //    string sql = @"SELECT COUNT(*)
        //              FROM Usuarios
        //              WHERE NombreLogin = @nombre AND Password = @password";


        //    using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["default"].ToString()))
        //    {
        //        conn.Open();

        //        SqlCommand command = new SqlCommand(sql, conn);
        //        command.Parameters.AddWithValue("@nombre", usuario);

        //        string hash = Helper.EncodePassword(string.Concat(usuario, password));
        //        command.Parameters.AddWithValue("@password", hash);

        //        int count = Convert.ToInt32(command.ExecuteScalar());

        //        if (count == 0)
        //            return false;
        //        else
        //            return true;

        //    }
        //}
        //public static UsuarioEntity Insert(string nombre, string apellido, string nombreLogin, string password)
        //{
        //    UsuarioEntity usuario = new UsuarioEntity();

        //    usuario.Nombre = nombre;
        //    usuario.Apellido = apellido;
        //    usuario.NombreLogin = nombreLogin;
        //    usuario.Password = password;

        //    return Insert(usuario);
        //}

        //public static UsuarioEntity Insert(UsuarioEntity usuario)
        //{

        //    string sql = @"INSERT INTO Usuarios (
        //                   Nombre
        //                  ,Apellido
        //                  ,NombreLogin
        //                  ,Password)
        //              VALUES (
        //                    @Nombre, 
        //                    @Apellido, 
        //                    @NombreLogin,
        //                    @Password)
        //            SELECT SCOPE_IDENTITY()";


        //    using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["default"].ToString()))
        //    {

        //        SqlCommand command = new SqlCommand(sql, conn);
        //        command.Parameters.AddWithValue("Nombre", usuario.Nombre);
        //        command.Parameters.AddWithValue("Apellido", usuario.Apellido);
        //        command.Parameters.AddWithValue("NombreLogin", usuario.NombreLogin);

        //        string password = Helper.EncodePassword(string.Concat(usuario.NombreLogin, usuario.Password));
        //        command.Parameters.AddWithValue("Password", password);

        //        conn.Open();

        //        usuario.Id = Convert.ToInt32(command.ExecuteScalar());

        //        return usuario;
        //    }
        //}

        //internal class Helper
        //{
        //    public static string EncodePassword(string originalPassword)
        //    {
        //        SHA1 sha1 = new SHA1CryptoServiceProvider();

        //        byte[] inputBytes = (new UnicodeEncoding()).GetBytes(originalPassword);
        //        byte[] hash = sha1.ComputeHash(inputBytes);

        //        return Convert.ToBase64String(hash);
        //    }
        //}
    }
}
