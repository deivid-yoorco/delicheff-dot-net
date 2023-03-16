namespace Nop.Services
{
    public static class TeedCommerceStores
    {
        public static TeedStores CurrentStore
        {
            get
            {
                return TeedStores.CentralEnLinea;
            }
        }
    }
    
    public enum TeedStores
    {
        Aldora,
        Indigo,
        Kromtek,
        UvasYMoras,
        Dermalomas,
        BastardsLeague,
        TeedCommerce,
        Montecito38,
        TecaHomeStore,
        LogoShop,
        Cuchurrumin,
        CentralEnLinea,
        Hamleys,
        Lining,
        Scalpers,
        UvasYMorasExpress,
        TeedImd,
        ProductosPersonalizados,
        Emedemar,
        InkStudio,
        ZanaAlquimia,
        Masa,
        Lamy,
        Lithographic,
        Atomica,
        ElPomito,
        AsianBay,
        Pelletier,
        SuperPapelera,
        Fragoza,
        Chabelo,
        CursosCirculoAlba,
        Cetro,
        Energiahumana,
        MexicoLimited,
        BubuBaby
    }
}