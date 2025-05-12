using apbd_test1.Exceptions;
using apbd_test1.Models.DTOs;
using Microsoft.Data.SqlClient;

namespace apbd_test1.Services;

public class DbService : IDbService
{
    private readonly string _connectionString;
    
    public DbService(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<AppointmentDetailsDTO> GetAppointmentDetailsByIdAsync(int appointmentId)
    {
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
        
        var cmd = new SqlCommand(@"
            SELECT a.date, p.first_name, p.last_name, p.date_of_birth, d.doctor_id, d.PWZ,
                   s.name, s.base_fee
            FROM Appointment a
            JOIN Patient p ON a.PatientId = p.PatientId
            JOIN Doctor d ON a.DoctorId = d.DoctorId
            JOIN Appointment_Service aps ON a.AppointmentId = aps.AppointmentId
            JOIN Services s ON aps.ServiceId = s.ServiceId
            WHERE a.AppointmentId = @id 
            ", conn);
        
        cmd.Parameters.AddWithValue("@id", appointmentId);
        
        using var reader = await cmd.ExecuteReaderAsync();
        
        if (!reader.HasRows)
            throw new NotFoundException("Appointment not found");

        var result = new AppointmentDetailsDTO()
        {
            AppointmentServices = new List<ServiceDTO>()
        };

        while (await reader.ReadAsync())
        {
            if (result.Date == default)
            {
                result.Date = reader.GetDateTime(0);
                result.Patient = new PatientDTO()
                {
                    FirstName = reader.GetString(1),
                    LastName = reader.GetString(2),
                    DateOfBirth = reader.GetDateTime(3)
                };
                result.Doctor = new DoctorDTO()
                {
                    DoctorId = reader.GetInt32(4),
                    PWZ = reader.GetString(5),
                };
            }

            result.AppointmentServices.Add(new ServiceDTO()
            {
                Name = reader.GetString(6),
                ServiceFee = reader.GetDecimal(7),
            });
        }
        
        return result;
    }

    public async Task AddAppointmentAsync(AddAppointmentDTO appointmentDTO)
    {
        using var conn = new SqlConnection(_connectionString);
        var cmd = new SqlCommand();
        
        cmd.Connection = conn;
        await conn.OpenAsync();

        using var trans = await conn.BeginTransactionAsync();
        cmd.Transaction = trans as SqlTransaction;

        try
        {
            // check if appointment with id already exists
            cmd.Parameters.Clear();
            cmd.CommandText = "SELECT 1 FROM Appointment WHERE appointmed_id = @id";
            cmd.Parameters.AddWithValue("@id", appointmentDTO.AppointmentId);
            var countAppointments = await cmd.ExecuteScalarAsync(); 
            if (countAppointments is not null)
                throw new ValidationException($"Appointment with ID - {appointmentDTO.AppointmentId} already exists.");
            
            // validate existance of a patient
            cmd.Parameters.Clear();
            cmd.CommandText = "SELECT 1 FROM Patient WHERE patient_id = @id";
            cmd.Parameters.AddWithValue("@id", appointmentDTO.PatientId);
            var countPatients = await cmd.ExecuteScalarAsync(); 
            if (countPatients is null)
                throw new ValidationException($"Patient with id - {appointmentDTO.PatientId} does not exist.");
            
            // get doctor id by pwz
            cmd.Parameters.Clear();
            cmd.CommandText = "SELECT doctor_id FROM Doctor WHERE PWZ = @pwz";
            cmd.Parameters.AddWithValue("@pwz", appointmentDTO.PWZ);
            object doctorIdObj = await cmd.ExecuteScalarAsync();
            if (doctorIdObj == null)
                throw new ValidationException($"Doctor with PWZ - {appointmentDTO.PWZ} does not exist.");
            // save doctor's id
            int doctorId = (int)doctorIdObj;
            
            // validate all services and get their ids
            var serviceIds = new List<int>();
            foreach (var service in appointmentDTO.Services)
            {
                cmd.Parameters.Clear();
                cmd.CommandText = "SELECT service_id FROM Service WHERE name = @name AND base_fee = @fee";
                cmd.Parameters.AddWithValue("@name", service.Name);
                cmd.Parameters.AddWithValue("@fee", service.ServiceFee);
                object serviceIdObj = await cmd.ExecuteScalarAsync();
                if (serviceIdObj == null)
                    throw new ValidationException($"Service '{service.Name}' with fee {service.ServiceFee} does not exist.");
                // save service id 
                serviceIds.Add((int)serviceIdObj);
            }
            
            // insert new appointment
            cmd.Parameters.Clear();
            cmd.CommandText =
                "INSERT INTO Appointment (appointment_id, date, patient_id, doctor_id) VALUES (@id, @date, @patientId, @doctorId)";
            cmd.Parameters.AddWithValue("@id", appointmentDTO.AppointmentId);
            cmd.Parameters.AddWithValue("@date", DateTime.Now); 
            cmd.Parameters.AddWithValue("@patientId", appointmentDTO.PatientId);
            cmd.Parameters.AddWithValue("@doctorId", doctorId);
            await cmd.ExecuteNonQueryAsync();

            // insert into Appointment_Service
            foreach (var serviceId in serviceIds)
            {
                cmd.CommandText = "INSERT INTO Appointment_Service (appointment_id, service_id) VALUES (@appointmentId, @serviceId)";
                cmd.Parameters.AddWithValue("@appointmentId", appointmentDTO.AppointmentId);
                cmd.Parameters.AddWithValue("@serviceId", serviceId);
                await cmd.ExecuteNonQueryAsync();
            }
            
            await trans.CommitAsync();
        }
        catch (Exception e)
        {
            await trans.RollbackAsync();
            throw;
        }

    }
}