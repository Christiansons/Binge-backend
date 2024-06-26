﻿namespace GenerateDishesAPI.Models
{
	public class Ingredient
	{
		public int Id { get; set; }
		public string IngredientName { get; set; }

		public int DishId { get; set; }
		public virtual Dish dish { get; set; } = null!;
	}
}
