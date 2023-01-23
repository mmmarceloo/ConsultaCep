using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TesteConsultaCep.Models;

namespace TesteConsultaCep.Data
{
    public class TesteConsultaCepContext : DbContext
    {
        public TesteConsultaCepContext (DbContextOptions<TesteConsultaCepContext> options)
            : base(options)
        {
        }

        public DbSet<TesteConsultaCep.Models.CepBD> CepBD { get; set; } = default!;
    }
}
