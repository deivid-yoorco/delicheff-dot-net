using Nop.Core;
using Nop.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using Teed.Plugin.Medical.Data.Mapping;
using Teed.Plugin.Medical.Domain;
using Teed.Plugin.Medical.Domain.Appointment;
using Teed.Plugin.Medical.Domain.Branches;
using Teed.Plugin.Medical.Domain.Doctor;
using Teed.Plugin.Medical.Domain.Note;
using Teed.Plugin.Medical.Domain.Prescription;
using Teed.Plugin.Medical.Domain.Visit;

namespace Teed.Plugin.Medical.Data
{
    public class MedicalObjectContext : DbContext, IDbContext
    {
        public MedicalObjectContext(string nameOrConnectionString) : base(nameOrConnectionString) { }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new PatientMap());
            modelBuilder.Configurations.Add(new DoctorPatientMap());
            modelBuilder.Configurations.Add(new PrescriptionMap());
            modelBuilder.Configurations.Add(new PrescriptionItemMap());
            modelBuilder.Configurations.Add(new AppointmentMap());
            modelBuilder.Configurations.Add(new AppointmentItemMap());
            modelBuilder.Configurations.Add(new DoctorScheduleMap());
            modelBuilder.Configurations.Add(new VisitMap());
            modelBuilder.Configurations.Add(new VisitProductMap());
            modelBuilder.Configurations.Add(new AppointmentExtraUsersMap());
            modelBuilder.Configurations.Add(new NoteMap());
            modelBuilder.Configurations.Add(new VisitExtraUserMap());
            modelBuilder.Configurations.Add(new DoctorLockedDateMap());
            modelBuilder.Configurations.Add(new BranchMap());
            modelBuilder.Configurations.Add(new HolidayMap());
            modelBuilder.Configurations.Add(new HolidayBranchMap());
            modelBuilder.Configurations.Add(new OfficeMap());
            modelBuilder.Configurations.Add(new BranchWorkerMap());
            modelBuilder.Configurations.Add(new DoctorAppointmentIntervalMap());
            modelBuilder.Configurations.Add(new PatientFileMap()); //new


            base.OnModelCreating(modelBuilder);
        }
        public string CreateDatabaseScript()
        {
            return ((IObjectContextAdapter)this).ObjectContext.CreateDatabaseScript();
        }
        public new IDbSet<TEntity> Set<TEntity>() where TEntity : BaseEntity
        {
            return base.Set<TEntity>();
        }
        public void Install()
        {
            var dbScript = CreateDatabaseScript();
            Database.ExecuteSqlCommand(dbScript);
            SaveChanges();
        }

        public void Uninstall()
        {
            //drop the table

            //////DropTable<DoctorPatient>();
            //////DropTable<Prescription>();
            //////DropTable<PrescriptionItem>();
            //////DropTable<Appointment>();
            //////DropTable<AppointmentItem>();
            //////DropTable<Patient>();
            //////DropTable<DoctorSchedule>();
            //////DropTable<Visit>();
            //////DropTable<VisitProduct>();
            //////DropTable<AppointmentExtraUsers>();
            //////DropTable<Note>();
            //////DropTable<VisitExtraUsers>();
            //////DropTable<DoctorLockedDates>();
            //////DropTable<Branch>();
            //////DropTable<Holiday>();
            //////DropTable<HolidayBranch>();
            //////DropTable<Office>();
            //////DropTable<BranchWorker>();
            //////DropTable<DoctorAppointmentInterval>();
            //////DropTable<PatientFile>();
        }

        private void DropTable<T>() where T : BaseEntity
        {
            var tableName = this.GetTableName<T>();
            this.DropPluginTable(tableName);
        }

        public IList<TEntity> ExecuteStoredProcedureList<TEntity>(string commandText, params object[] parameters) where TEntity : BaseEntity, new()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TElement> SqlQuery<TElement>(string sql, params object[] parameters)
        {
            throw new NotImplementedException();
        }

        public int ExecuteSqlCommand(string sql, bool doNotEnsureTransaction = false, int? timeout = null, params object[] parameters)
        {
            throw new NotImplementedException();
        }
        public void Detach(object entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            ((IObjectContextAdapter)this).ObjectContext.Detach(entity);
        }

        public virtual bool ProxyCreationEnabled
        {
            get
            {
                return Configuration.ProxyCreationEnabled;
            }
            set
            {
                Configuration.ProxyCreationEnabled = value;
            }
        }

        public virtual bool AutoDetectChangesEnabled
        {
            get
            {
                return Configuration.AutoDetectChangesEnabled;
            }
            set
            {
                Configuration.AutoDetectChangesEnabled = value;
            }
        }
    }
}
