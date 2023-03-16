using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Facturify.Domain
{
    public enum FiscalRegime
    {
        [Display(Name = "General de Ley Personas Morales")]
        GeneraldeLeyPersonasMorales = 601,

        [Display(Name = "Personas Morales con Fines no Lucrativos")]
        PersonasMoralesconFinesnoLucrativos = 603,

        [Display(Name = "Sueldos y Salarios e Ingresos Asimilados a Salarios")]
        SueldosySalarioseIngresosAsimiladosaSalarios = 605,

        [Display(Name = "Arrendamiento")]
        Arrendamiento = 606,

        [Display(Name = "Demás ingresos")]
        Demasingresos = 608,

        [Display(Name = "Consolidación")]
        Consolidacion = 609,

        [Display(Name = "Residentes en el Extranjero sin Establecimiento Permanente en México")]
        ResidentesenelExtranjerosinEstablecimientoPermanenteenMexico = 610,

        [Display(Name = "Ingresos por Dividendos (socios y accionistas)")]
        IngresosporDividendos = 611,

        [Display(Name = "Personas Fí­sicas con Actividades Empresariales y Profesionales")]
        PersonasFi­sicasconActividadesEmpresarialesyProfesionales = 612,

        [Display(Name = "Ingresos por intereses")]
        Ingresosporintereses = 614,

        [Display(Name = "Sin obligaciones fiscales")]
        Sinobligacionesfiscales = 616,

        [Display(Name = "Sociedades Cooperativas de Producción que optan por diferir sus ingresos")]
        SociedadesCooperativasdeProduccionqueoptanpordiferirsusingresos = 620,

        [Display(Name = "Incorporación Fiscal")]
        IncorporacionFiscal = 621,

        [Display(Name = "Actividades Agrícolas, Ganaderas, Silvícolas y Pesqueras")]
        ActividadesAgricolasGanaderasSilvicolasyPesqueras = 622,

        [Display(Name = "Opcional para Grupos de Sociedades")]
        OpcionalparaGruposdeSociedades = 623,

        [Display(Name = "Coordinados")]
        Coordinados = 624,

        [Display(Name = "Hidrocarburos")]
        Hidrocarburos = 628,

        [Display(Name = "Régimen de Enajenación o Adquisición de Bienes")]
        RegimendeEnajenacionoAdquisiciondeBienes = 607,

        [Display(Name = "De los Regí­menes Fiscales Preferentes y de las Empresas Multinacionales")]
        DelosRegi­menesFiscalesPreferentesydelasEmpresasMultinacionales = 629,

        [Display(Name = "Enajenación de acciones en bolsa de valores")]
        Enajenaciondeaccionesenbolsadevalores = 630,

        [Display(Name = "Régimen de los ingresos por obtención de premios")]
        Regimendelosingresosporobtenciondepremios = 615
    }
}