using CoreCRUDwithORACLE.ViewModels.Reportes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoreCRUDwithORACLE.Interfaces
{
    public interface IServicioReportes
    {
        Task<IEnumerable<AOperadoresCanton>> OperadoresCanton(int? codigoProvincia = null);
        Task<IEnumerable<AOperadoresParroquia>> OperadoresParroquia(int? codigoCanton = null);
        Task<IEnumerable<AOperadoresProvincia>> OperadoresProvincia(int? codigoProvincia = null);
        Task<IEnumerable<DetalleOperadores>> OperadoresDetalle(int? codigoParroquia = null);
        Task<IEnumerable<ATransmitidasProvincia>> TransmitidasProvincia(int? codigoProvincia = null);
        Task<IEnumerable<ATransmitidasCanton>> TransmitidasCanton(int? codigoProvincia = null);
        Task<IEnumerable<ATransmitidasParroquias>> TransmitidasParroquia(int? codigoCanton = null);
        Task<IEnumerable<DetallesTransmitidas>> TransmitidasDetalle(int? codigoParroquia = null);
        Task<IEnumerable<InformacionGeneral>> GeneralProvincia(int? codigoProvincia = null);
        Task<IEnumerable<InformacionGeneral>> GeneralCanton(int? codigoProvincia = null);
        Task<IEnumerable<InformacionGeneral>> GeneralParroquia(int? codigoCanton = null);

        Task<IEnumerable<GeneralAsistencia>> GeneralAsistencia(int? codigoProvincia = null);
        Task<IEnumerable<GeneralAsistencia>> GeneralAsistenciaDet(int? codigoProvincia = null);
    }
}