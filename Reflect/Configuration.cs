using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;

//Example Config.xml, needs to be created in the App_Data folder.
/*
<?xml version="1.0" encoding="utf-8" ?>
<Domains>
  <Domain Target="localhost">
    <Configuration>
      <add key="ConnectionString" value="Server=LOCALHOST;Database=ReflectDB"/>
      <add key="CryptoLevel" value="0"/>
      <add key="DBLog" value="0"/>
      <add key="CryptoHash" value=""/>
      <add key="CryptoSalt" value=""/>
      <add key="CryptoVI" value=""/>
    </Configuration>
  </Domain>
</Domains>
*/

namespace Reflect
{
    public class Configuration
    {
        public static string GetConfiguration(string Key)
        {
            string Domain = (HttpContext.Current.Request.IsLocal) ? "localhost" : HttpContext.Current.Request.Url.Host;
            return GetConfiguration(Key, Domain);
        }

        public static string GetConfiguration(string Key, string Domain)
        {
            string Value = null;
            try
            {
                XmlDocument ConfigDocument = new XmlDocument();
                ConfigDocument.Load(System.Web.Hosting.HostingEnvironment.MapPath("~/App_Data/Config.xml"));
                XmlNode DomainNode = ConfigDocument.DocumentElement.SelectSingleNode("//Domain[@Target='" + Domain + "']");
                if (DomainNode != null)
                {
                    XmlNode ConfigNode = DomainNode.SelectSingleNode("//add[@key='" + Key + "']");
                    if (ConfigNode != null)
                        Value = ConfigNode.Attributes["value"].Value;
                }
            }
            catch (Exception)
            {

            }
            return Value;
        }
    }
}
