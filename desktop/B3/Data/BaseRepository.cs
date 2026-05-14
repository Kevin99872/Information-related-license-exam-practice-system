using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace B3.Data;

/// <summary>
/// 通用Repository基類 - 消除CRUD重複代碼
/// 職責: 提供通用的查詢、新增、修改、刪除操作
/// </summary>
public abstract class BaseRepository<TEntity> where TEntity : class
{
    protected readonly ExamDbContext Context;

    protected BaseRepository(ExamDbContext context)
    {
        Context = context;
    }

    /// <summary>取得所有記錄</summary>
    public virtual async Task<List<TEntity>> GetAllAsync()
    {
        return await Context.Set<TEntity>().ToListAsync();
    }

    /// <summary>根據條件查詢</summary>
    public virtual async Task<List<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await Context.Set<TEntity>().Where(predicate).ToListAsync();
    }

    /// <summary>根據ID查詢單一記錄</summary>
    public virtual async Task<TEntity?> GetByIdAsync(int id)
    {
        return await Context.Set<TEntity>().FindAsync(id);
    }

    /// <summary>新增記錄</summary>
    public virtual async Task<TEntity> AddAsync(TEntity entity)
    {
        Context.Set<TEntity>().Add(entity);
        await Context.SaveChangesAsync();
        return entity;
    }

    /// <summary>批量新增</summary>
    public virtual async Task<List<TEntity>> AddMultipleAsync(List<TEntity> entities)
    {
        Context.Set<TEntity>().AddRange(entities);
        await Context.SaveChangesAsync();
        return entities;
    }

    /// <summary>更新記錄</summary>
    public virtual async Task<TEntity> UpdateAsync(TEntity entity)
    {
        Context.Set<TEntity>().Update(entity);
        await Context.SaveChangesAsync();
        return entity;
    }

    /// <summary>刪除記錄</summary>
    public virtual async Task DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            Context.Set<TEntity>().Remove(entity);
            await Context.SaveChangesAsync();
        }
    }

    /// <summary>刪除指定記錄</summary>
    public virtual async Task DeleteAsync(TEntity entity)
    {
        Context.Set<TEntity>().Remove(entity);
        await Context.SaveChangesAsync();
    }

    /// <summary>批量刪除</summary>
    public virtual async Task DeleteMultipleAsync(List<TEntity> entities)
    {
        Context.Set<TEntity>().RemoveRange(entities);
        await Context.SaveChangesAsync();
    }

    /// <summary>計算符合條件的記錄數</summary>
    public virtual async Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null)
    {
        var query = Context.Set<TEntity>().AsQueryable();
        if (predicate != null)
        {
            query = query.Where(predicate);
        }
        return await query.CountAsync();
    }

    /// <summary>檢查是否存在符合條件的記錄</summary>
    public virtual async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await Context.Set<TEntity>().AnyAsync(predicate);
    }
}
