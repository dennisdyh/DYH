using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DYH.Core.Data
{
    public interface IReponsitory<TEntity, in TPrimaryKey> : IQuery<TEntity, TPrimaryKey>
        where TEntity : BaseEntity<TPrimaryKey>
        where TPrimaryKey : struct
    {
        bool Insert(TEntity entity);
        bool Update(TEntity entity);
        bool Delete(TEntity entity);
    }
}
