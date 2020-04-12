namespace Contracts
{
    public interface IRepositoryWrapper
    {
        IAppUserRepository User { get; }
        IUniversityRepository University { get; }
        IGroupRepository Group { get; }
        IUserToGroupRepository UserToGroup { get; }
        ISubgroupRepository Subgroup { get; }
        IUserToSubgroupRepository UserToSubgroup { get; }
        void Save();
    }
}
