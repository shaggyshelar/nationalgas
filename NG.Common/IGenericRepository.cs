using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace NG.Common
{
    public interface IGenericRepository<TEntity> where TEntity : class
    {
        IEnumerable<TEntity> All();

        IQueryable<TEntity> Query();

        IEnumerable<TEntity> AllInclude(params Expression<Func<TEntity, object>>[] includeProperties);

        void Delete(Guid id);

        IEnumerable<TEntity> FindBy(Expression<Func<TEntity, bool>> predicate);
        IEnumerable<TEntity> FindByInclude(Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] includeProperties);

        TEntity FindByKey(Guid id);

        void Insert(TEntity entity);

        bool Update(TEntity entity);
    }
}