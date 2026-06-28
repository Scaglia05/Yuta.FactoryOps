using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Yuta.FactoryOps.Domain.Entities;
using Yuta.FactoryOps.Domain.Interfaces;
using Yuta.FactoryOps.Infrastructure.Data;

namespace Yuta.FactoryOps.Infrastructure.Repositories
{
    public class EmpresaRepository : IEmpresaRepository
    {
        private readonly FactoryDbContext _context;

        public EmpresaRepository(FactoryDbContext context)
        {
            _context = context;
        }

        public async Task<Empresa?> GetByIdAsync(int id)
        {
            return await _context.Empresas.FindAsync(id);
        }

        public async Task<IEnumerable<Empresa>> GetAllAsync()
        {
            return await _context.Empresas.ToListAsync();
        }

        public async Task<IEnumerable<Empresa>> FindAsync(Expression<Func<Empresa, bool>> predicate)
        {
            return await _context.Empresas.Where(predicate).ToListAsync();
        }

        public async Task AddAsync(Empresa entity)
        {
            await _context.Empresas.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Empresa entity)
        {
            _context.Empresas.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Empresa entity)
        {
            _context.Empresas.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<Empresa?> ObterPorCnpjAsync(string cnpj)
        {
            if (string.IsNullOrWhiteSpace(cnpj)) return null;

            return await _context.Empresas
                .FirstOrDefaultAsync(e => e.Cnpj == cnpj);
        }
    }
}