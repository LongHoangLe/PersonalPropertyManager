using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PersonalPropertyManager.Data;
using PersonalPropertyManager.Models;

namespace PersonalPropertyManager.Services;

/// <summary>
/// CRUD service over the SQLite-backed PropertyDbContext.
/// </summary>
public class PropertyService : IPropertyService
{
    private readonly PropertyDbContext _context;

    public PropertyService(PropertyDbContext context)
    {
        _context = context;
    }

    public async Task<List<PersonalProperty>> GetAllAsync()
    {
        return await _context.Properties
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<PersonalProperty?> GetByIdAsync(int id)
    {
        return await _context.Properties.FindAsync(id);
    }

    public async Task<PersonalProperty> AddAsync(PersonalProperty item)
    {
        item.CreatedAt = DateTime.Now;
        item.UpdatedAt = DateTime.Now;
        _context.Properties.Add(item);
        await _context.SaveChangesAsync();
        return item;
    }

    public async Task UpdateAsync(PersonalProperty item)
    {
        var existing = await _context.Properties.FindAsync(item.Id);
        if (existing is null)
            throw new InvalidOperationException($"Property with id {item.Id} was not found.");

        existing.Name = item.Name;
        existing.Description = item.Description;
        existing.CurrentValue = item.CurrentValue;
        existing.ImagePath = item.ImagePath;
        existing.DesireStatus = item.DesireStatus;
        existing.ItemType = item.ItemType;
        existing.AcquiredDate = item.AcquiredDate;
        existing.Notes = item.Notes;
        existing.Location = item.Location;
        existing.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();
        // Caller is holding a reference to `item` (a UI-bound copy); refresh from DB so the grid reflects the change.
        item.CreatedAt = existing.CreatedAt;
        item.UpdatedAt = existing.UpdatedAt;
    }

    public async Task DeleteAsync(int id)
    {
        var existing = await _context.Properties.FindAsync(id);
        if (existing is null) return;
        _context.Properties.Remove(existing);
        await _context.SaveChangesAsync();
    }

    public async Task<List<PersonalProperty>> SearchAsync(string? searchText, ItemType? type, DesireStatus? status)
    {
        IQueryable<PersonalProperty> query = _context.Properties.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            var s = searchText.Trim().ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(s) ||
                (p.Description != null && p.Description.ToLower().Contains(s)) ||
                (p.Notes != null && p.Notes.ToLower().Contains(s)) ||
                (p.Location != null && p.Location.ToLower().Contains(s)));
        }

        if (type.HasValue)
            query = query.Where(p => p.ItemType == type.Value);

        if (status.HasValue)
            query = query.Where(p => p.DesireStatus == status.Value);

        return await query.OrderBy(p => p.Name).ToListAsync();
    }

    public async Task<decimal> GetTotalValueAsync()
    {
        return await _context.Properties.SumAsync(p => (decimal?)p.CurrentValue) ?? 0m;
    }
}