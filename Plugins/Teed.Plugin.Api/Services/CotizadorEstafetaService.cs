using Nop.Services.Common;
using Nop.Services.Shipping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teed.Plugin.Api.Dtos.Products;

namespace Teed.Plugin.Api.Services
{
    public class CotizadorEstafetaService
    {
        private const string ESTAFETA_LOGIN = "8705154";
        private const string ESTAFETA_PASSWORD = "cUuC9Q4HZ";
        private const int ESTAFETA_COTIZADOR_ID = 99;

        private readonly IShippingService _shippingService;
        private readonly IAddressService _addressService;


        public CotizadorEstafetaService(IShippingService shippingService,
            IAddressService addressService)
        {
            _shippingService = shippingService;
            _addressService = addressService;
        }

        public FrecuenciaCotizadorEstafeta.Respuesta[] CreateRateRequest(CalculateShippingDto getShippingOptionRequest)
        {
            #region List

            List<LegacyProductDto> categoryX = new List<LegacyProductDto>();
            List<LegacyProductDto> categoryS = new List<LegacyProductDto>();
            List<LegacyProductDto> categoryC = new List<LegacyProductDto>();

            List<List<LegacyProductDto>> boxesA = new List<List<LegacyProductDto>>();
            List<List<LegacyProductDto>> boxesB = new List<List<LegacyProductDto>>();
            List<List<LegacyProductDto>> boxesC = new List<List<LegacyProductDto>>();

            #endregion

            #region SepararCategorias
            for (int i = 0; i < getShippingOptionRequest.Items.Count; i++)
            {
                var cat = getShippingOptionRequest.Items[i].ProductCategories.ToList();

                for (int j = 0; j < getShippingOptionRequest.Items[i].Quantity; j++)
                {
                    if (cat[cat.Count - 1].Name == "Cascos" || cat[cat.Count - 1].Name == "Botas" ||
                    cat[cat.Count - 1].Name == "Chamarras" || cat[cat.Count - 1].Name == "Pantalones")
                    {
                        categoryX.Add(getShippingOptionRequest.Items[i]);
                    }
                    else if (cat[cat.Count - 1].Name == "Candados y cadenas")
                    {
                        categoryC.Add(getShippingOptionRequest.Items[i]);
                    }
                    else
                    {
                        categoryS.Add(getShippingOptionRequest.Items[i]);
                    }
                }
            }
            #endregion

            #region ArmarCajas

            if (categoryX.Count == 0 && categoryC.Count > 0)
            {
                List<LegacyProductDto> box = new List<LegacyProductDto>();
                for (int i = 0; categoryC.Count > 0 && box.Count < 2; i++)
                {
                    box.Add(categoryC[0]);
                    categoryC.RemoveAt(0);
                }
                int products = box.Count;

                for (int j = 0; categoryS.Count > 0 && box.Count - products < 5 - products; j++)
                {
                    box.Add(categoryS[0]);
                    categoryS.RemoveAt(0);
                }

                if (box.Count < 4)
                {
                    boxesB.Add(box);
                }
                else
                {
                    boxesC.Add(box);
                }
            }

            while (categoryX.Count > 0)
            {
                List<LegacyProductDto> box = new List<LegacyProductDto>();
                box.Add(categoryX[0]);
                categoryX.RemoveAt(0);

                if (categoryC.Count != 0)
                {
                    for (int i = 0; categoryC.Count > 0 && box.Count < 2; i++)
                    {
                        box.Add(categoryC[0]);
                        categoryC.RemoveAt(0);
                    }
                    int products = box.Count;

                    for (int j = 0; categoryS.Count > 0 && box.Count - products < 5 - products; j++)
                    {
                        box.Add(categoryS[0]);
                        categoryS.RemoveAt(0);
                    }

                    if (box.Count < 4)
                    {
                        boxesB.Add(box);
                    }
                    else
                    {
                        boxesC.Add(box);
                    }
                }
                else
                {
                    for (int j = 0; categoryS.Count > 0 && box.Count < 6; j++)
                    {
                        box.Add(categoryS[0]);
                        categoryS.RemoveAt(0);
                    }
                    boxesB.Add(box);
                }
            }

            while (categoryS.Count > 0)
            {
                List<LegacyProductDto> box = null;

                if (categoryC.Count != 0)
                {
                    box = new List<LegacyProductDto>();
                    for (int i = 0; categoryC.Count > 0 && box.Count < 2; i++)
                    {
                        box.Add(categoryC[i]);
                        categoryC.RemoveAt(i);
                    }
                    int products = box.Count;
                    if (products < 2)
                    {
                        for (int j = 0; categoryS.Count > 0 && box.Count - products < 3 - products; j++)
                        {
                            box.Add(categoryS[0]);
                            categoryS.RemoveAt(0);
                        }
                    }
                    boxesA.Add(box);
                }
                else
                {
                    box = new List<LegacyProductDto>();
                    for (int j = 0; categoryS.Count > 0 && box.Count < 5; j++)
                    {
                        box.Add(categoryS[0]);
                        categoryS.RemoveAt(0);
                    }
                    boxesA.Add(box);
                }
            }
            #endregion

            #region CotizarPaquetes

            FrecuenciaCotizadorEstafeta.ServiceSoapClient client = new FrecuenciaCotizadorEstafeta.ServiceSoapClient(FrecuenciaCotizadorEstafeta.ServiceSoapClient.EndpointConfiguration.ServiceSoap12);

            var warehouseAddressId = _shippingService.GetWarehouseById(getShippingOptionRequest.Items.Select(x => x.WarehouseId).FirstOrDefault()).AddressId;
            string[] envio = { _addressService.GetAddressById(warehouseAddressId).ZipPostalCode };
            string[] destino = { getShippingOptionRequest.ShippingAddress.ZipPostalCode };

            FrecuenciaCotizadorEstafeta.Respuesta[] result = null;

            foreach (var box in boxesC)
            {
                Decimal peso = 0;
                foreach (var product in box)
                {
                    peso += product.Weight;
                }

                FrecuenciaCotizadorEstafeta.TipoEnvio tEnvio = new FrecuenciaCotizadorEstafeta.TipoEnvio
                {
                    Alto = 32,
                    Ancho = 30,
                    EsPaquete = true,
                    Largo = 45,
                    Peso = Convert.ToDouble(peso)
                };

                FrecuenciaCotizadorEstafeta.Respuesta[] res = client.FrecuenciaCotizadorAsync(ESTAFETA_COTIZADOR_ID,
                    ESTAFETA_LOGIN, ESTAFETA_PASSWORD, false, true, tEnvio, envio, destino).Result;

                if (result == null)
                {
                    result = res;
                }
                else
                {
                    for (int j = 0; j < result.Length; j++)
                    {
                        for (int k = 0; k < result[j].TipoServicio.Length; k++)
                        {
                            result[j].TipoServicio[k].CostoTotal += res[j].TipoServicio[k].CostoTotal;
                        }
                    }
                }
            }

            foreach (var box in boxesB)
            {
                Decimal peso = 0;
                foreach (var product in box)
                {
                    peso += product.Weight;
                }

                FrecuenciaCotizadorEstafeta.TipoEnvio tEnvio = new FrecuenciaCotizadorEstafeta.TipoEnvio
                {
                    Alto = 31,
                    Ancho = 30,
                    EsPaquete = true,
                    Largo = 40,
                    Peso = Convert.ToDouble(peso)
                };

                FrecuenciaCotizadorEstafeta.Respuesta[] res = client.FrecuenciaCotizadorAsync(ESTAFETA_COTIZADOR_ID,
                    ESTAFETA_LOGIN, ESTAFETA_PASSWORD, false, true, tEnvio, envio, destino).Result;

                if (result == null)
                {
                    result = res;
                }
                else
                {
                    for (int j = 0; j < result.Length; j++)
                    {
                        for (int k = 0; k < result[j].TipoServicio.Length; k++)
                        {
                            result[j].TipoServicio[k].CostoTotal += res[j].TipoServicio[k].CostoTotal;
                        }
                    }
                }
            }

            foreach (var box in boxesA)
            {
                Decimal peso = 0;
                foreach (var product in box)
                {
                    peso += product.Weight;
                }

                FrecuenciaCotizadorEstafeta.TipoEnvio tEnvio = new FrecuenciaCotizadorEstafeta.TipoEnvio
                {
                    Alto = 22.5,
                    Ancho = 18,
                    EsPaquete = true,
                    Largo = 29,
                    Peso = Convert.ToDouble(peso)
                };

                FrecuenciaCotizadorEstafeta.Respuesta[] res = client.FrecuenciaCotizadorAsync(ESTAFETA_COTIZADOR_ID,
                    ESTAFETA_LOGIN, ESTAFETA_PASSWORD, false, true, tEnvio, envio, destino).Result;

                if (result == null)
                {
                    result = res;
                }
                else
                {
                    for (int j = 0; j < result.Length; j++)
                    {
                        for (int k = 0; k < result[j].TipoServicio.Length; k++)
                        {
                            result[j].TipoServicio[k].CostoTotal += res[j].TipoServicio[k].CostoTotal;
                        }
                    }
                }

            }

            #endregion

            return result;
        }
    }
}
