using AutoMapper;
using TourPlanner.API.DTO;
using TourPlanner.DAL;
using TourPlanner.Entities;

namespace TourPlanner.Services;

public class TourLogService
{
    private readonly TourLogRepository _logRepo;
    private readonly IMapper _mapper;

    public TourLogService(TourLogRepository repo, IMapper mapper)
    {
        _logRepo = repo;
        _mapper = mapper;
    }

    // Create tour log
    public async Task<TourLogDTO> AddTourLog(TourLogDTO tourLogDto)
    {
        var tl = _mapper.Map<TourLog>(tourLogDto);
        var tlReturn = await _logRepo.AddTourLog(tl);
        var dto = _mapper.Map<TourLogDTO>(tlReturn);
        return dto;
    }

    public async Task<TourLogDTO> UpdateTourLog(TourLogDTO tourLog, int tourLogId)
    {
        tourLog.Id = tourLogId;
        var tl = _mapper.Map<TourLog>(tourLog);
        var tlNew = await _logRepo.UpdateTourLog(tl);
        var dto = _mapper.Map<TourLogDTO>(tlNew);
        return dto;
    }

    // Delete tour log
    public async Task DeleteTourLog(int tourLogId, TourLogDeleteDTO tourId)
    {
        var tId = tourId.TourId;
        await _logRepo.DeleteTourLog(tourLogId, tId);
    }
}