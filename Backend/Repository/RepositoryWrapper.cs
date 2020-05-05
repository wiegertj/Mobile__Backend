using Contracts;
using Entities;

namespace Repository
{
    public class RepositoryWrapper : IRepositoryWrapper
    {
        private RepositoryContext _repoContext;

        private IAppUserRepository _user;
        private IUniversityRepository _universityRepository;
        private IGroupRepository _groupRepository;
        private IUserToGroupRepository _userToGroupRepository;
        private ISubgroupRepository _subgroupRepository;
        private IUserToSubgroupRepository _userToSubgroupRepository;
        private IDiscussionEntryRepository _discussionEntryRepository;
        private IFileRepository _fileRepository;

        public RepositoryWrapper(RepositoryContext repositoryContext)
        {
            _repoContext = repositoryContext;
        }

        public IAppUserRepository User
        {
            get
            {
                if(_user == null)
                {
                    _user = new UserRepository(_repoContext);
                }

                return _user;
            }
        }

        public IUniversityRepository University 
        {
            get
            {
                if (_universityRepository == null)
                {
                    _universityRepository = new UniversityRepository(_repoContext);
                }

                return _universityRepository;
            }
        }

        public IGroupRepository Group 
        {
            get
            {
                if (_groupRepository == null)
                {
                    _groupRepository = new GroupRepository(_repoContext);
                }

                return _groupRepository;
            }
        }

        public IUserToGroupRepository UserToGroup
        {
            get
            {
                if (_userToGroupRepository == null) {
                    _userToGroupRepository = new UserToGroupRepository(_repoContext);
                }

                return _userToGroupRepository;
            }
        }

        public ISubgroupRepository Subgroup
        {
            get
            {
                if (_subgroupRepository == null) {
                    _subgroupRepository = new SubgroupRepository(_repoContext);
                }

                return _subgroupRepository;
            }
        }

        public IUserToSubgroupRepository UserToSubgroup
        {
            get
            {
                if (_userToSubgroupRepository == null) {
                    _userToSubgroupRepository = new UserToSubgroupRepository(_repoContext);
                }

                return _userToSubgroupRepository;
            }
        }

        public IDiscussionEntryRepository DiscussionEntry
        {
            get
            {
                if (_discussionEntryRepository == null)
                {
                    _discussionEntryRepository = new DiscussionEntryRepository(_repoContext);
                }

                return _discussionEntryRepository;
            }
        }

        public IFileRepository File {
            get
            {
                if (_fileRepository == null)
                {
                    _fileRepository = new FileRepository(_repoContext);
                }

                return _fileRepository;
            }
        }
        
        public void Save()
        {
            _repoContext.SaveChanges();
        }
    }
}
