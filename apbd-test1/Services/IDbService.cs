using apbd_test1.Models.DTOs;

namespace apbd_test1.Services;

public interface IDbService
{
    Task<AppointmentDetailsDTO> GetAppointmentDetailsByIdAsync(int id);
    Task AddAppointmentAsync(AddAppointmentDTO appointmentDTO);
}