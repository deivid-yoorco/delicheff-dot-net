using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Teed.Plugin.Payroll.Security;
using Teed.Plugin.Payroll.Services;
using Teed.Plugin.Payroll.Models.PayrollEmployee;
using Teed.Plugin.Payroll.Domain.PayrollEmployees;
using Nop.Web.Framework.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Teed.Plugin.Payroll.Models.JobCatalog;
using Teed.Plugin.Payroll.Domain.JobCatalogs;
using Common.Logging;
using Teed.Plugin.Payroll.Domain.Benefits;
using System.Text.RegularExpressions;

namespace Teed.Plugin.Payroll.Controllers
{
    [Area(AreaNames.Admin)]
    public class JobCatalogController : BasePluginController
    {
        private readonly IPermissionService _permissionService;
        private readonly PayrollEmployeeService _payrollEmployeeService;
        private readonly PayrollEmployeeFileService _payrollEmployeeFileService;
        private readonly PayrollSalaryService _payrollSalaryService;
        private readonly JobCatalogService _jobCatalogService;

        public JobCatalogController(IPermissionService permissionService, PayrollEmployeeService payrollEmployeeService,
            PayrollEmployeeFileService payrollEmployeeFileService, PayrollSalaryService payrollSalaryService,
            JobCatalogService jobCatalogService)
        {
            _permissionService = permissionService;
            _payrollEmployeeService = payrollEmployeeService;
            _payrollEmployeeFileService = payrollEmployeeFileService;
            _payrollSalaryService = payrollSalaryService;
            _jobCatalogService = jobCatalogService;
        }

        public IActionResult Index()
        {
            if (!_permissionService.Authorize(TeedPayrollPermissionProvider.JobCatalog))
                return AccessDeniedView();

            var jobs = _jobCatalogService.GetAll()
                .OrderBy(x => x.DisplayOrder)
                .ThenBy(x => x.Name)
                .ToList();
            var data = jobs.Select(x => new JobCatalogData
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                DisplayOrder = x.DisplayOrder,
                PayrollEmployees = _payrollEmployeeService.GetAllByJobCatalogId(x.Id)
                .Where(y => y.EmployeeStatusId != (int)EmployeeStatus.Discharged)
                .ToList()
            }).ToList();
            var model = new JobCatalogModel
            {
                JobCatalogs = data
            };
            model.OrganigramString = GetOrganigramString(jobs);

            var benefits = Enum.GetValues(typeof(BenefitType)).Cast<BenefitType>().ToList();
            var benefiteTypes = benefits.Select(x => new
            {
                Id = x.GetDisplayName(),
                Name = x.GetDisplayName()
            });
            model.BenefitTypes = new SelectList(benefiteTypes, "Id", "Name");
            model.Jobs = new SelectList(data, "Id", "Name");

            model.EmployeesWithoutJobAssigned = new List<SelectListItem>();
            var employees = _payrollEmployeeService.GetAll().ToList();
            var employeesUnnasigned = employees
                .Where(x => x.GetCurrentJob() == null).ToList();
            if (employeesUnnasigned.Any())
            {
                model.EmployeesWithoutJobAssigned = employeesUnnasigned.Select(x => new SelectListItem
                {
                    Text = x.GetFullName(),
                    Value = x.Id.ToString()
                }).ToList();
            }
            model.AddJobCatalog = new JobCatalogData();
            model.AddJobCatalog = CreateHoursSelect(model.AddJobCatalog);
            CreateMinutesSelect(model.AddJobCatalog);

            return View("~/Plugins/Teed.Plugin.Payroll/Views/JobCatalog/Index.cshtml", model);
        }

        private string GetOrganigramString(List<JobCatalog> jobCatalogs)
        {
            var final = string.Empty;
            var employees = _payrollEmployeeService.GetAll()
                .Where(y => y.EmployeeStatusId != (int)EmployeeStatus.Discharged)
                .ToList();
            jobCatalogs = jobCatalogs.OrderBy(x => x.ParentJobId).ToList();
            if (!jobCatalogs.Where(x => x.ParentJobId == null || x.ParentJobId == 0).Any())
                return "{}";
            var president = jobCatalogs.Where(x => x.ParentJobId == null || x.ParentJobId == 0).FirstOrDefault();
            final += string.Format(@"{{ 'id': '{0}',
            'name': '{1}',
            'title': '{2}',
            'relationship': {3},
            'children': [|{4}|] }}", president.Id, president.Name ?? "", DescriptionWithEmployees(president.Description, employees, president),
            "{ 'children_num': " + jobCatalogs.Where(x => x.ParentJobId == president.Id).Count() + " }", "job-" + president.Id);
            jobCatalogs.Remove(president);
            var jobCatalogGroup = new List<IGrouping<int?, JobCatalog>>();
            var count = 0;
            do
            {
                jobCatalogGroup = jobCatalogs.Where(x => x.ParentJobId != null && x.ParentJobId > 0)
                   .OrderBy(x => x.ParentJobId).GroupBy(x => x.ParentJobId).ToList();
                foreach (var group in jobCatalogGroup)
                {
                    var currentJobString = string.Empty;
                    var jobsOfGroup = group.ToList();
                    foreach (var job in jobsOfGroup)
                    {
                        var jobsInGroup = jobsOfGroup.Where(x => x.ParentJobId == job.Id).Count();
                        currentJobString += string.Format(@"{{ 'id': '{0}',
                    'name': '{1}',
                    'title': '{2}',
                    'children': [|{3}|] }},", job.Id, job.Name ?? "", DescriptionWithEmployees(job.Description, employees, job),
                        "job-" + job.Id);
                    }
                    if (!jobsOfGroup.Any())
                        currentJobString += "{}";
                    if (final.Contains("|job-" + group.Key + "|"))
                    {
                        final = final.Replace("|job-" + group.Key + "|", currentJobString);
                        foreach (var toRemove in jobsOfGroup)
                            jobCatalogs.Remove(toRemove);
                        count = 0;
                    }
                    else
                        count++;
                    if (count > 10)
                        foreach (var toRemove in jobsOfGroup)
                            jobCatalogs.Remove(toRemove);
                }
                count++;
                if (count > 10)
                    jobCatalogs = new List<JobCatalog>();
            } while (jobCatalogs.Any());
            if (final.Contains("|job-"))
            {
                Regex regex = new Regex("(\\|)(.*?)(\\|)");
                final = regex.Replace(final, "$1" + "" + "$3");
                final = final.Replace("||", "");
            }
            return final;
        }

        private string DescriptionWithEmployees(string description, List<PayrollEmployee> employees, JobCatalog job)
        {
            var final = "";
            //final += description?.Replace("", "");
            //if (!string.IsNullOrEmpty(description))
            //    final += Regex.Replace(description, @"(?:\r\n|\r|\n)", "");
            var employeesOfJob = employees.Where(x => (x.GetCurrentJob()?.Id ?? 0) == job.Id);
            if (employeesOfJob.Any())
                final += "<div style=\"display: grid;margin-top: 10px;\">";
            foreach (var employee in employeesOfJob)
            {
                final +=
                    $"<span>- <a target=\"blank\" href=\"{Url.Action("Edit", "PayrollEmployee")}/{employee.Id}\">{employee.GetFullName()}</a></span>";
            }
            if (employeesOfJob.Any())
                final += "</div>";

            return final;
        }

        [HttpPost]
        public IActionResult JobCatalogList(int exceptJobId = 0)
        {
            if (!_permissionService.Authorize(TeedPayrollPermissionProvider.JobCatalog))
                return AccessDeniedView();
            var jobs = _jobCatalogService.GetAll()
                .OrderBy(x => x.DisplayOrder)
                .ThenBy(x => x.Name)
                .ToList();
            var childJobs = new List<JobCatalog>();
            if (exceptJobId > 0)
            {
                jobs = jobs.Where(x => x.Id != exceptJobId).ToList();
                var job = _jobCatalogService.GetById(exceptJobId);
                if (job.ParentJobId.HasValue)
                    jobs = jobs.Where(x => x.Id != job.ParentJobId).ToList();
            }
            var gridModel = new DataSourceResult
            {
                Data = jobs.Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.Description,
                    x.DisplayOrder,
                    Salary = x.SalaryMin?.ToString("C") + "/" + x.SalaryMax?.ToString("C"),
                    EmployeeCount = _payrollEmployeeService.GetAllByJobCatalogId(x.Id)
                    .Where(y => y.EmployeeStatusId != (int)EmployeeStatus.Discharged).Count(),
                    ParentJob = GetParentJobName(x.ParentJobId, jobs),
                    WorkSchedule = CreateWorkScheduleString(x.WorkSchedule),
                    x.ParentJobId
                }).ToList(),
                Total = jobs.Count()
            };

            return Json(gridModel);
        }

        private string GetParentJobName(int? parentJobId, List<JobCatalog> jobCatalogs)
        {
            var job = jobCatalogs.Where(x => x.Id == parentJobId).FirstOrDefault();
            if (job != null)
                return job.Name;
            else
                return "Sin puesto padre";
        }

        [HttpPost]
        public IActionResult JobCatalogAdd(JobCatalogData model)
        {
            if (!_permissionService.Authorize(TeedPayrollPermissionProvider.JobCatalog))
                return AccessDeniedView();
            try
            {
                int? parent = null;
                if (model.JobParentId > 0)
                    parent = model.JobParentId;
                else
                    parent = null;
                var job = new JobCatalog
                {
                    Name = model.Name,
                    Description = model.Description,
                    DisplayOrder = model.DisplayOrder,
                    SalaryMin = model.SalaryMin,
                    SalaryMax = model.SalaryMax,
                    ParentJobId = parent,
                    WorkSchedule = model.WorkSchedule,
                    SubjectToWorkingHours = model.SubjectToWorkingHours,
                };
                _jobCatalogService.Insert(job);
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

        [HttpPost]
        public IActionResult JobCatalogUpdate(JobCatalogData model)
        {
            if (!_permissionService.Authorize(TeedPayrollPermissionProvider.JobCatalog))
                return AccessDeniedView();
            var job = _jobCatalogService.GetById(model.Id);
            if (job == null)
                return Ok();

            job.Name = model.Name;
            job.Description = model.Description;
            job.DisplayOrder = model.DisplayOrder;
            job.SalaryMin = model.SalaryMin;
            job.SalaryMax = model.SalaryMax;
            _jobCatalogService.Update(job);
            return Ok();
        }

        [HttpPost]
        public IActionResult JobCatalogUpdateParent(int parentId, int jobId)
        {
            if (!_permissionService.Authorize(TeedPayrollPermissionProvider.JobCatalog))
                return AccessDeniedView();
            var job = _jobCatalogService.GetById(jobId);
            if (job == null)
                return Ok();

            job.ParentJobId = parentId;
            _jobCatalogService.Update(job);
            return Ok();
        }

        [HttpPost]
        public IActionResult JobCatalogDelete(JobCatalogData model)
        {
            if (!_permissionService.Authorize(TeedPayrollPermissionProvider.JobCatalog))
                return AccessDeniedView();
            var job = _jobCatalogService.GetById(model.Id);
            if (job == null)
                return Ok();
            var jobsOfParent = _jobCatalogService.GetAllByParentId(job.Id);
            foreach (var jobOfParent in jobsOfParent)
            {
                jobOfParent.ParentJobId = null;
                _jobCatalogService.Update(job);
            }

            job.Deleted = true;
            _jobCatalogService.Update(job);
            return Ok();
        }

        public IActionResult Edit(int id)
        {
            var job = _jobCatalogService.GetById(id);
            if (job == null)
                return BadRequest("Job not found");
            var jobData = new JobCatalogData
            {
                Description = job.Description,
                DisplayOrder = job.DisplayOrder,
                Id = job.Id,
                JobParentId = job.ParentJobId ?? 0,
                Name = job.Name,
                SalaryMax = job.SalaryMax ?? 0,
                SalaryMin = job.SalaryMin ?? 0,
                WorkSchedule = job.WorkSchedule,
                SubjectToWorkingHours = job.SubjectToWorkingHours ?? false,
            };
            var jobs = _jobCatalogService.GetAll()
                .Where(x => x.Id != job.Id)
                .OrderBy(x => x.DisplayOrder)
                .ThenBy(x => x.Name)
                .ToList();

            jobData.JobParents = new List<SelectListItem>
            {
                new SelectListItem
                {
                    Value = "0",
                    Text = "Sin puesto padre",
                    Selected = 0 == job.ParentJobId
                }
            };
            foreach (var jobb in jobs)
            {
                jobData.JobParents.Add(new SelectListItem
                {
                    Value = jobb.Id.ToString(),
                    Text = jobb.Name,
                    Selected = jobb.Id == job.ParentJobId
                });
            }
            CreateHoursSelect(jobData);
            CreateMinutesSelect(jobData);
            if (!string.IsNullOrEmpty(jobData.WorkSchedule))
            {
                foreach (var day in jobData.WorkSchedule.Split('|'))
                {
                    var daySplit = day.Split('-');
                    switch (daySplit[0])
                    {
                        case "Monday":
                            jobData.Works_Monday = true;
                            if (daySplit[1].Contains(":"))
                            {
                                var time = daySplit[1].Split(':').ToList();
                                jobData.MondayInHour = int.Parse(time[0]);
                                jobData.MondayInMinutes = int.Parse(time[1]);
                            }
                            else
                                jobData.MondayInHour = int.Parse(daySplit[1]);
                            if (daySplit[2].Contains(":"))
                            {
                                var time = daySplit[2].Split(':').ToList();
                                jobData.MondayOutHour = int.Parse(time[0]);
                                jobData.MondayOutMinutes = int.Parse(time[1]);
                            }
                            else
                                jobData.MondayOutHour = int.Parse(daySplit[2]);
                            break;
                        case "Tuesday":
                            jobData.Works_Tuesday = true;
                            if (daySplit[1].Contains(":"))
                            {
                                var time = daySplit[1].Split(':').ToList();
                                jobData.TuesdayInHour = int.Parse(time[0]);
                                jobData.TuesdayInMinutes = int.Parse(time[1]);
                            }
                            else
                                jobData.TuesdayInHour = int.Parse(daySplit[1]);
                            if (daySplit[2].Contains(":"))
                            {
                                var time = daySplit[2].Split(':').ToList();
                                jobData.TuesdayOutHour = int.Parse(time[0]);
                                jobData.TuesdayOutMinutes = int.Parse(time[1]);
                            }
                            else
                                jobData.TuesdayOutHour = int.Parse(daySplit[2]);
                            break;
                        case "Wednesday":
                            jobData.Works_Wednesday = true;
                            if (daySplit[1].Contains(":"))
                            {
                                var time = daySplit[1].Split(':').ToList();
                                jobData.WednesdayInHour = int.Parse(time[0]);
                                jobData.WednesdayInMinutes = int.Parse(time[1]);
                            }
                            else
                                jobData.WednesdayInHour = int.Parse(daySplit[1]);
                            if (daySplit[2].Contains(":"))
                            {
                                var time = daySplit[2].Split(':').ToList();
                                jobData.WednesdayOutHour = int.Parse(time[0]);
                                jobData.WednesdayOutMinutes = int.Parse(time[1]);
                            }
                            else
                                jobData.WednesdayOutHour = int.Parse(daySplit[2]);
                            break;
                        case "Thursday":
                            jobData.Works_Thursday = true;
                            if (daySplit[1].Contains(":"))
                            {
                                var time = daySplit[1].Split(':').ToList();
                                jobData.ThursdayInHour = int.Parse(time[0]);
                                jobData.ThursdayInMinutes = int.Parse(time[1]);
                            }
                            else
                                jobData.ThursdayInHour = int.Parse(daySplit[1]);
                            if (daySplit[2].Contains(":"))
                            {
                                var time = daySplit[2].Split(':').ToList();
                                jobData.ThursdayOutHour = int.Parse(time[0]);
                                jobData.ThursdayOutMinutes = int.Parse(time[1]);
                            }
                            else
                                jobData.ThursdayOutHour = int.Parse(daySplit[2]);
                            break;
                        case "Friday":
                            jobData.Works_Friday = true;
                            if (daySplit[1].Contains(":"))
                            {
                                var time = daySplit[1].Split(':').ToList();
                                jobData.FridayInHour = int.Parse(time[0]);
                                jobData.FridayInMinutes = int.Parse(time[1]);
                            }
                            else
                                jobData.FridayInHour = int.Parse(daySplit[1]);
                            if (daySplit[2].Contains(":"))
                            {
                                var time = daySplit[2].Split(':').ToList();
                                jobData.FridayOutHour = int.Parse(time[0]);
                                jobData.FridayOutMinutes = int.Parse(time[1]);
                            }
                            else
                                jobData.FridayOutHour = int.Parse(daySplit[2]);
                            break;
                        case "Saturday":
                            jobData.Works_Saturday = true;
                            if (daySplit[1].Contains(":"))
                            {
                                var time = daySplit[1].Split(':').ToList();
                                jobData.SaturdayInHour = int.Parse(time[0]);
                                jobData.SaturdayInMinutes = int.Parse(time[1]);
                            }
                            else
                                jobData.SaturdayInHour = int.Parse(daySplit[1]);
                            if (daySplit[2].Contains(":"))
                            {
                                var time = daySplit[2].Split(':').ToList();
                                jobData.SaturdayOutHour = int.Parse(time[0]);
                                jobData.SaturdayOutMinutes = int.Parse(time[1]);
                            }
                            else
                                jobData.SaturdayOutHour = int.Parse(daySplit[2]);
                            break;
                        case "Sunday":
                            jobData.Works_Sunday = true;
                            if (daySplit[1].Contains(":"))
                            {
                                var time = daySplit[1].Split(':').ToList();
                                jobData.SundayInHour = int.Parse(time[0]);
                                jobData.SundayInMinutes = int.Parse(time[1]);
                            }
                            else
                                jobData.SundayInHour = int.Parse(daySplit[1]);
                            if (daySplit[2].Contains(":"))
                            {
                                var time = daySplit[2].Split(':').ToList();
                                jobData.SundayOutHour = int.Parse(time[0]);
                                jobData.SundayOutMinutes = int.Parse(time[1]);
                            }
                            else
                                jobData.SundayOutHour = int.Parse(daySplit[2]);
                            break;
                        default:
                            break;
                    }
                }
            }

            return View("~/Plugins/Teed.Plugin.Payroll/Views/JobCatalog/Edit.cshtml", jobData);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult Edit(JobCatalogData jobData, bool continueEditing)
        {
            var job = _jobCatalogService.GetById(jobData.Id);
            if (job == null)
                return Index();

            job.Description = jobData.Description;
            job.DisplayOrder = jobData.DisplayOrder;
            if (jobData.JobParentId < 1)
                job.ParentJobId = null;
            else
                job.ParentJobId = jobData.JobParentId;
            job.Name = jobData.Name;
            job.SalaryMax = jobData.SalaryMax;
            job.SalaryMin = jobData.SalaryMin;
            job.SubjectToWorkingHours = jobData.SubjectToWorkingHours;

            // Date string creation
            var days = new List<string>();
            if (jobData.Works_Monday)
            {
                days.Add($"Monday-{jobData.MondayInHour}:{jobData.MondayInMinutes}-{jobData.MondayOutHour}:{jobData.MondayOutMinutes}");
            }
            if (jobData.Works_Tuesday)
            {
                days.Add($"Tuesday-{jobData.TuesdayInHour}:{jobData.TuesdayInMinutes}-{jobData.TuesdayOutHour}:{jobData.TuesdayOutMinutes}");
            }
            if (jobData.Works_Wednesday)
            {
                days.Add($"Wednesday-{jobData.WednesdayInHour}:{jobData.WednesdayInMinutes}-{jobData.WednesdayOutHour}:{jobData.WednesdayOutMinutes}");
            }
            if (jobData.Works_Thursday)
            {
                days.Add($"Thursday-{jobData.ThursdayInHour}:{jobData.ThursdayInMinutes}-{jobData.ThursdayOutHour}:{jobData.ThursdayOutMinutes}");
            }
            if (jobData.Works_Friday)
            {
                days.Add($"Friday-{jobData.FridayInHour}:{jobData.FridayInMinutes}-{jobData.FridayOutHour}:{jobData.FridayOutMinutes}");
            }
            if (jobData.Works_Saturday)
            {
                days.Add($"Saturday-{jobData.SaturdayInHour}:{jobData.SaturdayInMinutes}-{jobData.SaturdayOutHour}:{jobData.SaturdayOutMinutes}");
            }
            if (jobData.Works_Sunday)
            {
                days.Add($"Sunday-{jobData.SundayInHour}:{jobData.SaturdayInMinutes}-{jobData.SundayOutHour}:{jobData.SundayOutMinutes}");
            }
            if (days.Any())
                job.WorkSchedule = string.Join("|", days);
            else
                job.WorkSchedule = null;

            _jobCatalogService.Update(job);

            if (continueEditing)
            {
                return RedirectToAction("Edit", new { id = job.Id });
            }
            return RedirectToAction("Index");
        }

        public JobCatalogData CreateHoursSelect(JobCatalogData model)
        {
            model.HoursSelect = new List<SelectListItem>();
            model.HoursSelect.Add(new SelectListItem { Selected = true, Text = "12:00 AM", Value = "0" });
            model.HoursSelect.Add(new SelectListItem { Text = "01:00 AM", Value = "1" });
            model.HoursSelect.Add(new SelectListItem { Text = "02:00 AM", Value = "2" });
            model.HoursSelect.Add(new SelectListItem { Text = "03:00 AM", Value = "3" });
            model.HoursSelect.Add(new SelectListItem { Text = "04:00 AM", Value = "4" });
            model.HoursSelect.Add(new SelectListItem { Text = "05:00 AM", Value = "5" });
            model.HoursSelect.Add(new SelectListItem { Text = "06:00 AM", Value = "6" });
            model.HoursSelect.Add(new SelectListItem { Text = "07:00 AM", Value = "7" });
            model.HoursSelect.Add(new SelectListItem { Text = "08:00 AM", Value = "8" });
            model.HoursSelect.Add(new SelectListItem { Text = "09:00 AM", Value = "9" });
            model.HoursSelect.Add(new SelectListItem { Text = "10:00 AM", Value = "10" });
            model.HoursSelect.Add(new SelectListItem { Text = "11:00 AM", Value = "11" });
            model.HoursSelect.Add(new SelectListItem { Text = "12:00 PM", Value = "12" });
            model.HoursSelect.Add(new SelectListItem { Text = "01:00 PM", Value = "13" });
            model.HoursSelect.Add(new SelectListItem { Text = "02:00 PM", Value = "14" });
            model.HoursSelect.Add(new SelectListItem { Text = "03:00 PM", Value = "15" });
            model.HoursSelect.Add(new SelectListItem { Text = "04:00 PM", Value = "16" });
            model.HoursSelect.Add(new SelectListItem { Text = "05:00 PM", Value = "17" });
            model.HoursSelect.Add(new SelectListItem { Text = "06:00 PM", Value = "18" });
            model.HoursSelect.Add(new SelectListItem { Text = "07:00 PM", Value = "19" });
            model.HoursSelect.Add(new SelectListItem { Text = "08:00 PM", Value = "20" });
            model.HoursSelect.Add(new SelectListItem { Text = "09:00 PM", Value = "21" });
            model.HoursSelect.Add(new SelectListItem { Text = "10:00 PM", Value = "22" });
            model.HoursSelect.Add(new SelectListItem { Text = "11:00 PM", Value = "23" });
            return model;
        }

        public JobCatalogData CreateMinutesSelect(JobCatalogData model)
        {
            model.MinutesSelect = new List<SelectListItem>();
            for (int i = 0; i < 60; i++)
            {
                model.MinutesSelect.Add(new SelectListItem { Selected = i == 0 ? true : false, Text = $"{i} minutos", Value = i.ToString() });
            }
            return model;
        }

        public string CreateWorkScheduleString(string schedule)
        {
            var final = "Sin horario de trabajo especificado";
            if (!string.IsNullOrEmpty(schedule))
            {
                var listOfDays = new List<string>();
                var checkInTime = DateTime.Now;
                var checkOutTime = DateTime.Now;
                foreach (var day in schedule.Split('|'))
                {
                    var daySplit = day.Split('-');
                    switch (daySplit[0])
                    {
                        case "Monday":
                            if (daySplit[1].Contains(":"))
                            {
                                var time = daySplit[1].Split(':').ToList();
                                checkInTime = new DateTime(2000, 1, 1, int.Parse(time[0]), int.Parse(time[1]), 0);
                            }
                            else
                                checkInTime = new DateTime(2000, 1, 1, int.Parse(daySplit[1]), 0, 0);
                            if (daySplit[2].Contains(":"))
                            {
                                var time = daySplit[2].Split(':').ToList();
                                checkOutTime = new DateTime(2000, 1, 1, int.Parse(time[0]), int.Parse(time[1]), 0);
                            }
                            else
                                checkOutTime = new DateTime(2000, 1, 1, int.Parse(daySplit[2]), 0, 0);
                            listOfDays.Add($"Lunes de {checkInTime:hh:mm tt} a {checkOutTime:hh:mm tt}");
                            break;
                        case "Tuesday":
                            if (daySplit[1].Contains(":"))
                            {
                                var time = daySplit[1].Split(':').ToList();
                                checkInTime = new DateTime(2000, 1, 1, int.Parse(time[0]), int.Parse(time[1]), 0);
                            }
                            else
                                checkInTime = new DateTime(2000, 1, 1, int.Parse(daySplit[1]), 0, 0);
                            if (daySplit[2].Contains(":"))
                            {
                                var time = daySplit[2].Split(':').ToList();
                                checkOutTime = new DateTime(2000, 1, 1, int.Parse(time[0]), int.Parse(time[1]), 0);
                            }
                            else
                                checkOutTime = new DateTime(2000, 1, 1, int.Parse(daySplit[2]), 0, 0);
                            listOfDays.Add($"Martes de {checkInTime:hh:mm tt} a {checkOutTime:hh:mm tt}");
                            break;
                        case "Wednesday":
                            if (daySplit[1].Contains(":"))
                            {
                                var time = daySplit[1].Split(':').ToList();
                                checkInTime = new DateTime(2000, 1, 1, int.Parse(time[0]), int.Parse(time[1]), 0);
                            }
                            else
                                checkInTime = new DateTime(2000, 1, 1, int.Parse(daySplit[1]), 0, 0);
                            if (daySplit[2].Contains(":"))
                            {
                                var time = daySplit[2].Split(':').ToList();
                                checkOutTime = new DateTime(2000, 1, 1, int.Parse(time[0]), int.Parse(time[1]), 0);
                            }
                            else
                                checkOutTime = new DateTime(2000, 1, 1, int.Parse(daySplit[2]), 0, 0);
                            listOfDays.Add($"Miércoles de {checkInTime:hh:mm tt} a {checkOutTime:hh:mm tt}");
                            break;
                        case "Thursday":
                            if (daySplit[1].Contains(":"))
                            {
                                var time = daySplit[1].Split(':').ToList();
                                checkInTime = new DateTime(2000, 1, 1, int.Parse(time[0]), int.Parse(time[1]), 0);
                            }
                            else
                                checkInTime = new DateTime(2000, 1, 1, int.Parse(daySplit[1]), 0, 0);
                            if (daySplit[2].Contains(":"))
                            {
                                var time = daySplit[2].Split(':').ToList();
                                checkOutTime = new DateTime(2000, 1, 1, int.Parse(time[0]), int.Parse(time[1]), 0);
                            }
                            else
                                checkOutTime = new DateTime(2000, 1, 1, int.Parse(daySplit[2]), 0, 0);
                            listOfDays.Add($"Jueves de {checkInTime:hh:mm tt} a {checkOutTime:hh:mm tt}");
                            break;
                        case "Friday":
                            if (daySplit[1].Contains(":"))
                            {
                                var time = daySplit[1].Split(':').ToList();
                                checkInTime = new DateTime(2000, 1, 1, int.Parse(time[0]), int.Parse(time[1]), 0);
                            }
                            else
                                checkInTime = new DateTime(2000, 1, 1, int.Parse(daySplit[1]), 0, 0);
                            if (daySplit[2].Contains(":"))
                            {
                                var time = daySplit[2].Split(':').ToList();
                                checkOutTime = new DateTime(2000, 1, 1, int.Parse(time[0]), int.Parse(time[1]), 0);
                            }
                            else
                                checkOutTime = new DateTime(2000, 1, 1, int.Parse(daySplit[2]), 0, 0);
                            listOfDays.Add($"Viernes de {checkInTime:hh:mm tt} a {checkOutTime:hh:mm tt}");
                            break;
                        case "Saturday":
                            if (daySplit[1].Contains(":"))
                            {
                                var time = daySplit[1].Split(':').ToList();
                                checkInTime = new DateTime(2000, 1, 1, int.Parse(time[0]), int.Parse(time[1]), 0);
                            }
                            else
                                checkInTime = new DateTime(2000, 1, 1, int.Parse(daySplit[1]), 0, 0);
                            if (daySplit[2].Contains(":"))
                            {
                                var time = daySplit[2].Split(':').ToList();
                                checkOutTime = new DateTime(2000, 1, 1, int.Parse(time[0]), int.Parse(time[1]), 0);
                            }
                            else
                                checkOutTime = new DateTime(2000, 1, 1, int.Parse(daySplit[2]), 0, 0);
                            listOfDays.Add($"Sabado de {checkInTime:hh:mm tt} a {checkOutTime:hh:mm tt}");
                            break;
                        case "Sunday":
                            if (daySplit[1].Contains(":"))
                            {
                                var time = daySplit[1].Split(':').ToList();
                                checkInTime = new DateTime(2000, 1, 1, int.Parse(time[0]), int.Parse(time[1]), 0);
                            }
                            else
                                checkInTime = new DateTime(2000, 1, 1, int.Parse(daySplit[1]), 0, 0);
                            if (daySplit[2].Contains(":"))
                            {
                                var time = daySplit[2].Split(':').ToList();
                                checkOutTime = new DateTime(2000, 1, 1, int.Parse(time[0]), int.Parse(time[1]), 0);
                            }
                            else
                                checkOutTime = new DateTime(2000, 1, 1, int.Parse(daySplit[2]), 0, 0);
                            listOfDays.Add($"Domingo de {checkInTime:hh:mm tt} a {checkOutTime:hh:mm tt}");
                            break;
                        default:
                            break;
                    }
                }
                final = string.Join("<br/>", listOfDays);
            }
            return final;
        }

        [HttpPost]
        public IActionResult GetJobs(int id = 0)
        {
            var jobs = _jobCatalogService.GetAll()
                .OrderBy(x => x.Id).ToList();

            return Json(jobs.Select(x =>
            {
                var allNames = new List<string>();
                var currentParentId = x.ParentJobId ?? 0;
                var lastParentId = 0;
                do
                {
                    var parent = jobs.Where(y => y.Id == currentParentId).FirstOrDefault();
                    if (parent != null)
                    {
                        allNames.Add(parent.Name);
                        currentParentId = parent.ParentJobId ?? 0;
                        lastParentId = parent.Id;
                    }
                } while (currentParentId > 0);
                allNames.Reverse();
                allNames.Add(x.Name);
                return new
                {
                    Id = x.Id.ToString(),
                    Name = string.Join(" >> ", allNames),
                    Selected = x.Id == id,
                    ParentId = lastParentId
                };
            }).OrderBy(x => x.ParentId)
            .ToList());
        }
    }
    public class SaveJobsData
    {
        public int ParentJobId { get; set; }
        public int[] JobIds { get; set; }
    }
}