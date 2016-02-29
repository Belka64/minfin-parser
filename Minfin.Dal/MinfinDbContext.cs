using System.Data.Entity;

namespace Minfin.Dal
{
    public class MinfinDbContext : DbContext
    {
        public MinfinDbContext()
            : base("minfinlocaldb")
        {
            Database.SetInitializer<MinfinDbContext>(new CreateDatabaseIfNotExists<MinfinDbContext>());
        }

        public DbSet<Record> Record { get; set; }

    }
}
