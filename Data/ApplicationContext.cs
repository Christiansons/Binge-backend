﻿using GenerateDishesAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace GenerateDishesAPI.Data
{
    public class ApplicationContext : DbContext
    {
        public DbSet<Dish> Dishes { get; set; }
        public DbSet<Ingredient> Ingredients { get; set; }
        public DbSet<Recipe> Recipes { get; set; }
        public DbSet<User> Users { get; set; }

        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Data Source=(LocalDb)\\MSSQLLocalDB;Initial Catalog=BingeDb;Integrated Security=True");

        public ApplicationContext()
        {
        }
    }
}
