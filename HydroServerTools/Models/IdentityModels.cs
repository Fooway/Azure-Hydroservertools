﻿using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HydroServerTools.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class User : IdentityUser
    {
        public string UserEmail { get; set; }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<User> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }

    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }
        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }


        public DbSet<ConnectionParameters> ConnectionParameters { get; set; }
        public DbSet<ConnectionParametersUser> ConnectionParametersUser { get; set; }
       

        public DbSet<HydroServerTools.Models.ServiceRegistration> ServiceRegistrations { get; set; }


        //protected override void OnModelCreating(DbModelBuilder modelBuilder)
        //{


        //    base.OnModelCreating(modelBuilder);
        //    modelBuilder.Entity<IdentityUserLogin>().HasKey<string>(l => l.UserId);
        //    modelBuilder.Entity<IdentityRole>().HasKey<string>(r => r.Id);
        //    modelBuilder.Entity<IdentityUserRole>().HasKey(r => new { r.RoleId, r.UserId });

        //    modelBuilder.Entity<User>().Property(c => c.UserName).IsRequired();
        //    modelBuilder.Entity<User>().Property(c => c.UserName).HasMaxLength(100);
        //}


    }
    public class ConnectionParameters
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string DataSource { get; set; }
        public string InitialCatalog { get; set; }
        public string UserId { get; set; }
        public string Password { get; set; }

        //public virtual ICollection<User> User { get; set; }
        //public virtual newtable2 nt2 {get; set;}
    }

    public class ConnectionParametersUser
    {
        public int Id { get; set; }
        [Display(Name = "Connection Name")]
        public int ConnectionParametersId { get; set; }
        [Display(Name = "User Name")]      
        public string UserId { get; set; }
        public virtual ConnectionParameters ConnectionParameters { get; set; }
        public virtual User User { get; set; }
    }
   
    public class ServiceRegistration
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]//switch on autogenerated
        [Key]//set as Primary key
        public int Id { get; set; }
        
        [DatabaseGenerated(DatabaseGeneratedOption.None)]// switch off autogenerated PK

        [MaxLength(30)]
        [Required]
        [RegularExpression("^[a-zA-Z]*$", ErrorMessage = "Please Enter Only Letters (no spaces)")]       
        [Display(Name = "Service Name*")]
        public string ServiceName { get; set; }
        [MaxLength(30)]
        [Required]
        [Display(Name = "Google Account Email*")]
        [RegularExpression(".+@.+\\..+", ErrorMessage = "Please Enter Correct Email Address")]
        public string GoogleAccount { get; set; }
        [MaxLength(100)]
        [Required]
        [Display(Name = "Service Title*")]
        public string ServiceTitle { get; set; }
        [MaxLength(500)]
       // [Required]
       // [DataType(DataType.MultilineText)]
        //[RegularExpression("!/r/n", ErrorMessage = "Please remove carriage returns")]
        [Display(Name = "Service Abstract")]
        public string ServiceDescription { get; set; }
        [MaxLength(50)]
        [Required]
        [Display(Name = "Contact Name*")]
        public string ContactName { get; set; }
        [MaxLength(30)]
        [Required]
        [Display(Name = "Contact Email*")]
        [RegularExpression(".+@.+\\..+", ErrorMessage = "Please Enter Correct Email Address")]
        public string ContactEmail { get; set; }
        [MaxLength(12)]
        //[Required]
        [Display(Name = "Contact Phone")]
        public string ContactPhone { get; set; }
        [MaxLength(100)]
        [Required]
        [Display(Name = "Organization*")]
        public string Organization { get; set; }
        [MaxLength(100)]
        [RegularExpression("^(http[s]?:\\/\\/(www\\.)?|ftp:\\/\\/(www\\.)?|www\\.){1}([0-9A-Za-z-\\.@:%_\\+~#=]+)+((\\.[a-zA-Z]{2,3})+)(/(.)*)?(\\?(.)*)?", ErrorMessage = "Please Enter Correct Website URL")]
        [Display(Name = "Organization Website")]
        public string OrganizationUrl { get; set; }
        [DataType(DataType.Date)]
        public System.DateTime RequestIssued { get; set; }
        [DataType(DataType.Date)]
        public Nullable <DateTime> RequestConfirmed { get; set; }
        
        public Guid ActivationGuid { get; set; }
}
}