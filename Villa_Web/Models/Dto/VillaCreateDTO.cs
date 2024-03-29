﻿using System.ComponentModel.DataAnnotations;

namespace Villa_Web.Models.Dto
{
    public class VillaCreateDTO
    {
        [Required]
        public string Name { get; set; }
        public string? Details { get; set; }
        [Required]
        public double Rate { get; set; }
        public int Occupancy { get; set; }
        public int SqFt { get; set; }
        public string? ImageUrl { get; set; }
        public IFormFile? Image { get; set; }
        public string? Amenity { get; set; }
    }
}
