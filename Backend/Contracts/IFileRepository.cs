﻿using Entities.Models;
using System;
using System.Collections.Generic;

namespace Contracts
{
    public interface IFileRepository : IRepositoryBase<File>
    {
        void PostFile(File file);
        void UpdateFile(File file);
        IEnumerable<File> GetGroupFiles(int groupId);
        IEnumerable<File> GetSubGroupFiles(int groupId);
    }
}
