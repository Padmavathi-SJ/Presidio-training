using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.Domain.Entities.CropMonitoring;
using AgriculturePlatform.Infrastructure.Context;

namespace AgriculturePlatform.Infrastructure.Repositories
{
    public class DiseaseRepository : IDiseaseRepository
    {
        private readonly AppDbContext _context;

        public DiseaseRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<DiseaseAnalysisEntity> CreateAsync(DiseaseAnalysisEntity entity)
        {
            entity.CreatedAt = DateTime.UtcNow;

            // Safe fallback for demo purposes if FieldId doesn't exist
            var fieldExists = await _context.Fields.AnyAsync(f => f.Id == entity.FieldId);
            if (!fieldExists) 
            {
                var validField = await _context.Fields.FirstOrDefaultAsync();
                if (validField != null) 
                {
                    entity.FieldId = validField.Id;
                    entity.FarmId = validField.FarmId;
                }
            }

            await _context.DiseaseAnalyses.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<DiseaseAnalysisEntity?> GetByIdAsync(int id)
        {
            return await _context.DiseaseAnalyses
                .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);
        }

        public async Task<List<DiseaseAnalysisEntity>> GetByFarmIdAsync(int farmId)
        {
            return await _context.DiseaseAnalyses
                .Where(d => d.FarmId == farmId && !d.IsDeleted)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<DiseaseAnalysisEntity>> GetByUserIdAsync(int userId)
        {
            return await _context.DiseaseAnalyses
                .Where(d => d.CreatedBy == userId && !d.IsDeleted)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();
        }

        public async Task UpdateAsync(DiseaseAnalysisEntity entity)
        {
            entity.UpdatedAt = DateTime.UtcNow;
            _context.DiseaseAnalyses.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                entity.IsDeleted = true;
                entity.DeletedAt = DateTime.UtcNow;
                _context.DiseaseAnalyses.Update(entity);
                await _context.SaveChangesAsync();
            }
        }
    }
}
