using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DYH.Core.Data
{
    public interface IQuery<TEntity, in TPrimaryKey>
    {
        TEntity GetById(TPrimaryKey id);
        IQueryable<TEntity> FindAll(Expression<Func<TEntity, bool>> predicate);
    }
}
