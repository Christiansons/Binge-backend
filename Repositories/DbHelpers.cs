using GenerateDishesAPI.Data;
using Microsoft.EntityFrameworkCore;
using GenerateDishesAPI.Models;
using System.Net;

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
						
					});
				} catch
				{
					return Results.Problem("Could not add dish");
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

    }
}
