namespace Cars.Migrations
{
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using Models;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Data.Entity.Validation;
    using System.Linq;
    using System.Text;
    using System.Threading;

    internal sealed class Configuration : DbMigrationsConfiguration<Cars.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }
        protected override void Seed(Cars.Models.ApplicationDbContext context)
        {
            Truncate(context);
            SeedUsers(context);
            SeedCars(context);
            SeedRatings(context);
        }

        private void Truncate(ApplicationDbContext context)
        {
            context.Database.ExecuteSqlCommand("delete FROM [Cars].[dbo].[Likes]");
            context.Database.ExecuteSqlCommand("delete FROM [Cars].[dbo].[Reviews]");
            context.Database.ExecuteSqlCommand("delete FROM [Cars].[dbo].[Cars]");
            context.Database.ExecuteSqlCommand("delete FROM [Cars].[dbo].[Manufacturers]");
            context.Database.ExecuteSqlCommand("delete FROM [Cars].[dbo].[AspNetUsers]");
            context.Database.ExecuteSqlCommand("delete FROM [Cars].[dbo].[AspNetRoles]");
            context.Database.ExecuteSqlCommand("delete FROM [Cars].[dbo].[AspNetUserRoles]");
            context.SaveChanges();
        }

        private void SeedRatings(ApplicationDbContext context)
        {
            if (context.Review.Any())
            {
                Console.WriteLine("Aborting seeding ratings, Reviews already exist.");
                return;
            }
            if (context.Likes.Any())
            {
                Console.WriteLine("Aborting seeding ratings, Likes already exist.");
                return;
            }
            var cars = context.Cars.Include(c => c.Make).ToList();
            var users = context.Users.ToList();

            if (users.Count == 0) throw new Exception("No users");

            foreach (var c in cars)
            {
                for (int i = 0; i < 20; i++)
                {
                    context.Review.AddOrUpdate(new Review()
                    {
                        Author = users[0],
                        Text = $"Review {i} for {c.Make.Name}-{c.Model} owned by {c.Owner.UserName}",
                        Car = c,
                        CarID = 42
                    });
                }

                foreach (var u in users)
                {
                    var date = DateTime.UtcNow;
                    var pastMonth = date.AddMonths(-1);
                    var pastTwoMonths = date.AddMonths(-2);

                    if (RandomBool())
                        context.Likes.AddOrUpdate(new Like() { Author = u, Created = date, CarID = c.CarID, Car = c });
                    if (RandomBool())
                        context.Likes.AddOrUpdate(new Like() { Author = u, Created = pastMonth, CarID = c.CarID, Car = c });
                    if (RandomBool())
                        context.Likes.AddOrUpdate(new Like() { Author = u, Created = pastTwoMonths, CarID = c.CarID, Car = c });
                }

            }

            context.SaveChanges();
        }

        private void SeedCars(ApplicationDbContext context)
        {
            if (context.Cars.Any()) return;
            if (context.Manufacturers.Any()) return;

            var users = context.Users.ToList();
            if (users.Count < 5) throw new Exception("Not enough users");

            var vw = new Manufacturer() { Name = "VW" };
            var bmw = new Manufacturer() { Name = "BMW" };
            var audi = new Manufacturer() { Name = "AUDI" };
            var mercedes = new Manufacturer() { Name = "MERCEDES" };

            var golf4 = new Car() { Model = "Golf", Make = vw, Owner = users[0], Created = RandomMonthDate() };
            var jetta = new Car() { Model = "Jetta", Make = vw, Owner = users[0], Created = RandomMonthDate() };
            var is318 = new Car() { Model = "318is", Make = bmw, Owner = users[1], Created = RandomMonthDate() };
            var a4 = new Car() { Model = "A4", Make = audi, Owner = users[2], Created = RandomMonthDate() };
            var a6 = new Car() { Model = "A6", Make = audi, Owner = users[3], Created = RandomMonthDate() };
            var a8 = new Car() { Model = "A8", Make = audi, Owner = users[3], Created = RandomMonthDate() };
            var clk = new Car() { Model = "CLK", Make = mercedes, Owner = users[3], Created = RandomMonthDate() };
            var slk = new Car() { Model = "SLK", Make = mercedes, Owner = users[3], Created = RandomMonthDate() };

            context.Manufacturers.Add(vw);
            context.Manufacturers.Add(bmw);
            context.Manufacturers.Add(audi);
            context.Manufacturers.Add(mercedes);

            context.Cars.Add(golf4);
            context.Cars.Add(jetta);
            context.Cars.Add(is318);
            context.Cars.Add(a4);
            context.Cars.Add(a6);
            context.Cars.Add(a8);
            context.Cars.Add(clk);
            context.Cars.Add(slk);

            context.SaveChanges();
        }

        private DateTime RandomMonthDate()
        {
            Thread.Sleep(20);
            var r = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);
            var date = new DateTime(2016, r.Next(1, 13), 12);
            return date;
        }

        private bool RandomBool()
        {
            Thread.Sleep(20);
            var r = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);
            return r.Next(1, 3) % 2 == 0;
        }

        private void SeedUsers(ApplicationDbContext context)
        {
            try
            {
                if (context.Users.Any()) return;

                var userStore = new ApplicationUserStore(context);
                var roleStore = new ApplicationRoleStore(context);
                var userManager = new ApplicationUserManager(userStore);

                var roleManager = new RoleManager<ApplicationRole>(roleStore);

                var admin = new ApplicationUser() { Email = "admin@softuni.bg", UserName = "admin@softuni.bg", Id = Guid.NewGuid().ToString() };
                var owner = new ApplicationUser() { Email = "owner@softuni.bg", UserName = "owner@softuni.bg", Id = Guid.NewGuid().ToString() };
                var user1 = new ApplicationUser() { Email = "user1@softuni.bg", UserName = "user1@softuni.bg", Id = Guid.NewGuid().ToString() };
                var user2 = new ApplicationUser() { Email = "user2@softuni.bg", UserName = "user2@softuni.bg", Id = Guid.NewGuid().ToString() };
                var user3 = new ApplicationUser() { Email = "user3@softuni.bg", UserName = "user3@softuni.bg", Id = Guid.NewGuid().ToString() };

                userManager.Create(admin, "aA123456#");
                userManager.Create(owner, "aA123456#");
                userManager.Create(user1, "aA123456#");
                userManager.Create(user2, "aA123456#");
                userManager.Create(user3, "aA123456#");
                roleManager.Create(new ApplicationRole("Admin"));
                userManager.AddToRole(admin.Id, "Admin", admin.Id);
                userManager.AddToRole(owner.Id, "Admin", admin.Id);
            }
            catch (DbEntityValidationException ex)
            {
                StringBuilder sb = new StringBuilder();

                foreach (var failure in ex.EntityValidationErrors)
                {
                    sb.AppendFormat("{0} failed validation\n", failure.Entry.Entity.GetType());
                    foreach (var error in failure.ValidationErrors)
                    {
                        sb.AppendFormat("- {0} : {1}", error.PropertyName, error.ErrorMessage);
                        sb.AppendLine();
                    }
                }

                throw new DbEntityValidationException(
                    "Entity Validation Failed - errors follow:\n" +
                    sb.ToString(), ex
                ); // Add the original exception as the innerException
            }
        }
    }
}
