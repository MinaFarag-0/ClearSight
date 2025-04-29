using ClearSight.Core.Interfaces.Repository;
using ClearSight.Core.Mosels;
using ClearSight.Infrastructure.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClearSight.Infrastructure.Implementations.Repositories
{
    public class PatientReposatory : BaseRepository<Patient>, IPatientReposatory
    {
        public PatientReposatory(AppDbContext context) : base(context)
        {
        }
    }
}
