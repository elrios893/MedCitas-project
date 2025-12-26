using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MedCitas.Core.Entities;
using MedCitas.Core.Interfaces;
using MedCitas.Infrastructure.DataDb;
using Microsoft.EntityFrameworkCore;

namespace MedCitas.Infrastructure.Repositories
{
    public class EfSpecialtyRepository : ISpecialtyRepository
    {
   private readonly MedCitasDbContext _db;

        public EfSpecialtyRepository(MedCitasDbContext db) => _db = db;

        public async Task<Specialty?> ObtenerPorIdAsync(Guid id) =>
      await _db.Specialties.FirstOrDefaultAsync(s => s.Id == id);

  public async Task<List<Specialty>> ObtenerTodasAsync() =>
       await _db.Specialties
          .Where(s => s.EstaActiva)
    .OrderBy(s => s.Nombre)
             .ToListAsync();
    }
}
