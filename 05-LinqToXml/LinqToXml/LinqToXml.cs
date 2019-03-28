using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace LinqToXml
{
    public static class LinqToXml
    {
        /// <summary>
        /// Creates hierarchical data grouped by category
        /// </summary>
        /// <param name="xmlRepresentation">Xml representation (refer to CreateHierarchySourceFile.xml in Resources)</param>
        /// <returns>Xml representation (refer to CreateHierarchyResultFile.xml in Resources)</returns>
        public static string CreateHierarchy(string xmlRepresentation)
        {
            return new XElement("Root",
                XElement.Parse(xmlRepresentation)
                    .Elements("Data")
                    .GroupBy(x => (string)x.Element("Category"))
                    .Select(x => 
                    new XElement("Group",
                        new XAttribute("ID", x.Key),
                        x.Select(y =>
                        new XElement("Data",
                            y.Element("Quantity"),
                            y.Element("Price")))
                    ))
            ).ToString();
        }

        /// <summary>
        /// Get list of orders numbers (where shipping state is NY) from xml representation
        /// </summary>
        /// <param name="xmlRepresentation">Orders xml representation (refer to PurchaseOrdersSourceFile.xml in Resources)</param>
        /// <returns>Concatenated orders numbers</returns>
        /// <example>
        /// 99301,99189,99110
        /// </example>
        public static string GetPurchaseOrders(string xmlRepresentation)
        {
            XNamespace aw = "http://www.adventure-works.com";

            return string.Join(",", XElement.Parse(xmlRepresentation)
                .Elements(aw + "PurchaseOrder")
                .Where(x => (string)x.Elements(aw + "Address")
                .Where(y => (string)y.Attribute(aw + "Type") == "Shipping")
                .FirstOrDefault()?.Element(aw + "State") == "NY")
                .Select(x => (string)x.Attribute(aw + "PurchaseOrderNumber"))
            );
        }

        /// <summary>
        /// Reads csv representation and creates appropriate xml representation
        /// </summary>
        /// <param name="customers">Csv customers representation (refer to XmlFromCsvSourceFile.csv in Resources)</param>
        /// <returns>Xml customers representation (refer to XmlFromCsvResultFile.xml in Resources)</returns>
        public static string ReadCustomersFromCsv(string customers)
        {
            return new XElement("Root",
                customers.Split(new string[] { "\r\n" }, StringSplitOptions.None)
                .Select(x => x.Split(','))
                .Select(x =>
                  new XElement("Customer",
                      new XAttribute("CustomerID", x[0]),
                      new XElement("CompanyName", x[1]),
                      new XElement("ContactName", x[2]),
                      new XElement("ContactTitle", x[3]),
                      new XElement("Phone", x[4]),
                      new XElement("FullAddress",
                          new XElement("Address", x[5]),
                          new XElement("City", x[6]),
                          new XElement("Region", x[7]),
                          new XElement("PostalCode", x[8]),
                          new XElement("Country", x[9])
                    )
                ))
            ).ToString();
        }

        /// <summary>
        /// Gets recursive concatenation of elements
        /// </summary>
        /// <param name="xmlRepresentation">Xml representation of document with Sentence, Word and Punctuation elements. (refer to ConcatenationStringSource.xml in Resources)</param>
        /// <returns>Concatenation of all this element values.</returns>
        public static string GetConcatenationString(string xmlRepresentation)
        {
            return XElement.Parse(xmlRepresentation).Value;
        }

        /// <summary>
        /// Replaces all "customer" elements with "contact" elements with the same childs
        /// </summary>
        /// <param name="xmlRepresentation">Xml representation with customers (refer to ReplaceCustomersWithContactsSource.xml in Resources)</param>
        /// <returns>Xml representation with contacts (refer to ReplaceCustomersWithContactsResult.xml in Resources)</returns>
        public static string ReplaceAllCustomersWithContacts(string xmlRepresentation)
        {
            //return new XElement(
            //  "Document",
            //      XElement.Parse(xmlRepresentation)
            //      .Elements("customer")
            //      .Select(x =>  new XElement("contact", x.Descendants()))
            //  ).ToString();

            return new XElement(
                "Document", 
                    XElement.Parse(xmlRepresentation)
                    .Elements("customer")
                    .Select(x =>
                    {
                        x.Name = "contact";
                        return x;
                    })
            ).ToString();
        }

        /// <summary>
        /// Finds all ids for channels with 2 or more subscribers and mark the "DELETE" comment
        /// </summary>
        /// <param name="xmlRepresentation">Xml representation with channels (refer to FindAllChannelsIdsSource.xml in Resources)</param>
        /// <returns>Sequence of channels ids</returns>
        public static IEnumerable<int> FindChannelsIds(string xmlRepresentation)
        {
            return XElement.Parse(xmlRepresentation)
                .Elements("channel")
                .Where(x => 
                    x.Elements("subscriber").Count() > 1 &&
                    x.Nodes().OfType<XComment>().Any(y => y.Value == "DELETE")
                )
                .Select(x => (int)x.Attribute("id"));
        }

        /// <summary>
        /// Sort customers in docement by Country and City
        /// </summary>
        /// <param name="xmlRepresentation">Customers xml representation (refer to GeneralCustomersSourceFile.xml in Resources)</param>
        /// <returns>Sorted customers representation (refer to GeneralCustomersResultFile.xml in Resources)</returns>
        public static string SortCustomers(string xmlRepresentation)
        {
            return new XElement(
                "Root",
                    XElement.Parse(xmlRepresentation)
                    .Elements("Customers")
                    .OrderBy(x => (string)x.Element("FullAddress").Element("Country"))
                    .ThenBy(x => (string)x.Element("FullAddress").Element("City"))
            ).ToString();
        }

        /// <summary>
        /// Gets XElement flatten string representation to save memory
        /// </summary>
        /// <param name="xmlRepresentation">XElement object</param>
        /// <returns>Flatten string representation</returns>
        /// <example>
        ///     <root><element>something</element></root>
        /// </example>
        public static string GetFlattenString(XElement xmlRepresentation)
        {
            return xmlRepresentation.ToString(SaveOptions.DisableFormatting);
        }

        /// <summary>
        /// Gets total value of orders by calculating products value
        /// </summary>
        /// <param name="xmlRepresentation">Orders and products xml representation (refer to GeneralOrdersFileSource.xml in Resources)</param>
        /// <returns>Total purchase value</returns>
        public static int GetOrdersValue(string xmlRepresentation)
        {
            return XElement.Parse(xmlRepresentation)
                .Element("Orders")
                .Elements("Order")
                .Join(XElement.Parse(xmlRepresentation)
                    .Element("products").Elements(),
                    order => (string)order.Element("product"),
                    price => (string)price.Attribute("Id"), 
                    (order, price) => new { Price = price.Attribute("Value") }
                ).Sum(x => (int)x.Price);
        }
    }
}