using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AppoinmentManagement;
using AppoinmentManagement.Models;

namespace AppoinmentManagement.Controllers
{
    [Route("api/appointment")]
    [ApiController]
    public class AppointmentController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AppointmentController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Appointment
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Appointment>>> GetAppointments()
        {
            if(_context.Appointments == null) {  return NotFound("No data found"); }
            return await _context.Appointments.Where(e=>!e.Deleted&&!e.Done).ToListAsync();
        }

        // GET: api/Appointment/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Appointment>> GetAppointment(int id)
        {
            if (_context.Appointments == null) { return NotFound("No data found"); }
            var appointment = await _context.Appointments.FindAsync(id);

            if (appointment == null)
            {
                return NotFound("No data found");
            }

            return appointment;
        }

        // PUT: api/Appointment/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAppointment(int id, Appointment appointment)
        {
            if (id != appointment.ID)
            {
                return BadRequest("You are trying to modify the wrong appoinment.");
            }

            _context.Entry(appointment).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AppointmentExists(id))
                {
                    return NotFound($"The appointment with id {id} does not exist");
                }
                else
                {
                    throw;
                }
            }

            return Ok("Appoinment updated successfully");
        }

        [HttpPost("filters")]
        public async Task<ActionResult<IEnumerable<Appointment>>> FilteredAppointments(Filter filters)
        {
            if (_context.Appointments == null)
            {
                return NotFound("No Data Found!");
            }

            List<Appointment> allData = await _context.Appointments.ToListAsync();

            if (filters.All)
            {
                return allData;
            }

            if (filters.LevelOfImportance != null)
            {
                allData = allData.Where(e => e.LevelOfImportance == filters.LevelOfImportance).ToList();
            }

            if (filters.SpecifiedDate != null)
            {
                allData = allData.Where(e => e.Date == filters.SpecifiedDate).ToList();
            }

            if (filters.StartDate != null && filters.EndDate != null)
            {
                allData = allData.Where(e => e.Date >= filters.StartDate && e.Date <= filters.EndDate).ToList();
            }

            if (filters.SpecifiedTime != null)
            {
                allData = allData.Where(e => e.Time == filters.SpecifiedTime).ToList();
            }

            allData = allData.Where(e => e.Done == filters.Done).ToList();
            allData = allData.Where(e => e.Deleted == filters.Deleted).ToList();

            return allData;
        }


        // POST: api/Appointment
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Appointment>> PostAppointment(Appointment appointment)
        {
            if (_context.Appointments == null) { return Problem("Not found"); }
            try {
                _context.Appointments.Add(appointment);
                await _context.SaveChangesAsync();
            }

            catch (DbUpdateConcurrencyException e)
            {
                return BadRequest("Could not create the new appointment");
            }
            return CreatedAtAction("GetAppointment", new { id = appointment.ID }, appointment);
        }

        // DELETE: api/Appointment/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAppointment(int id)
        {
            if (_context.Appointments == null) { return NotFound("No data found"); }
            var appointment = await _context.Appointments.FirstAsync(e => e.ID == id);
            if (appointment == null)
            {
                return NotFound($"No appointment with id {id}");
            }

            Appointment entry_ = await _context.Appointments.FirstAsync(e => e.ID == appointment.ID);
            entry_.Deleted = true;
            entry_.ModifiedDate = DateTime.Now;
            await _context.SaveChangesAsync();

            return Ok("Appointment deleted successfully");
        }

        private bool AppointmentExists(int id)
        {
            return _context.Appointments.Any(e => e.ID == id);
        }
    }
}
