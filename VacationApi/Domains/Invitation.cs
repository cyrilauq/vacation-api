using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VacationApi.Domains
{
    public class Invitation
    {
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public String Id { get; set; }

        [Required]
        [ForeignKey("User")]
        public string UserId { get; set; }

        [Required]
        [ForeignKey("Vacation")]
        public string VacationId { get; set; }

        [Required]
        public bool IsAccepted { get; set; }

        public static Invitation New(String userId, String vacationId)
        {
            return new Invitation
            {
                UserId = userId,
                VacationId = vacationId,
                IsAccepted = false
            };
        }
    }
}
