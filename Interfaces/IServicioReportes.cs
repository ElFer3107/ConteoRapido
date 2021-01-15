﻿using CoreCRUDwithORACLE.ViewModels.Reportes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoreCRUDwithORACLE.Interfaces
{
    public interface IServicioReportes
    {
        Task<IEnumerable<AOperadoresCanton>> OperadoresCanton(int? codigoProvincia = null);
        Task<IEnumerable<AOperadoresParroquia>> OperadoresParroquia(int? codigoCanton = null);
        Task<IEnumerable<AOperadoresProvincia>> OperadoresProvincia(int? codigoProvincia = null);
    }
}