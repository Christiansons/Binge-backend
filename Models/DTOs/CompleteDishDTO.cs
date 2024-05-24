namespace GenerateDishesAPI.Models.DTOs
{
	public class CompleteDishDTO
	{
		public string dishName { get; set; }
		public List<string> ingredients { get; set; }
		public string recipe { get; set; }
	}
}
