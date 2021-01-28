using CoreCRUDwithORACLE.ViewModels.Reportes;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreCRUDwithORACLE.Models
{
    public class ApplicationUser : DbContext
    {
        public ApplicationUser(DbContextOptions<ApplicationUser> options) : base(options)
        {

        }
        public DbSet<Provincia> PROVINCIA { get; set; }
        public DbSet<Rol> ROL { get; set; }
        public DbSet<Canton> CANTON { get; set; }
        public DbSet<Parroquia> PARROQUIA { get; set; }
        public DbSet<Zona> ZONA { get; set; }
        public DbSet<Junta> JUNTA { get; set; }

    }
}
