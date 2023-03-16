using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Teed.Plugin.Payroll.Domain.PayrollEmployees
{
    public static class FileTypes
    {
        public static IList<FileType> OptionalFileTypes
        {
            get
            {
                return new List<FileType>
                {
                    FileType.EmploymentTerminationDocument,
                    FileType.ProfilePicture,
                    FileType.SocioeconomicStudy,
                    FileType.PenalBackgroundReport,
                    FileType.LetterOfRecommendation,
                    FileType.AdministrativeRecord,
                };
            }
        }

        public static IList<FileType> ExtraOptionalFileTypes
        {
            get
            {
                return new List<FileType>
                {
                    FileType.ImssVoucher,
                    FileType.EmployerRegistrationIMSS,
                };
            }
        }
    }

    public enum FileType
    {
        ProfilePicture = 0,

        [Display(Name = "Documentos de terminación de relación laboral",
            Description = "Documentos de terminación de relación laboral")]
        EmploymentTerminationDocument = 1,

        [Display(Name = "Acta de nacimiento",
            Description = "Acta de nacimiento")]
        BirthCertificate = 2,

        [Display(Name = "Comprobante de domicilio",
            Description = "Comprobante de domicilio")]
        ProofOfAddress = 3,

        [Display(Name = "Identificación oficial",
            Description = "Identificación oficial")]
        OfficialIdentification = 4,

        [Display(Name = "Contrato laboral firmado",
            Description = "Contrato laboral firmado")]
        SignedContract = 5,

        [Display(Name = "CURP",
            Description = "CURP")]
        Curp = 6,

        [Display(Name = "Reporte no antecedentes penales",
            Description = "Reporte no antecedentes penales")]
        PenalBackgroundReport = 7,

        [Display(Name = "Número de Seguridad Social (NSS IMSS)",
            Description = "Número de Seguridad Social (NSS IMSS)")]
        ImssVoucher = 8,

        [Display(Name = "Comprobante de estudios",
            Description = "Comprobante de estudios")]
        ProofOfStudies = 9,

        [Display(Name = "Carátula de estado de cuenta",
            Description = "Carátula de estado de cuenta")]
        BankStatementCover = 10,

        [Display(Name = "Estudio socioeconómico",
            Description = "Estudio socioeconómico")]
        SocioeconomicStudy = 11,

        [Display(Name = "Contrato de cesión de derechos de imagen",
            Description = "Contrato de cesión de derechos de imagen")]
        ImageTransferContract = 12,

        [Display(Name = "Convenio de confidencialidad",
            Description = "Convenio de confidencialidad")]
        ConfidentialityAgreement = 13,

        [Display(Name = "Solicitud de empleo",
            Description = "Solicitud de empleo")]
        JobApplication = 14,

        [Display(Name = "Alta patronal en IMSS",
            Description = "Alta patronal en IMSS")]
        EmployerRegistrationIMSS = 15,

        [Display(Name = "Constancia de RFC",
            Description = "Constancia de RFC")]
        RfcConstancy = 16,

        [Display(Name = "Carta de recomendación",
            Description = "Carta de recomendación")]
        LetterOfRecommendation = 17,

        [Display(Name = "Acta administrativa",
            Description = "Acta administrativa")]
        AdministrativeRecord = 18,
    }
}
