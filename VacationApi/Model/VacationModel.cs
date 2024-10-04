using System.ComponentModel.DataAnnotations;

namespace VacationApi.Model
{
    public class AddVacationModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Place{ get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string DateBegin { get; set; }
        public string HourBegin { get; set; }
        public string DateEnd { get; set; }
        public string HourEnd { get; set; }
        public string UserId { get; set; }
        public string? Country { get; set; }
        public IFormFile? File { get; set; }
        public string? PicturePath { get; set; }
    }
}
