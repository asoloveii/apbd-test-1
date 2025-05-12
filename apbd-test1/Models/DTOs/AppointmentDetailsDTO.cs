namespace apbd_test1.Models.DTOs;

public class AppointmentDetailsDTO
{
    public DateTime Date { get; set; }
    public PatientDTO Patient { get; set; }
    public DoctorDTO Doctor { get; set; }
    public List<ServiceDTO> AppointmentServices { get; set; }
}

public class PatientDTO
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime DateOfBirth { get; set; }
}

public class DoctorDTO
{
    public int DoctorId { get; set; }
    public string PWZ { get; set; }
}

public class ServiceDTO
{
    public int Name { get; set; }
    public decimal ServiceFee { get; set; }
}
