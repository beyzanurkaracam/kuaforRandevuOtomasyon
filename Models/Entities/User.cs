using kuaforBerberOtomasyon.Enums;
using System.ComponentModel.DataAnnotations;

namespace kuaforBerberOtomasyon.Models.Entities
{
    public class User
    {
        [Key]
        public int UserID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
       
        public string Email { get; set; }
        public string Password { get; set; }
        
        public String Role { get; set; } // Kullanıcının rolü (Admin, Employee, Customer)

        // Kullanıcının randevuları
        public ICollection<Appointment> Appointments { get; set; }
    }
}
