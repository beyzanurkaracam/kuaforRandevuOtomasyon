using kuaforBerberOtomasyon.Models.Entities;

namespace kuaforBerberOtomasyon.Models.DTO
{
    public class CreateAppointmentDto
    {
        public int ServiceID { get; set; }
        public int EmployeeID { get; set; }
        public DateTime AppointmentDate { get; set; }
        public TimeSpan AppointmentTime { get; set; }
        public int UserID { get; set; } // Kullanıcı ID

        public List<Services> Services { get; set; } // Hizmetler
        public List<Employee> Employees { get; set; } // Çalışanlar
    }
}
