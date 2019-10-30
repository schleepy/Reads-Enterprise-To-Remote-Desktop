using RETRD.Extensions;
using RETRD.Helpers;
using RETRD.Models.Configuration;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.IO;
using System.Linq;
using System.Text;
using File = RETRD.Models.Configuration.File;

namespace RETRD
{
    class Program
    {
        private static string _outputDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        static string OutputDirectory
        {
            get
            {
                return _outputDirectory;
            }
            set
            {
                if (!Directory.Exists(value))
                {
                    throw new DirectoryNotFoundException($"Directory {value} is not a valid path");
                }
            }
        }

        static string OutputFilename { get; set; } = "Computers";
        static string Domain { get; set; }
        static string Username { get; set; }
        static string Password { get; set; }
        static string LDAPPAth { get; set; }

        static void Main(string[] args)
        {
            // Basic unhandled exception trapper
            System.AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionTrapper;

            args = new string[2] { "--ldap-path", "OU=Tolvur,DC=2t,DC=local" };

            ParseArguments(args);

            var configuration = ExportConfigurationFromActiveDirectory(LDAPPAth);

            var serializedConfiguration = configuration.Serialize(Encoding.UTF8);

            var filePath = $"{OutputDirectory}\\{OutputFilename}.rdg";

            // Delete the file if it already exists
            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);

            System.IO.File.WriteAllText(filePath, serializedConfiguration);

            Console.ReadKey();
        }

        private static void UnhandledExceptionTrapper(object sender, UnhandledExceptionEventArgs e)
        {
            Console.Error.Write($"'{((Exception)e.ExceptionObject).Message}'\nPress any key to exit");
            Console.ReadKey();
            Environment.Exit(-1);
        }

        /// <summary>
        /// Parers for input arguments
        /// </summary>
        static void ParseArguments(string[] args)
        {
            for (int i = 0; i < args.Length; i += 2)
            {
                var argument = args[i].ToLower();
                var value = args[i + 1];

                try
                {
                    switch (argument)
                    {
                        case "--domain":
                            Domain = value;
                            break;

                        case "--username":
                            Username = value;
                            break;

                        case "--password":
                            Password = value;
                            break;

                        case "--ldap-path":
                            LDAPPAth = value;
                            break;

                        case "--output-filename":
                            OutputFilename = value;
                            break;

                        case "--output-directory":
                            OutputDirectory = value;
                            break;

                        default:
                            throw new ArgumentNullException($"'{argument}' is not a valid argument");
                    }
                } 
                catch (Exception e)
                {
                    Console.Error.Write($"'{e.Message}'\nPress any key to exit");
                    Console.ReadKey();
                    Environment.Exit(-1);
                }
            }
        }

        /// <summary>
        /// Exports configuration from active directory
        /// </summary>
        static RDCManager ExportConfigurationFromActiveDirectory(string ldap)
        {
            if (string.IsNullOrEmpty(ldap)) throw new ArgumentNullException("LDAPPath can not be null");

            using(var entry = new DirectoryEntry(StringHelpers.FormatLdapPath(ldap)))
            {
                return new RDCManager
                {
                    File = new File
                    {
                        Properties = new FileProperties
                        {
                            Name = OutputFilename
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
