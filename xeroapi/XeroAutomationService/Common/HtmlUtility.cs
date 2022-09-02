using CosmicApiModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XeroAutomationService.Common
{
    public class HtmlUtility
    {
        public static string getInvoiceTable(List<XeroPostResponse> lstInvoice)
        {
            StringBuilder sb = new StringBuilder();
            //Table start.
            sb.Append("<table align='center' cellpadding='5' cellspacing='0' style='border: 1px solid #ccc;font-size: 9pt;font-family:Arial'>");

            //Adding HeaderRow.
            sb.Append("<tr>");
            sb.Append("<th style='background-color: #e3e3e3;border: 1px solid #ccc;padding:5;text-align:center'>Invoice</th>");
            sb.Append("<th style='background-color: #e3e3e3;border: 1px solid #ccc;padding:5;text-align:center'>Supplier</th>");
            sb.Append("<th style='background-color: #e3e3e3;border: 1px solid #ccc;padding:5;text-align:center'>Response</th>");
            sb.Append("<th style='background-color: #e3e3e3;border: 1px solid #ccc;padding:5;text-align:center'>Uploaded Date</th>");
            sb.Append("</tr>");
            foreach (var invoiceresponse in lstInvoice)
            {
                sb.Append("<tr>");
                sb.Append("<td style='width:100px;border: 1px solid #ccc;text-align:center;color:black'>" + invoiceresponse.InvoiceNo + "</td>");
                sb.Append("<td style='width:100px;border: 1px solid #ccc;text-align:center;color:black'>" + invoiceresponse.Supplier + "</td>");
                sb.Append("<td style='width:100px;border: 1px solid #ccc;text-align:center;color:black'>" + invoiceresponse.ReponseFromXero + "</td>");
                sb.Append("<td style='width:100px;border: 1px solid #ccc;text-align:center;color:black'>" + invoiceresponse.uploadedDate.ToString("dd/MM/yyyy") + "</td>");
                sb.Append("</tr>");
            }

            //Table end.
            sb.Append("</table>");

            return sb.ToString();
        }

        public static string getScanInvoiceTable(List<QBResponse> lstInvoice)
        {
            StringBuilder sb = new StringBuilder();
            //Table start.
            sb.Append("<table align='center' cellpadding='5' cellspacing='0' style='border: 1px solid #ccc;font-size: 9pt;font-family:Arial'>");

            //Adding HeaderRow.
            sb.Append("<tr>");
            sb.Append("<th style='background-color: #e3e3e3;border: 1px solid #ccc;padding:5;text-align:center'>Invoice</th>");
            sb.Append("<th style='background-color: #e3e3e3;border: 1px solid #ccc;padding:5;text-align:center'>Supplier</th>");
            sb.Append("<th style='background-color: #e3e3e3;border: 1px solid #ccc;padding:5;text-align:center'>Status</th>");
            sb.Append("<th style='background-color: #e3e3e3;border: 1px solid #ccc;padding:5;text-align:center'>Uploaded Date</th>");
            sb.Append("</tr>");
            foreach (var invoiceresponse in lstInvoice)
            {
                sb.Append("<tr>");
                sb.Append("<td style='width:100px;border: 1px solid #ccc;text-align:center;color:black'>" + invoiceresponse.InvoiceNo + "</td>");
                sb.Append("<td style='width:100px;border: 1px solid #ccc;text-align:center;color:black'>" + invoiceresponse.InvoiceSupplier + "</td>");
                sb.Append("<td style='width:100px;border: 1px solid #ccc;text-align:center;color:black'>" + invoiceresponse.Message + "</td>");
                sb.Append("<td style='width:100px;border: 1px solid #ccc;text-align:center;color:black'>" + invoiceresponse.uploadedDate.ToString("dd/MM/yyyy") + "</td>");
                sb.Append("</tr>");
            }

            //Table end.
            sb.Append("</table>");

            return sb.ToString();
        }
    }
}
