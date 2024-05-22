using Microsoft.AspNetCore.Identity;

namespace GenerateDishesAPI.Models
{
	public class ApplicationUser : IdentityUser
	{
        public ICollection<Dish>? Dishes { get; set; } = new List<Dish>();
	}
}
