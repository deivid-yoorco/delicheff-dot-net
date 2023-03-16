using System.ComponentModel.DataAnnotations;

namespace Teed.Plugin.CustomPages.Models.CustomPages
{
    public class CreateViewModel
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        public string Name { get; set; }
    }
}