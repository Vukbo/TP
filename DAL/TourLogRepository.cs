using Microsoft.EntityFrameworkCore;
using TourPlanner.Entities;
using TourPlanner.Repositories;

namespace TourPlanner.DAL;

public class TourLogRepository : BaseRepository
{
    public TourLogRepository(AppDBC dbc) : base(dbc)
    {
    }

    public virtual async Task<List<TourLog>> GetAllTourLog(int tourId)
    {
        return await dbc.TourLogs.Where(tl => tl.TourId == tourId).ToListAsync();
    }

    public virtual async Task<TourLog> AddTourLog(TourLog tourLog)
    {
        await dbc.TourLogs.AddAsync(tourLog);
        await dbc.SaveChangesAsync();
        return tourLog;
    }

    public virtual async Task<TourLog> UpdateTourLog(TourLog tourLog)
    {
        dbc.TourLogs.Update(tourLog);
        await dbc.SaveChangesAsync();
        return tourLog;
    }

    public virtual async Task DeleteTourLog(int tourLogId, int tourId)
    {
        var tourLog = await dbc.TourLogs.Where(t => t.Id == tourLogId && t.TourId == tourId ).FirstOrDefaultAsync();
        
        if (tourLog == null)
        {
            throw new Exception("TourLog not found");
        }
        
        dbc.TourLogs.Remove(tourLog);
        await dbc.SaveChangesAsync();
    }
}