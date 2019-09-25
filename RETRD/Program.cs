using RETRD.Extensions;
using RETRD.Helpers;
using RETRD.Models.Configuration;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Text;

namespace RETRD
{
    class Program
    {
        static string exportFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        static void Main(string[] args)
        {
            /*var fuck = new RDCManager();

            var what = fuck.Serialize(Encoding.UTF8);*/

            var wow = ExportConfigurationFromActiveDirectory(Console.ReadLine());

            var neat = wow.Serialize(Encoding.UTF8);


            /*Console.WriteLine("Enter an LDAP Path");

            var ldapPath = Console.ReadLine();
            */
            Console.ReadKey();
        }

        /// <summary>
        /// Exports configuration from active directory
        /// </summary>
        static RDCManager ExportConfigurationFromActiveDirectory(string ldap)
        {
            using(var entry = new DirectoryEntry(StringHelpers.FormatLdapPath(ldap)))
            {
                return new RDCManager
                {
                    File = new File
                    {
                        Properties = new FileProperties
                        {
                            Name = "Tölvur"
                        },
                        Groups = GetGroupsFromOU(entry),
                        Servers = GetComputersFromOU(entry)
                    }
                };
            }
        }

        /// <summary>
        /// Gets organizational units and casts them as groups
        /// </summary>
        static List<Group> GetGroupsFromOU(DirectoryEntry ou)
        {
            var groups = ou
                    .Children
                    .Cast<DirectoryEntry>()
                    .Where(d => d.SchemaClassName == "organizationalUnit")
                    .Select(d => (Group)d)
                    .ToList();

            // Get underlying groups and computers
            foreach (var group in groups)
            {
                group.Groups = GetGroupsFromOU(new DirectoryEntry(group.Path));
                group.Servers = GetComputersFromOU(new DirectoryEntry(group.Path));
            }

            return groups;
        }

        /// <summary>
        /// Gets all computers in an organizational unit and casts them as servers
        /// </summary>
        /// <param name="ou"></param>
        /// <returns></returns>
        static List<Server> GetComputersFromOU(DirectoryEntry ou)
        {
            return ou
                .Children
                .Cast<DirectoryEntry>()
                .Where(c => c.SchemaClassName == "computer")
                .Select(c => (Server)c)
                .ToList();
        }
    }
}
