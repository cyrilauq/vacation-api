using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VacationApi.Domains;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using VacationApi.Auth;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Tests
{
    public static class DbContextFactory
    {
        public static VacationApiDbContext Create()
        {
            var options = new DbContextOptionsBuilder<VacationApiDbContext>()
                .UseInMemoryDatabase(databaseName: "TestVacationBd")
                .Options;

            var context = new VacationApiDbContext(options);

            context.Database.EnsureCreated();

            return context;
        }

        private static void Seed(VacationApiDbContext context)
        {
            context.SaveChanges();
        }

        public static void Destroy(VacationApiDbContext context)
        {
            context.Database.EnsureDeleted();
            context.Dispose();
        }
    }
}
