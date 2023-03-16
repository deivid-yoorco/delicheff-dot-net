using Nop.Services.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Medical.Domain;

namespace Teed.Plugin.Medical.Services
{
    public class PatientScheduleService : IScheduleTask
    {
        private readonly PatientService _patientService;
        public PatientScheduleService(PatientService patientService)
        {
            _patientService = patientService;
        }
        public void Execute()
        {
            var patients = _patientService.GetAllPatients().ToList();
            foreach (Patient item in patients)
            {
                if (item.UpdatePageActivationDateOnUtc.Value.AddDays(7).Date == DateTime.UtcNow.Date && item.UpdatePageActive)
                {
                   
                    item.UpdatePageActive = false;
                    _patientService.Update(item);
                }
            }
        }
    }
}
