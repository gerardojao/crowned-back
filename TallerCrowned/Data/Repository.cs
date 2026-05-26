using FamilyApp.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace FamilyApp.Data
{
    public class Repository<TDbContext> : IRepository where TDbContext : DbContext
    {
        private readonly IConfiguration _appsettings;
        private readonly dbContext _context;
        private TDbContext _dbContext;

        public Repository(TDbContext context, IConfiguration appsettings, dbContext appcontext)
        {
            this._dbContext = context;
            this._appsettings = appsettings;
            this._context = appcontext;
        }

        public async Task<T> CreateAsync<T>(T entity) where T : class
        {
            _dbContext.Set<T>().Add(entity);
            await _dbContext.SaveChangesAsync();
            return entity; // EF ya rellenó la PK
        }


        public async Task DeleteAsync<T>(T entity) where T : class
        {
            this._dbContext.Set<T>().Update(entity);
            _ = await this._dbContext.SaveChangesAsync();
        }

        public async Task<T> SelectById<T>(int Id) where T : class
        {
            return await this._dbContext.Set<T>().FindAsync(Id);
        }

        public async Task UpdateAsync<T>(T entity) where T : class
        {
            this._dbContext.Set<T>().Update(entity);
            _ = await this._dbContext.SaveChangesAsync();
        }

        public async Task<List<T>> SelectAll<T>() where T : class
        {
            return await _dbContext.Set<T>().AsNoTracking().ToListAsync();
        }

    

    }
}
