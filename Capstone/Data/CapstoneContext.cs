using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Capstone.Models;

namespace Capstone.Data
{
    public class CapstoneContext : DbContext
    {
        public CapstoneContext (DbContextOptions<CapstoneContext> options)
            : base(options)
        {
        }

        public DbSet<Capstone.Models.Applicant> Applicant { get; set; } = default!;
    }
}
