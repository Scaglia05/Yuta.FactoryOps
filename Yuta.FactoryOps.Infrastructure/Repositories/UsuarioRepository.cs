using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Yuta.FactoryOps.Domain.Entities;
using Yuta.FactoryOps.Domain.Interfaces;
using Yuta.FactoryOps.Infrastructure.Data;

namespace Yuta.FactoryOps.Infrastructure.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly FactoryDbContext _context;

        public UsuarioRepository(FactoryDbContext context)
        {
            _context = context;
        }

        public async Task<Usuario?> GetByIdAsync(int id)
        {
            return await _context.Usuarios.FindAsync(id);
        }

        public async Task<IEnumerable<Usuario>> GetAllAsync()
        {
            return await _context.Usuarios.ToListAsync();
        }

        public async Task<IEnumerable<Usuario>> FindAsync(Expression<Func<Usuario, bool>> predicate)
        {
            return await _context.Usuarios.Where(predicate).ToListAsync();
        }

        public async Task AddAsync(Usuario entity)
        {
            await _context.Usuarios.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Usuario entity)
        {
            _context.Usuarios.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Usuario entity)
        {
            _context.Usuarios.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<Usuario?> ObterPorEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;

            return await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower().Trim());
        }
    }
}