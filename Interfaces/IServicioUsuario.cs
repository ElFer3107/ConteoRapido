using CoreCRUDwithORACLE.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreCRUDwithORACLE.Interfaces
{
    public interface IServicioUsuario
    {
        IEnumerable<Usuario> GetUsuarios(int codigoRol, int codigoProvincia);
        //Usuario GetUsuario(string iCedula);
        Usuario GetUsuario(string iCedula);
        Usuario GetUsuarioxCedulaMail(string iCedula,String iClave);
        Usuario ActualizaUsuario(UsuarioResponse usuarioActualizado);
        Login GetAutenticacionUsuario(string iMail, string iPass);
        Task<int> IngresaUsuario(UsuarioResponse usuario);
        Usuario ActualizaClave(Usuario usuarioNew, int estado);

         string EncerarBase(String Detalle, String Version, String Contr);

        string MuestraVersion();
        string GeneraPDF();
        string[,] Resultados_Candidato();
    }
}
