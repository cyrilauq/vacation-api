namespace VacationApi.Model
{
    public class RegisterModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Mail { get; set; }
        public string PicturePath { get; set; }
        public string? Name { get; set; }
        public string? Firstname { get; set; }
        public IFormFile? File { get; set; }
    }
}
