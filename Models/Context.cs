using kuaforBerberOtomasyon.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace kuaforBerberOtomasyon.Models
{
    public class Context:DbContext
    {
       
        public Context() { }
        public Context(DbContextOptions<Context> options)
            : base(options)
        {
        }
        public DbSet<User> User { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Services> Services { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<EmployeeService> EmployeeServices { get; set; }
        public DbSet<WorkingHours> WorkingHours { get; set; }

    }
}
