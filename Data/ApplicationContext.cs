using GenerateDishesAPI.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;


namespace GenerateDishesAPI.Data
{
    public class ApplicationContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Dish> Dishes { get; set; }
        public DbSet<Ingredient> Ingredients { get; set; }
        public DbSet<Recipe> Recipes { get; set; }
        public DbSet<ApplicationUser> Users { get; set; }
        public DbSet<Diet> Diets {  get; set; } 
        public DbSet<Allergy> Allergies { get; set; }

        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options) { }

		protected override void OnModelCreating(ModelBuilder builder)
		{
			base.OnModelCreating(builder);
		}

		public ApplicationContext()
        {
            
        }
    }
}
