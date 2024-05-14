namespace GenerateDishesAPI.Models
{
	public class User
	{
		public int Id { get; set; }
		public string Name { get; set; }

		public ICollection<Dish>? Dishes { get; set; }
	}
}
