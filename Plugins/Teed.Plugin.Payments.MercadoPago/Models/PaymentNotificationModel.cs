using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teed.Plugin.Payments.MercadoPago.Models
{
    public class PaymentNotificationModel
    {
        public string id { get; set; }
        public DateTime date_created { get; set; }
        public DateTime date_approved { get; set; }
        public DateTime date_last_updated { get; set; }
        public DateTime date_of_expiration { get; set; }
        public DateTime money_release_date { get; set; }
        public string operation_type { get; set; }
        public string issuer_id { get; set; }
        public string payment_method_id { get; set; }
        public string payment_type_id { get; set; }
        public string status { get; set; }
        public string status_detail { get; set; }
        public string currency_id { get; set; }
        public string description { get; set; }
        public bool live_mode { get; set; }
        public string sponsor_id { get; set; }
        public string authorization_code { get; set; }
        public string money_release_schema { get; set; }
        public string counter_currency { get; set; }
        public string collector_id { get; set; }
        public PayerModel payer { get; set; }
        public string metadata { get; set; }
        public string additional_info { get; set; }
        public string order { get; set; }
        public string external_reference { get; set; }
        public string transaction_amount { get; set; }
        public string transaction_amount_refunded { get; set; }
        public string coupon_amount { get; set; }
        public string differential_pricing_id { get; set; }
        public string deduction_schema { get; set; }
        public TransactionDetailsModel transaction_details { get; set; }
        public string fee_details { get; set; }
        public string captured { get; set; }
        public string binary_mode { get; set; }
        public string call_for_authorize_id { get; set; }
        public string statement_descriptor { get; set; }
        public string installments { get; set; }
        public string card { get; set; }
        public string notification_url { get; set; }
        public string refunds { get; set; }
        public string processing_mode { get; set; }
        public string merchant_account_id { get; set; }
        public string acquirer { get; set; }
        public string merchant_number { get; set; }
        public string acquirer_reconciliation { get; set; }
    }

    public class PayerModel
    {
        public string type { get; set; }
        public string id { get; set; }
        public string email { get; set; }
        public IdentificationModel identification { get; set; }
        public PhoneModel phone { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string entity_type { get; set; }
    }

    public class IdentificationModel
    {
        public string type { get; set; }
        public string number { get; set; }
    }

    public class PhoneModel
    {
        public string area_code { get; set; }
        public string number { get; set; }
        public string extension { get; set; }
    }

    public class TransactionDetailsModel
    {
        public decimal net_received_amount { get; set; }
        public decimal total_paid_amount { get; set; }
        public decimal overpaid_amount { get; set; }
        public string external_resource_url { get; set; }
        public decimal installment_amount { get; set; }
        public string financial_institution { get; set; }
        public string payment_method_reference_id { get; set; }
        public string payable_deferral_period { get; set; }
        public string acquirer_reference { get; set; }
    }
}
