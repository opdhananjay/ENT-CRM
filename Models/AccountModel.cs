namespace ENT.Models
{
    public class AccountModel
    {

    }

    public class UserRegistration
    {
        public int Id { get; set; }  // Pass  0 
        public string Fname { get; set; }
        public string Lname { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string PhoneNumber { get; set; }
        public string ProfileImageUrl { get; set; }
        public bool IsActive { get; set; }
        public bool IsVerified { get; set; }
        public int RoleId { get; set; }
        public int OrgId { get; set; }
        public int? CreatedBy { get; set; } = 0; // Pass Created by Value At the time of Auto Creation by Admin Id
        public bool IsAuto { get; set; } = false;
    }







}
