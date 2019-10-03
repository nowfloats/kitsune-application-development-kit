using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kitsune.Models
{
    public class MonthlyInvoiceModel: MongoEntity
    {
        public Context context { get; set; }
        public string link { get; set; }
    }
    public class address
    {
        public string AddressDetail { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string _cls { get; set; }
    }

    public class Record
    {
        public string date { get; set; }
        public string component { get; set; }
        public string quantity { get; set; }
        public string currency { get; set; }
        public double amount { get; set; }
        public string tariff_string { get; set; }
    }

    public class ChargeComponent
    {
        public string name { get; set; }
        public double cost { get; set; }
        public List<Record> records { get; set; }
    }

    public class InvoiceFromAddress
    {
        public string AddressDetail { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string Pin { get; set; }
        public string _cls { get; set; }
    }

    public class InvoiceFinanceNumber
    {
        public string PAN { get; set; }
        public string TAN { get; set; }
        public string CIN { get; set; }
        public string GSTIN { get; set; }
    }

    public class Tax
    {
        public double tax_rate { get; set; }
        public double tax_amount { get; set; }
        public string tax_code { get; set; }
        public string tax_name { get; set; }
    }

    public class Context
    {
        public string user_name { get; set; }
        public string user_id { get; set; }
        public address address { get; set; }
        public string invoice_number { get; set; }
        public string date_issued { get; set; }
        public string period { get; set; }
        public List<ChargeComponent> charge_components { get; set; }
        public string invoice_from_company { get; set; }
        public string invoice_from_parent_company { get; set; }
        public InvoiceFromAddress invoice_from_address { get; set; }
        public List<InvoiceFinanceNumber> invoice_finance_numbers { get; set; }
        public double charges { get; set; }
        public string currency { get; set; }
        public double total { get; set; }
        public List<Tax> taxes { get; set; }
    }

    
}
