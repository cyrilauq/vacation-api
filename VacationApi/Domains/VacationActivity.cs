using Bogus.DataSets;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using VacationApi.Domains.Exceptions;

namespace VacationApi.Domains
{
    public class VacationActivity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required]
        public String Id { get; set; }
        [Required]
        public String Name { get; set; }
        [Required]
        public String Description { get; set; }
        [Required]
        public Double Longitude { get; set; }
        [Required]
        public Double Latitude { get; set; }
        [Required]
        public String Place { get; set; }
        [AllowNull]
        public DateTime? Begin { get; set; }
        [AllowNull]
        public DateTime? End { get; set; }
        [Required]
        [ForeignKey("Vacation")]
        public String VacationId { get; set; }

        public static VacationActivity New(string name, string description, double longitude, double latitude, string place, string vacationId)
        {
            VerifyTitle(name);

            VerifyDescription(description);

            if (place.Trim().Length == 0)
            {
                throw new VacationActivityFormatExceptions("The place cannot be empty");
            }

            if (vacationId.Trim().Length == 0)
            {
                throw new VacationActivityFormatExceptions("The activity must be related to a vacation");
            }

            return new VacationActivity
            {
                Name = name,
                Description = description,
                Longitude = longitude,
                Latitude = latitude,
                Place = place,
                VacationId = vacationId,
                Begin = null,
                End = null
            };
        }

        private static void VerifyDescription(string description)
        {
            if (description.Trim().Length == 0)
            {
                throw new VacationActivityFormatExceptions("The description cannot be empty");
            }
            if (description.Trim().Length < 10)
            {
                throw new VacationActivityFormatExceptions("The description must be at least 10 character long");
            }
            if (description.Trim().Length > 50)
            {
                throw new VacationActivityFormatExceptions("The description can't exceed 30 characters");
            }
        }

        private static void VerifyTitle(string name)
        {
            if (name.Trim().Length == 0)
            {
                throw new VacationActivityFormatExceptions("The name cannot be empty");
            }
            if (name.Trim().Length < 5)
            {
                throw new VacationActivityFormatExceptions("The name must be at least 5 character long");
            }
            if (name.Trim().Length > 50)
            {
                throw new VacationActivityFormatExceptions("The name can't exceed 50 characters");
            }
        }
    }
}
