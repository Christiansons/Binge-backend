using GenerateDishesAPI.Data;
using Microsoft.EntityFrameworkCore;
using GenerateDishesAPI.Models;
using System.Net;
using Unsplasharp.Models;
using GenerateDishesAPI.Models.DTOs;
using GenerateDishesAPI.Models.ViewModels;

namespace GenerateDishesAPI.Repositories
{
    public class DbHelpers
    {
        private readonly ApplicationContext _context;

		public DbHelpers(ApplicationContext context)
		{
			_context = context;
		}

        public IResult SaveDishAndUrl(string dishName, string url, string id)
        {
			using (_context)
			{
				ApplicationUser? user = _context.Users.Where(u => u.Id == id).FirstOrDefault();
				if (user == null)
				{
					return Results.Problem("User not found");
				}

				try
				{
					user.Dishes.Add(new Dish
					{
						DishName = dishName,
						Url = url,
					});
				} catch (Exception ex)
				{
					return Results.Problem($"Could not add dish to user {ex.Message}");
				}
				try
				{
					_context.SaveChanges();
					return Results.StatusCode((int)HttpStatusCode.Created);
				}
				catch(Exception ex)
				{
					return Results.Problem($"Failed to save {ex.Message}");
				}
			}
		}

		public CompleteDishDTO GetCompleteDishFromDb(string userId, string dishName)
		{
			Dish? dish = _context.Dishes
				.Where(d => d.DishName == dishName)
				.Where(d => d.ApplicationUserId == userId)
				.Include(d => d.ingredients)
				.Include(d => d.Recipe)
				.FirstOrDefault();

			CompleteDishDTO dishDTO = new CompleteDishDTO
			{
				dishName = dish.DishName,
				ingredients = dish.ingredients.Select(i => i.IngredientName).ToList(),
				recipe = dish.Recipe.Instructions
			};

			return dishDTO;
		}

		public IResult SaveIngredientsAndRecipe(string userId, string dishName, string[] ingredients, string recipe)
		{	
			ApplicationUser? user = _context.Users.Where(u => u.Id == userId).FirstOrDefault();

			//Get the dish with the correct name, and that is connected to the correct user
			Dish? dish = _context.Dishes
			.Where(d => d.DishName == dishName)
			.Where(d => d.ApplicationUserId == userId)
			.First();

			if(dish == null)
			{
				return Results.NotFound("Gick inte att hitta rätten");
			}

			//Add all the ingredients to the dish

			try
			{
				foreach (var Ingredient in ingredients)
				{
					dish.ingredients.Add(new Ingredient()
					{
						IngredientName = Ingredient,
						DishId = dish.Id
					});
				}
			}
			catch
			{
				return Results.BadRequest("Gick inte att lägga till ingredienser till rätten");
			}
			try
			{
				dish.Recipe = new Recipe
				{
					dish = dish,
					DishId = dish.Id,
					Instructions = recipe
				};
			}
			catch
			{
				return Results.BadRequest("Gick inte att lägga till recept till rätten");
			}
			try
			{
				_context.SaveChanges();
			}
			catch
			{
				return Results.BadRequest("Gick inte att spara hela skiten");
			}
				

			return Results.Created();
			
		}

		public bool CheckIfRecipeAdded(string dishName, string userId)
		{
			Dish? dish = _context.Dishes
				.Where(d => d.DishName == dishName)
				.Where(d => d.ApplicationUserId == userId)
				.Include(d => d.Recipe)
				.First();
			if (dish.Recipe == null) return false;

			return true;
		}

		public IResult DeleteDishFromDb (string dishName, string userId)
		{
			Dish? dish = _context.Dishes
				.Where(d => d.DishName == dishName)
				.Where(d => d.ApplicationUserId == userId)
				.First();

			if (dish == null)
			{
				return Results.NotFound();
			}

			_context.Dishes.Remove(dish);

			_context.SaveChanges();
			return Results.Ok();
		}

		public IResult DeleteRecipeFromDb (string dishName, string userId)
		{
			Dish? dish = _context.Dishes
				.Where(d => d.DishName == dishName)
				.Where(d => d.ApplicationUserId == userId)
				.Include(d => d.Recipe)
				.Include(d => d.ingredients)
				.First();

			if (dish.Recipe == null)
			{
				return Results.NotFound();
			}
			_context.Ingredients.RemoveRange(dish.ingredients);
			_context.Recipes.Remove(dish.Recipe);


			_context.SaveChanges();
			return Results.Ok();
		}
		
		 

		public AllDishesConnectedToUserViewModel GetAllDishesConnectedToUser(string userId)
		{
			ApplicationUser? user = _context.Users
				.Where(u => u.Id == userId)
				.Include(u => u.Dishes)
				.FirstOrDefault();

			AllDishesConnectedToUserViewModel dishAndUrlViewModel = new AllDishesConnectedToUserViewModel()
			{
				dishViewModels = user.Dishes.Select(d => new DishViewModel
				{
					dishName = d.DishName,
					url = d.Url
				}).ToList()
			};

			return dishAndUrlViewModel;
		}
    }
}
