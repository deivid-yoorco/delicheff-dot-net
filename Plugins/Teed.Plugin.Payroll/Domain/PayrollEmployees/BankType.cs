using System.ComponentModel.DataAnnotations;

namespace Teed.Plugin.Payroll.Domain.PayrollEmployees
{
    public enum BankType
    {
        [Display(Name = "ABC Capital",
        Description = "ABC Capital")]
        ABCCapital = 0,

        [Display(Name = "Accendo Banco",
        Description = "Accendo Banco")]
        AccendoBanco = 1,

        [Display(Name = "American Express Bank (México)",
        Description = "American Express Bank (México)")]
        AmericanExpressBank = 2,

        [Display(Name = "Banca Afirme",
        Description = "Banca Afirme")]
        BancaAfirme = 3,

        [Display(Name = "Banca Mifel",
        Description = "Banca Mifel")]
        BancaMifel = 4,

        [Display(Name = "Banco Actinver",
        Description = "Banco Actinver")]
        BancoActinver = 5,

        [Display(Name = "Banco Autofin México",
        Description = "Banco Autofin México")]
        BancoAutofinMexico = 6,

        [Display(Name = "Banco Azteca",
        Description = "Banco Azteca")]
        BancoAzteca = 7,

        [Display(Name = "Banco Bancrea",
        Description = "Banco Bancrea")]
        BancoBancrea = 8,

        [Display(Name = "Banco Base",
        Description = "Banco Base")]
        BancoBase = 9,

        [Display(Name = "Banco Compartamos",
        Description = "Banco Compartamos")]
        BancoCompartamos = 10,

        [Display(Name = "Banco Credit Suisse (México)",
        Description = "Banco Credit Suisse (México)")]
        BancoCreditSuisse = 11,

        [Display(Name = "Banco de Inversión Afirme",
        Description = "Banco de Inversión Afirme")]
        BancoInversionAfirme = 12,

        [Display(Name = "Banco del Bajío",
        Description = "Banco del Bajío")]
        BancoBajio = 13,

        [Display(Name = "Banco Finterra",
        Description = "Banco Finterra")]
        BancoFinterra = 14,

        [Display(Name = "Banco Forjadores",
        Description = "Banco Forjadores")]
        BancoForjadores = 15,

        [Display(Name = "Banco Inbursa",
        Description = "Banco Inbursa")]
        BancoInbursa = 16,

        [Display(Name = "Banco Inmobiliario Mexicano",
        Description = "Banco Inmobiliario Mexicano")]
        BancoInmobiliarioMexicano = 17,

        [Display(Name = "Banco Invex",
        Description = "Banco Invex")]
        BancoInvex = 18,

        [Display(Name = "Banco JP Morgan",
        Description = "Banco JP Morgan")]
        BancoJpMorgan = 19,

        [Display(Name = "Banco KEB Hana México",
        Description = "Banco KEB Hana México")]
        BancoKebHanaMexico = 20,

        [Display(Name = "Banco Monex",
        Description = "Banco Monex")]
        BancoMonex = 21,

        [Display(Name = "Banco Multiva",
        Description = "Banco Multiva")]
        BancoMultiva = 22,

        [Display(Name = "Banco PagaTodo",
        Description = "Banco PagaTodo")]
        BancoPagaTodo = 23,

        [Display(Name = "Banco Regional de Monterrey",
        Description = "Banco Regional de Monterrey")]
        BancoRegionalMonterrey = 24,

        [Display(Name = "Banco S3 México",
        Description = "Banco S3 México")]
        BancoS3Mexico = 25,

        [Display(Name = "Banco Sabadell",
        Description = "Banco Sabadell")]
        BancoSabadell = 26,

        [Display(Name = "Banco Santander",
        Description = "Banco Santander")]
        BancoSantander = 27,

        [Display(Name = "Banco Shinhan de México",
        Description = "Banco Shinhan de México")]
        BancoShinhanMexico = 28,

        [Display(Name = "Banco Ve por Más",
        Description = "Banco Ve por Más")]
        BancoVePorMas = 29,

        [Display(Name = "BanCoppel",
        Description = "BanCoppel")]
        BanCoppel = 30,

        [Display(Name = "Bank of America Mexico",
        Description = "Bank of America Mexico")]
        BankOfAmericaMexico = 31,

        [Display(Name = "Bank of China Mexico",
        Description = "Bank of China Mexico")]
        BankOfChinaMexico = 32,

        [Display(Name = "Bankaool",
        Description = "Bankaool")]
        Bankaool = 33,

        [Display(Name = "Banorte",
        Description = "Banorte")]
        Banorte = 34,

        [Display(Name = "Bansí",
        Description = "Bansí")]
        Bansi = 35,

        [Display(Name = "Barclays Bank México",
        Description = "Barclays Bank México")]
        BarclaysBankMexico = 36,

        [Display(Name = "BBVA",
        Description = "BBVA")]
        BBVA = 37,

        [Display(Name = "Citibanamex",
        Description = "Citibanamex")]
        Citibanamex = 38,

        [Display(Name = "CIBanco",
        Description = "CIBanco")]
        CIBanco = 39,

        [Display(Name = "Consubanco",
        Description = "Consubanco")]
        Consubanco = 40,

        [Display(Name = "Deutsche Bank México",
        Description = "Deutsche Bank México")]
        DeutscheBankMexico = 41,

        [Display(Name = "Fundación Dondé Banco",
        Description = "Fundación Dondé Banco")]
        FundacionDondeBanco = 42,

        [Display(Name = "HSBC México",
        Description = "HSBC México")]
        HSBCMexico = 43,

        [Display(Name = "Industrial and Commercial Bank of China",
        Description = "Industrial and Commercial Bank of China")]
        IndustrialCommercialBankChina = 44,

        [Display(Name = "Intercam Banco",
        Description = "Intercam Banco")]
        IntercamBanco = 45,

        [Display(Name = "Mizuho Bank",
        Description = "Mizuho Bank")]
        MizuhoBank = 46,

        [Display(Name = "MUFG Bank Mexico",
        Description = "MUFG Bank Mexico")]
        MufgBankMexico = 47,

        [Display(Name = "Scotiabank",
        Description = "Scotiabank")]
        Scotiabank = 48,

        [Display(Name = "Otro",
        Description = "Other")]
        Other = 100
    }
}
