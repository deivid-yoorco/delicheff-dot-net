using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Payments.Visa.Utils
{
    public static class RequestUtils
    {
        public static readonly string PRODUCTION_HOST = "api.cybersource.com";
        public static readonly string SANDBOX_HOST = "apitest.cybersource.com";
        public static readonly string CREATE_CUSTOMER_RESOURCE = "/tms/v2/customers";
        public static readonly string DECISION_MANAGER_RESOURCE = "/risk/v1/decisions";
        public static readonly string PAYMENT_AUTHORIZATION_RESOURCE = "/pts/v2/payments";

        public static StringBuilder GenerateSignature(string digest,
            string gmtDateTime,
            string method,
            string resource,
            string requestHost,
            string merchandId,
            string merchantSecretKey,
            string merchantApiKey)
        {
            StringBuilder signatureHeaderValue = new StringBuilder();

            string algorithm = "HmacSHA256";
            string postHeaders = "host date (request-target) digest v-c-merchant-id";
            string getHeaders = "host date (request-target) v-c-merchant-id";
            string request = method + " " + resource;

            try
            {
                // Generate the Signature
                StringBuilder signatureString = new StringBuilder();
                signatureString.Append('\n');
                signatureString.Append("host");
                signatureString.Append(": ");
                signatureString.Append(requestHost);
                signatureString.Append('\n');
                signatureString.Append("date");
                signatureString.Append(": ");
                signatureString.Append(gmtDateTime);
                signatureString.Append('\n');
                signatureString.Append("(request-target)");
                signatureString.Append(": ");
                signatureString.Append(request);

                if (!string.IsNullOrWhiteSpace(digest))
                {
                    signatureString.Append('\n');
                    signatureString.Append("digest");
                    signatureString.Append(": ");
                    signatureString.Append(digest);
                }

                signatureString.Append('\n');
                signatureString.Append("v-c-merchant-id");
                signatureString.Append(": ");
                signatureString.Append(merchandId);
                signatureString.Remove(0, 1);

                byte[] signatureByteString = Encoding.UTF8.GetBytes(signatureString.ToString());

                byte[] decodedKey = Convert.FromBase64String(merchantSecretKey);

                HMACSHA256 aKeyId = new HMACSHA256(decodedKey);

                byte[] hashmessage = aKeyId.ComputeHash(signatureByteString);
                string base64EncodedSignature = Convert.ToBase64String(hashmessage);

                signatureHeaderValue.Append("keyid=\"" + merchantApiKey + "\"");
                signatureHeaderValue.Append(", algorithm=\"" + algorithm + "\"");

                if (string.IsNullOrWhiteSpace(digest))
                {
                    signatureHeaderValue.Append(", headers=\"" + getHeaders + "\"");
                }
                else
                {
                    signatureHeaderValue.Append(", headers=\"" + postHeaders + "\"");
                }

                signatureHeaderValue.Append(", signature=\"" + base64EncodedSignature + "\"");
            }
            catch (Exception ex)
            {
                Debugger.Break();
            }

            return signatureHeaderValue;
        }
    }
}
