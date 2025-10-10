using Microsoft.EntityFrameworkCore;
using SignaturePad.Models;

namespace SignaturePad.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Signature> Signatures { get; set; }
    }
}
