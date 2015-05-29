using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using DYH.Core.Data;

namespace DYH.Framework.Data
{
    public class Repository<TEnity, TPrimaryKey> : IReponsitory<TEnity, TPrimaryKey>
        where TEnity : BaseEntity<TPrimaryKey>
        where TPrimaryKey : struct
    {

        private readonly UnitOfWork _uow;
        public Repository(IUnitOfWork uow)
        {
            _uow = uow as UnitOfWork;
        }

        public bool Insert(TEnity entity)
        {
            _uow.Set<TEnity>().Add(entity);
            return true;
        }

        public bool Update(TEnity entity)
        {
            _uow.Set<TEnity>().Attach(entity);
            _uow.Entry(entity).State = EntityState.Modified;
            return true;
        }

        public bool Delete(TEnity entity)
        {
            _uow.Set<TEnity>().Remove(entity);
            return true;
        }

        public TEnity GetById(TPrimaryKey id)
        {
            return _uow.Set<TEnity>().FirstOrDefault<TEnity>(x => x.Id.Equals(id));
        }

        public IQueryable<TEnity> FindAll(Expression<Func<TEnity, bool>> predicate)
        {
            return _uow.Set<TEnity>().Where(predicate);
        }
    }
}
