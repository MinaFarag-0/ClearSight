using ClearSight.Core.Enums;
using ClearSight.Core.Models;
using ClearSight.Infrastructure.Context;
using Microsoft.AspNetCore.Identity;

namespace ClearSight.Infrastructure.Implementations.Services
{
    public class ActivateUserAccountsServices
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;

        public ActivateUserAccountsServices(AppDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<bool> ActivateUserAccount(User user)
        {
            var existingUser = await _context.Users.FindAsync(user.Id);
            if (existingUser == null)
                return false;

            var isDoctor = await _userManager.IsInRoleAsync(user, Roles.Doctor.ToString());

            if (isDoctor)
            {
                var doctorExists = await _context.Doctors.FindAsync(user.Id);
                if (doctorExists == null)
                {
                    var doctor = new Doctor
                    {
                        User = user,
                        Status = VerificationStatus.Pending
                    };
                    _context.Doctors.Add(doctor);
                    await _context.SaveChangesAsync();
                }
            }
            else
            {
                var patientExists = await _context.Patients.FindAsync(user.Id);
                if (patientExists == null)
                {
                    var patient = new Patient
                    {
                        User = user
                    };
                    _context.Patients.Add(patient);
                    await _context.SaveChangesAsync();
                }
            }

            return true;
        }
    }
}
