using Microsoft.CodeAnalysis;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using VacationApi.Auth;
using VacationApi.Domains.Exceptions;

namespace VacationApi.Domains
{
    public class Vacation
    {
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public string Place { get; set; }
        [Required]
        public double Latitude { get; set; }
        [Required]
        public double Longitude { get; set; }
        [Required]
        public DateTime DateTimeBegin { get; set; }
        [Required]
        public DateTime DateTimeEnd { get; set; }
        [Required]
        [ForeignKey("User")]
        public string UserId { get; set; }
        [Required]
        public Boolean IsPublished { get; set; }
        [Required]
        public String Country { get; set; }
        [AllowNull]
        public String? PicturePath { get; set; }
        [NotMapped]
        public String UserName { get; set; }

        public static Vacation New(String title, String description, String place, double latitude, double longitude, DateTime begin, DateTime end, String userId, string? picturePath = null, String country = "Belgique")
        {
            if (title.Trim().Length < 5)
            {
                throw new InvalidVacationInformation("The title must have at least 5 characters.");
            }
            if (description.Trim().Length < 5)
            {
                throw new InvalidVacationInformation("The description must have at least 5 characters.");
            }
            if (place.Trim().Length < 5)
            {
                throw new InvalidVacationInformation("The place must have at least 5 characters.");
            }
            if (userId.Trim().Length < 5)
            {
                throw new InvalidVacationInformation("The vacation must be attached to a user.");
            }
            if (DateTime.Compare(begin.AddHours(1), DateTime.Now) < 0)
            {
                throw new InvalidVacationInformation("You can't add a vacation in the past.");
            }
            if (DateTime.Compare(end, begin) <= 0)
            {
                throw new InvalidVacationInformation("A vacation need to end after its beginning.");
            }

            return new Vacation
            {
                DateTimeBegin = begin,
                DateTimeEnd = end,
                Description = description,
                Title = title,
                Latitude = latitude,
                Longitude = longitude,
                Place = place,
                UserId = userId,
                Country = country,
                PicturePath = picturePath,
                IsPublished = false
            };
        }
    }
}
