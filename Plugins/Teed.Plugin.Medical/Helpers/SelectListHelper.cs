using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using System;
using System.Collections.Generic;
using System.Linq;
using Teed.Plugin.Medical.Services;

namespace Teed.Plugin.Medical.Helpers
{
    public static class SelectListHelper
    {
        public static List<SelectListItem> GetDoctorsAndNursesList(ICustomerService customerService, int[] ignoreUsers = null)
        {
            if (customerService == null)
                throw new ArgumentNullException(nameof(customerService));

            List<SelectListItem> result = new List<SelectListItem>();
            List<int> roleIds = new List<int>();
            foreach (var role in customerService.GetAllCustomerRoles())
            {
                if (role.SystemName == "CosmetologistJr" || 
                    role.SystemName == "CosmetologistSr" || 
                    role.SystemName == "NurseJr" || 
                    role.SystemName == "NurseSr" || 
                    role.SystemName == "Doctor" || 
                    role.SystemName == "Assistant")
                {
                    roleIds.Add(role.Id);
                }
            }

            IEnumerable<Customer> listItems;
            if (ignoreUsers == null)
            {
                listItems = customerService.GetAllCustomers(customerRoleIds: roleIds.ToArray()).AsEnumerable();
            }
            else
            {
                listItems = customerService.GetAllCustomers(customerRoleIds: roleIds.ToArray()).Where(x => !ignoreUsers.Contains(x.Id));
            }
            foreach (var item in listItems)
            {
                result.Add(new SelectListItem
                {
                    Text = $"{item.GetFullName()} ({item.Email})",
                    Value = item.Id.ToString()
                });
            }
            return result;
        }

        public static List<Customer> GetDoctorsAndNursesListCustomer(ICustomerService customerService, int[] ignoreUsers = null)
        {
            if (customerService == null)
                throw new ArgumentNullException(nameof(customerService));

            List<SelectListItem> result = new List<SelectListItem>();
            List<int> roleIds = new List<int>();
            foreach (var role in customerService.GetAllCustomerRoles())
            {
                if (role.SystemName == "CosmetologistJr" ||
                    role.SystemName == "CosmetologistSr" ||
                    role.SystemName == "NurseJr" ||
                    role.SystemName == "NurseSr" ||
                    role.SystemName == "Doctor" ||
                    role.SystemName == "Assistant")
                {
                    roleIds.Add(role.Id);
                }
            }

            List<Customer> listItems;
            if (ignoreUsers == null)
            {
                listItems = customerService.GetAllCustomers(customerRoleIds: roleIds.ToArray()).ToList();
            }
            else
            {
                listItems = customerService.GetAllCustomers(customerRoleIds: roleIds.ToArray()).Where(x => !ignoreUsers.Contains(x.Id)).ToList();
            }
           
            return listItems;
        }

        public static List<SelectListItem> GetProductsList(IProductService productService)
        {
            if (productService == null)
                throw new ArgumentNullException(nameof(productService));

            List<SelectListItem> result = new List<SelectListItem>();
            var listItems = productService.SearchProducts();
            foreach (var item in listItems)
            {
                result.Add(new SelectListItem
                {
                    Text = $"{item.Name}",
                    Value = item.Id.ToString()
                });
            }
            return result;
        }

        public static List<SelectListItem> GetBranchesList(BranchService service)
        {
            if (service == null)
                throw new ArgumentNullException(nameof(service));

            List<SelectListItem> result = new List<SelectListItem>();
            var listItems = service.GetAll();
            foreach (var item in listItems)
            {
                result.Add(new SelectListItem
                {
                    Text = $"{item.Name}",
                    Value = item.Id.ToString()
                });
            }
            return result;
        }

        public static List<SelectListItem> GetAllCustomers(ICustomerService customerService)
        {
            if (customerService == null)
                throw new ArgumentNullException(nameof(customerService));

            List<SelectListItem> result = new List<SelectListItem>();
            IEnumerable<Customer> listItems = customerService.GetAllCustomers().Where(x => !string.IsNullOrWhiteSpace(x.Email) && !string.IsNullOrWhiteSpace(x.GetFullName()));
            foreach (var item in listItems)
            {
                result.Add(new SelectListItem
                {
                    Text = $"{item.GetFullName()} ({item.Email})",
                    Value = item.Id.ToString()
                });
            }
            return result;
        }
    }
}