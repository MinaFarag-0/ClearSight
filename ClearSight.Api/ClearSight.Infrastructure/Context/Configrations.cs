using ClearSight.Core.Mosels;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace ClearSight.Infrastructure.Context
{
    public class UserConfigrations : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.Ignore(c => c.PhoneNumber)
                .Ignore(c => c.PhoneNumberConfirmed);

        }
        public class PatientConfigrations : IEntityTypeConfiguration<Patient>
        {
            public void Configure(EntityTypeBuilder<Patient> builder)
            {
                builder.HasOne(x => x.User).WithOne()
                    .HasForeignKey<Patient>(p => p.PatientId);

                builder.HasMany(x => x.PatientHistories).WithOne(x => x.Patient)
                    .HasForeignKey(x => x.PatientId)
                    .OnDelete(DeleteBehavior.NoAction);

                builder.HasMany(x => x.PatientDoctorAccess)
                    .WithOne(x => x.Patient)
                    .HasForeignKey(x => x.PatientId)
                    .OnDelete(DeleteBehavior.NoAction);

            }
        }
        public class DoctorConfigrations : IEntityTypeConfiguration<Doctor>
        {
            public void Configure(EntityTypeBuilder<Doctor> builder)
            {
                builder.HasOne(x => x.User).WithOne()
                    .HasForeignKey<Doctor>(d => d.DoctorId);

                builder.HasMany(x => x.PatientHistories).WithOne(x => x.Doctor)
                    .HasForeignKey(x => x.DoctorId)
                    .OnDelete(DeleteBehavior.NoAction);

                builder.HasMany(x => x.PatientDoctorAccess)
                   .WithOne(x => x.Doctor)
                   .HasForeignKey(x => x.DoctorId)
                   .OnDelete(DeleteBehavior.NoAction);


            }
        }

        public class PatientHistoryConfigrations : IEntityTypeConfiguration<PatientHistory>
        {
            public void Configure(EntityTypeBuilder<PatientHistory> builder)
            {
                builder.Property(x => x.Date).HasDefaultValueSql("getdate()");

            }
        }


        public class UserCodeConfigrations : IEntityTypeConfiguration<UserCode>
        {
            public void Configure(EntityTypeBuilder<UserCode> builder)
            {
                builder.HasOne(x => x.User)
                    .WithMany(x => x.UserCodes)
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

            }
        }
        public class UserPhoneNumberConfigrations : IEntityTypeConfiguration<UserPhoneNumber>
        {
            public void Configure(EntityTypeBuilder<UserPhoneNumber> builder)
            {
                builder.HasOne(up => up.User)
                   .WithMany(u => u.PhoneNumbers)
                   .HasForeignKey(up => up.UserId);
            }
        }
    }
}

    
