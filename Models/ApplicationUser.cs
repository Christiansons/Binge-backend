﻿using Microsoft.AspNetCore.Identity;

namespace GenerateDishesAPI.Models
{
	public class ApplicationUser : IdentityUser
	{
        public ICollection<Dish>? Dishes { get; set; } = new List<Dish>();
		public ICollection<Allergy>? Allergies { get; set; } = new List<Allergy>();
		public ICollection<Diet>? Diets { get; set; } = new List<Diet>();
	}
}
