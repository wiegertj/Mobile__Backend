using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.Extensions
{
    public static class EntityExtensions
    {
        public static bool IsObjectNull(this IEntity entity)
        {
            return entity == null;
        }

        public static bool IsEmptyObject(this IEntity entity)
        {
            return entity.Id.Equals(0);
        }
    }
}
