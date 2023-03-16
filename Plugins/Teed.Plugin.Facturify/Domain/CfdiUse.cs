using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Facturify.Domain
{
    public enum CfdiUse
    {
        [Display(Name = "Adquisición de mercancias")]
        G01 = 1,

        [Display(Name = "Devoluciones, descuentos o bonificaciones")]
        G02 = 2,

        [Display(Name = "Gastos en general")]
        G03 = 3,

        [Display(Name = "Construcciones")]
        I01 = 4,

        [Display(Name = "Mobilario y equipo de oficina por inversiones")]
        I02 = 5,

        [Display(Name = "Equipo de transporte")]
        I03 = 6,

        [Display(Name = "Equipo de computo y accesorios")]
        I04 = 7,

        [Display(Name = "Dados, troqueles, moldes, matrices y herramental")]
        I05 = 8,

        [Display(Name = "Comunicaciones telefónicas")]
        I06 = 9,

        [Display(Name = "Comunicaciones satelitales")]
        I07 = 10,

        [Display(Name = "Otra maquinaria y equipo")]
        I08 = 11,

        [Display(Name = "Honorarios médicos, dentales y gastos hospitalarios.")]
        D01 = 12,

        [Display(Name = "Gastos médicos por incapacidad o discapacidad")]
        D02 = 13,

        [Display(Name = "Gastos funerales")]
        D03 = 14,

        [Display(Name = "Donativos")]
        D04 = 15,

        [Display(Name = "Intereses reales efectivamente pagados por créditos hipotecarios (casa habitación)")]
        D05 = 16,

        [Display(Name = "Aportaciones voluntarias al SAR")]
        D06 = 17,

        [Display(Name = "Primas por seguros de gastos médicos")]
        D07 = 18,

        [Display(Name = "Gastos de transportación escolar obligatoria")]
        D08 = 19,

        [Display(Name = "Gastos de transportación escolar obligatoria")]
        D09 = 20,

        [Display(Name = "Pagos por servicios educativos (colegiaturas)")]
        D10 = 21,

        [Display(Name = "Por definir")]
        P01 = 22
    }

}
