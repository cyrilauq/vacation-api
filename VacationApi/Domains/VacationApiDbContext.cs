using Bogus;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VacationApi.Infrastructure;
using VacationApi.Domains;

namespace VacationApi.Domains
{
    public class VacationApiDbContext : IdentityDbContext<User>
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Vacation> Vacations { get; set; }
        public DbSet<Invitation> Invitations { get; set; }
        public DbSet<VacationActivity> Activities { get; set; }
        public DbSet<Message> Messages { get; set; }

        public VacationApiDbContext(DbContextOptions<VacationApiDbContext> options) : base(options) { }

        private static readonly Faker<User> UserFaker = new Faker<User>()
            .RuleFor(x => x.Name, x => x.Person.LastName)
            .RuleFor(x => x.FirstName, x => x.Person.FirstName)
            .RuleFor(x => x.UserName, x => x.Person.UserName)
            .RuleFor(x => x.Email, x => x.Internet.Email(x.Person.FirstName, x.Person.LastName));

        public User GenerateUser()
        {
            User product = UserFaker.Generate();
            return product;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>().HasData(new User
            {
                Name = "Auquier",
                FirstName = "Cyril",
                Email = "cyril.auquier@vacation.api",
                UserName = "cyril_auquier",
                PicturePath = "url"
            });
        }

        public bool AddVacation(Vacation vacation)
        {
            var lastCount = Vacations.Count();
            Vacations.Add(vacation);
            SaveChanges();
            return Vacations.Count() > lastCount;
        }

        public IEnumerable<Vacation> GetVacations()
        {
            return Vacations;
        }

        public Boolean isFree(DateTime from, DateTime to)
        {
            return Vacations.Where(x => 
                (x.DateTimeBegin >= from && x.DateTimeEnd <= to) ||
                (x.DateTimeBegin <= from && x.DateTimeEnd <= to) ||
                (x.DateTimeBegin <= from && x.DateTimeEnd >= to)
            ).ToList().Count() == 0;
        }

        public bool Exists(string title)
        {
            throw new NotImplementedException();
        }

        public bool isFree(DateTime from, DateTime to, string uid)
        {
            throw new NotImplementedException();
        }

        public bool ExistsFor(string title, string uid)
        {
            throw new NotImplementedException();
        }

        public DbSet<VacationApi.Domains.VacationActivity>? VacationActivity { get; set; }
    }
}
