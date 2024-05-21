namespace GenerateDishesAPI.Models
{
	public class Ingredient
	{
		public int Id { get; set; }
		public string IngredientName { get; set; }

		public virtual Dish dish { get; set; }
	}
}
