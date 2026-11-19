using Microsoft.EntityFrameworkCore;
using StadiumReservationApi.Data;
using StadiumReservationApi.Models;

namespace StadiumReservationApi.Services;

public class FeatureService
{
    private readonly AppDbContext _context;
    public FeatureService(AppDbContext context) { _context = context; }
    public Task<List<Feature>> GetAllAsync() => _context.Features.AsNoTracking().OrderBy(f => f.Name).ToListAsync();
    public async Task<Feature> AddAsync(string name)
    {
        var feature = new Feature { Name = name.Trim() };
        _context.Features.Add(feature);
        await _context.SaveChangesAsync();
        return feature;
    }
    public async Task<bool> UpdateAsync(int id, string name)
    {
        var feature = await _context.Features.FindAsync(id);
        if (feature == null) return false;
        feature.Name = name.Trim();
        await _context.SaveChangesAsync();
        return true;
    }
    public async Task<bool> DeleteAsync(int id)
    {
        var feature = await _context.Features.FindAsync(id);
        if (feature == null) return false;
        _context.Features.Remove(feature);
        await _context.SaveChangesAsync();
        return true;
    }
}
