using Entities.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Contracts
{
    public interface IUniversityRepository : IRepositoryBase<University>
    {
        IEnumerable<University> GetUniversities();
        bool CheckIfExisting(long id);
    }
}
