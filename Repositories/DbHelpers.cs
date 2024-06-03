using GenerateDishesAPI.Data;
using Microsoft.EntityFrameworkCore;
using GenerateDishesAPI.Models;
using System.Net;
using Unsplasharp.Models;
using GenerateDishesAPI.Models.DTOs;
using GenerateDishesAPI.Models.ViewModels;
using Microsoft.IdentityModel.Tokens;

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

		public int? CheckNumOfPeopleForRecipe(string userId, string dishName)
		{
			try
			{
				Dish? dish = _context.Dishes
				.Where(d => d.DishName == dishName)
				.Where(d => d.ApplicationUserId == userId)
				.Include(d => d.Servings)
				.FirstOrDefault();

				return dish.Servings;
			}
			catch (Exception ex)
			{
				return null;
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

		public IResult SaveIngredientsAndRecipe(string userId, string dishName, string[] ingredients, string recipe, int numOfPeople)
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

			//Add the cooking instructions to dish
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

			//Add the serving size to dish
			try
			{
				dish.Servings = numOfPeople;
			}catch
			{
				return Results.BadRequest("Gick inte att lägga till antalet serveringar till rätten");
			}

			//Try saving the dish
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

		//Adds allergies and diet to user
		public IResult AddAllergiesAndDietToUser(string userId, string[]? allergies, string? diet)
		{
			//gets user by id
			ApplicationUser? user = _context.Users
				.Where(u => u.Id == userId)
				.Include (u => u.Diet)
				.Include (u => u.Allergies)
				.FirstOrDefault();

			if (user == null)
			{
				return Results.NotFound("User was not found");
			}

			//Removes all users saved allergies
			if(user.Allergies != null)
			{
				var existingAllergies = user.Allergies.ToList();
				foreach (var allergy in existingAllergies)
				{
					_context.Allergies.Remove(allergy);
				}
			}

			//adds new allergies to user
			if (allergies != null && allergies.Length > 0)
			{
				foreach (string allergy in allergies)
				{
					user.Allergies.Add(new Allergy
					{
						AllergyName = allergy,
					});
				}
			}

			//Gets the connected diet
			Diet? dietToRemove = _context.Diets
				.Where(fk => fk.ApplicationUserId == user.Id)
				.FirstOrDefault();

			//adds new diet to user
			if(string.IsNullOrEmpty(diet))
			{
				//Remove diet first
				if(dietToRemove != null)
				{
					_context.Diets.Remove(dietToRemove);
				}
			}
			else
			{
				if (dietToRemove != null)
				{
					dietToRemove.DietName = diet;
				}else
				{
					user.Diet = new Diet
					{
						DietName = diet,
					};
				}
			}
			
			_context.SaveChanges();

			return Results.Created();
		}
		

		public AllergyAndDietViewModel GetAllAllergiesAndDietsConnectedToUser (string userId)
		{
			ApplicationUser? user = _context.Users
				.Where(u => u.Id == userId)
				.Include(u => u.Diet)
				.Include(u => u.Allergies)
				.FirstOrDefault();

			if (user == null)
			{
				return null; //Bättre felhantering här
			}

			string diet;
			List<string> allergies;

			if (user.Allergies.IsNullOrEmpty())
			{
				allergies = new List<string> { };
			} else
			{
				allergies = user.Allergies.Select(a => a.AllergyName).ToList();
			}

			if(user.Diet == null)
			{
				diet = string.Empty;
			}
			else
			{
				diet = user.Diet.DietName;
			}


			return new AllergyAndDietViewModel
			{
				Diet = new DietViewModel
				{
					DietName = diet
				},
				Allergies = allergies
			};
		}

		public UserViewModel GetUserInfo (string userId)
		{
			ApplicationUser? user = _context.Users
				.Where(u => u.Id == userId)
				.FirstOrDefault();

			return new UserViewModel
			{
				Email = user.Email,
				UserId = user.Id
			};
				
		}

		public string GetUserDiet (string userId)
		{
			ApplicationUser? user = _context.Users
				.Where(u => u.Id == userId)
				.Include(u => u.Diet)
				.FirstOrDefault();

			if (user.Diet == null)
			{
				return "no";
			}
			return user.Diet.DietName;
		}

		public string[]? GetUserAllergies(string userId)
		{
			ApplicationUser? user = _context.Users
				.Where(u => u.Id == userId)
				.Include(u => u.Allergies)
				.FirstOrDefault();

			return user.Allergies.Select(u => u.AllergyName).ToArray();
		}

	}
}
