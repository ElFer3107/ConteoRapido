using CoreCRUDwithORACLE.Models;
using System.Collections.Generic;

namespace CoreCRUDwithORACLE.Interfaces
{
    public interface IServicioActa
    {
        int ActualizaActa(int cod_usuario, int junta);
        ActaResponse GetActa(int junta);
        IEnumerable<ActaResponse> GetActas(int codigoProvincia);
        Acta ConsultarAsignacion(int codigoUsuario);
        int ActualizaAsignacion(int codigoJunta);
        ResultadosVotos ConsultaResultados(int? codigoJunta);
        Respuesta ActualizarVotosActa(ResultadosVotos resultados);
    }
}