using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;
using System;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.Core;

namespace Cars.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser<string, IdentityUserLogin, ApplicationUserRole, IdentityUserClaim>, IUser, IUser<string> // ApplicationIdentityUser, IdentityUser
    {
        public ICollection<Car> Cars { get; set; }
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }

    public class ApplicationUserRole : IdentityUserRole
    {
        public override string RoleId { get; set;}
        public override string UserId { get; set;}
        public string AdminID { get; set; }
        public virtual ApplicationUser Admin { get; set; }
    }

    public class ApplicationIdentityRole : IdentityRole<string, ApplicationUserRole>
    {

    }

    public class ApplicationRole : ApplicationIdentityRole
    {
        public ApplicationRole()
        {
            Id = Guid.NewGuid().ToString();
        }
        public ApplicationRole(string roleName)
        {
            Id = Guid.NewGuid().ToString();
            Name = roleName;
        }
    }

    public class ApplicationRoleStore : RoleStore<ApplicationRole, string, ApplicationUserRole>
    {
        public ApplicationRoleStore(DbContext context) : base(context)
        {
        }
    }

    public class ApplicationUserStore : UserStore<ApplicationUser, ApplicationRole, string, IdentityUserLogin, ApplicationUserRole, IdentityUserClaim>, IUserStore<ApplicationUser>, IUserStore<ApplicationUser, string>, IDisposable
    {
        public ApplicationUserStore(DbContext context) : base(context)
        {
            Database.SetInitializer(new CreateDatabaseIfNotExists<ApplicationDbContext>());
        }

        // this method should be in concrete class inhereting IUserRoleStore as implementation or override, but hey, its 31.12 :)
        public IdentityResult AddToRole(string userId, string roleName, string issuerId)
        {
            
            var role = Context.Set<ApplicationRole>().Where(r => r.Name == roleName).FirstOrDefault();
            if (role == null) throw new ObjectNotFoundException(string.Format("No such role: {0}", roleName));

            Context.Set<ApplicationUserRole>().Add(new ApplicationUserRole()
            {
                UserId = userId,
                AdminID = issuerId,
                RoleId = role.Id
            });

            Context.SaveChanges();

            var result = new IdentityResult();
            return result;
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string, IdentityUserLogin, ApplicationUserRole, IdentityUserClaim>
    {
        public virtual DbSet<Car> Cars { get; set; }
        public virtual DbSet<Manufacturer> Manufacturers { get; set; }
        public virtual DbSet<Like> Likes { get; set; }
        public virtual DbSet<Review> Review { get; set; }
        public virtual DbSet<ApplicationUserRole> RoleOwnership { get; set; }

        public ApplicationDbContext() : base("DefaultConnection")
        {
            Database.SetInitializer<ApplicationDbContext>(new CreateDatabaseIfNotExists<ApplicationDbContext>());
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

    }

    public class Car
    {
        public int CarID { get; set; }
        public string Model { get; set; }
        public string OwnerID { get; set; }
        public int ManufacturerID { get; set; }
        public DateTime Created { get; set; }
        public virtual ApplicationUser Owner { get; set; }
        public virtual Manufacturer Make { get; set; }
        public virtual ICollection<Review> Reviews { get; set; }
        public virtual ICollection<Like> Likes { get; set; }
        public Car()
        {
            if (Created == null)
            {
                Created = DateTime.UtcNow;
            }
        }
    }

    public class Like
    {
        public int LikeID { get; set; }
        public string AuthorID { get; set; }
        public int CarID { get; set; }
        public virtual ApplicationUser Author { get; set; }
        public virtual Car Car { get; set; }
        public DateTime Created { get; set; }
    }

    public class Review
    {
        [Key]
        public int ReviewID { get; set; }
        public string AuthorID { get; set; }
        public int CarID { get; set; }
        public virtual ApplicationUser Author { get; set; }
        public virtual Car Car { get; set; }
        public string Text { get; set; }
    }

    public class Manufacturer
    {
        public int ManufacturerID { get; set; }
        public string Name { get; set; }
    }
}