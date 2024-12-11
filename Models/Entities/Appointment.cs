using kuaforBerberOtomasyon.Enums;
using System.ComponentModel.DataAnnotations;

namespace kuaforBerberOtomasyon.Models.Entities
{
    public class Appointment
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
       
        public DateTime RandevuSaati { get; set; }
       
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string ServiceName { get; set; }
        public bool IsApproved { get; set; } = false;
    }
}
