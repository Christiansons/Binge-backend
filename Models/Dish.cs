namespace GenerateDishesAPI.Models
{
	public class Dish
	{
		public int Id { get; set; }
		public string DishName { get; set; }
		public string? Url { get; set; }

		public virtual ApplicationUser user { get; set; }
		public virtual ICollection<Ingredient>? ingredients { get; set; }
		public virtual Recipe? Recipe { get; set; }
	}
}
