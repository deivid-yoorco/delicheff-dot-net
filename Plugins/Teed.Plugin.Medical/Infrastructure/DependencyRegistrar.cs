using Autofac;
using Autofac.Core;
using Nop.Core;
using Nop.Core.Configuration;
using Nop.Core.Data;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Data;
using Nop.Services.Customers;
using Nop.Web.Framework.Infrastructure;
using Teed.Plugin.Medical.Data;
using Teed.Plugin.Medical.Domain;
using Teed.Plugin.Medical.Domain.Appointment;
using Teed.Plugin.Medical.Domain.Branches;
using Teed.Plugin.Medical.Domain.Doctor;
using Teed.Plugin.Medical.Domain.Note;
using Teed.Plugin.Medical.Domain.Patients;
using Teed.Plugin.Medical.Domain.Prescription;
using Teed.Plugin.Medical.Domain.Visit;
using Teed.Plugin.Medical.Services;

namespace Teed.Plugin.Medical.Infrastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        private const string CONTEXT_NAME = "nop_object_context_medical";
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        {
            builder.RegisterType<PatientService>().InstancePerLifetimeScope();
            builder.RegisterType<DoctorPatientService>().InstancePerLifetimeScope();
            builder.RegisterType<PrescriptionService>().InstancePerLifetimeScope();
            builder.RegisterType<PrescriptionItemService>().InstancePerLifetimeScope();
            builder.RegisterType<CustomerService>().InstancePerLifetimeScope();
            builder.RegisterType<AppointmentService>().InstancePerLifetimeScope();
            builder.RegisterType<AppointmentItemService>().InstancePerLifetimeScope();
            builder.RegisterType<DoctorScheduleService>().InstancePerLifetimeScope();
            builder.RegisterType<VisitService>().InstancePerLifetimeScope();
            builder.RegisterType<VisitProductService>().InstancePerLifetimeScope();
            builder.RegisterType<VisitExtraUsersService>().InstancePerLifetimeScope();
            builder.RegisterType<NoteService>().InstancePerLifetimeScope();
            builder.RegisterType<DoctorLockedDatesServicecs>().InstancePerLifetimeScope();
            builder.RegisterType<DoctorAppointmentIntervalService>().InstancePerLifetimeScope();
            builder.RegisterType<PatientFileService>().InstancePerLifetimeScope();

            builder.RegisterType<BranchService>().InstancePerLifetimeScope();
            builder.RegisterType<HolidayService>().InstancePerLifetimeScope();
            builder.RegisterType<HolidayBranchService>().InstancePerLifetimeScope();
            builder.RegisterType<OfficeService>().InstancePerLifetimeScope();
            builder.RegisterType<BranchWorkerService>().InstancePerLifetimeScope();

            this.RegisterPluginDataContext<MedicalObjectContext>(builder, CONTEXT_NAME);

            Register<Patient>(builder);
            Register<PatientFile>(builder);
            Register<DoctorPatient>(builder);
            Register<Prescription>(builder);
            Register<PrescriptionItem>(builder);
            Register<Appointment>(builder);
            Register<AppointmentItem>(builder);
            Register<DoctorSchedule>(builder);
            Register<Visit>(builder);
            Register<VisitProduct>(builder);
            Register<AppointmentExtraUsers>(builder);
            Register<Note>(builder);
            Register<VisitExtraUsers>(builder);
            Register<DoctorLockedDates>(builder);
            Register<DoctorAppointmentInterval>(builder);

            Register<Branch>(builder);
            Register<Holiday>(builder);
            Register<HolidayBranch>(builder);
            Register<Office>(builder);
            Register<BranchWorker>(builder);
        }

        private void Register<T>(ContainerBuilder builder) where T : BaseEntity
        {
            builder.RegisterType<EfRepository<T>>()
            .As<IRepository<T>>()
            .WithParameter(ResolvedParameter.ForNamed<IDbContext>(CONTEXT_NAME))
            .InstancePerLifetimeScope();
        }

        public int Order
        {
            get { return 1; }
        }
    }
}
