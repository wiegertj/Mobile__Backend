using Entities.Models;
using System.Collections.Generic;

namespace Contracts
{
    public interface IUniversityRepository : IRepositoryBase<University>
    {
        IEnumerable<University> GetUniversities();
        bool CheckIfExisting(long id);
    }
}
