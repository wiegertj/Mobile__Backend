using System;
using Contracts;
using Entities;
using Entities.Models;
using System.Collections.Generic;
using System.Linq;

namespace Repository
{
    public class FileRepository : RepositoryBase<File>, IFileRepository
    {
        public FileRepository(RepositoryContext context) : base(context) { }
        
        public void PostFile(File file)
        {
            Create(file);
        }

        public void UpdateFile(File file)
        {
            var oldFile = FindByCondition(uts => uts.Id.Equals(file.Id)).First();
            if (!oldFile.Path.Equals(file.Path)) { throw new ArgumentException("Path cant change with update."); }

            Update(file);
        }

        public IEnumerable<File> GetGroupFiles(int groupId)
        {
            return FindByCondition(uts => uts.NormalGroup == groupId).ToList();
        }

        public IEnumerable<File> GetSubGroupFiles(int groupId)
        {
            return FindByCondition(uts => uts.Subgroup == groupId).ToList();
        }
    }
}
