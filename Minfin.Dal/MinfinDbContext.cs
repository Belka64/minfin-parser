using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
