using Minfin.Dal.Migrations;
using System.Data.Entity;

namespace Minfin.Dal
{
    public class MinfinDbContext : DbContext
    {
        public MinfinDbContext()
            : base("minfinlocaldb")
        {
            Database.SetInitializer<MinfinDbContext>(new MigrateDatabaseToLatestVersion<MinfinDbContext, Configuration>());
        }

        public DbSet<Record> Record { get; set; }
    }
}
