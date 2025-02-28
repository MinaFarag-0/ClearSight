using ClearSight.Core.Enums;
using ClearSight.Core.Mosels;
using ClearSight.Infrastructure.Context;
using Microsoft.AspNetCore.Identity;

namespace ClearSight.Infrastructure.Implementations.Services
{
    public class ActivateUserAccountsServices(AppDbContext context, UserManager<User> userManager)
    {
        private readonly AppDbContext _context = context;
        private readonly UserManager<User> _userManager = userManager;

        public async Task<bool> ActivateUserAccount(User user)
        {
            var checkUserExsist = _context.Users.Find(user.Id);
            var userUserADoctor = await _userManager.IsInRoleAsync(user, Roles.Doctor.ToString());
            if (checkUserExsist != null)
            {
                if (userUserADoctor && _context.Doctors.Find(user.Id) == null)
                {
                    var doctor = new Doctor()
                    {
                        User = user
                    };
                    _context.Add(doctor);
                    _context.SaveChanges();
                }
                if (!userUserADoctor && _context.Patients.Find(user.Id) == null)
                {
                    var patient = new Patient()
                    {
                        User = user
                    };
                    _context.Add(patient);
                    _context.SaveChanges();
                }
            }
            return true;

        }
    }
}
