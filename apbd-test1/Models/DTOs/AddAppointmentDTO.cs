namespace apbd_test1.Models.DTOs;

public class AddAppointmentDTO
{
    public int AppointmentId { get; set; }
    public int PatientId { get; set; }
    public string PWZ { get; set; }
    public List<ServiceDTO> Services { get; set; }
}