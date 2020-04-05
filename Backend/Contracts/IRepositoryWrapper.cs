using System;
using System.Collections.Generic;
using System.Text;

namespace Contracts
{
    public interface IRepositoryWrapper
    {
        IAppUserRepository User { get; }
        IUniversityRepository University { get; }
        IGroupRepository Group { get; }
        IUserToGroupRepository UserToGroup { get; }
        void Save();
    }
}
