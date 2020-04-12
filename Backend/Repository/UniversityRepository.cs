using Contracts;
using Entities;
using Entities.Models;
using System.Collections.Generic;
using System.Linq;

namespace Repository
{
    public class UniversityRepository : RepositoryBase<University>, IUniversityRepository
    {
        public UniversityRepository(RepositoryContext repositoryContext) : base(repositoryContext)
        {

        }

        public bool CheckIfExisting(long id)
        { 
                var uniDb = FindByCondition(uni => uni.Id.Equals(id))
                    .FirstOrDefault();

                if (uniDb == null)
                {
                    return false;
                }

                return true;      
        }

        public IEnumerable<University> GetUniversities()
        {
            return FindAll()
                .OrderBy(uni => uni.Name)
                .ToList();
        }
    }
}
