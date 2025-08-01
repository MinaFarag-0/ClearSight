﻿using ClearSight.Core.Interfaces.Repository;
using ClearSight.Core.Models;
using ClearSight.Infrastructure.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClearSight.Infrastructure.Implementations.Repositories
{
    public class DoctorReposatory : BaseRepository<Doctor>, IDoctorReposatory
    {
        public DoctorReposatory(AppDbContext context) : base(context)
        {
        }
    }
}
