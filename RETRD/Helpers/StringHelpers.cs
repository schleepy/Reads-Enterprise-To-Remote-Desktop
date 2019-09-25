namespace RETRD.Helpers
{
    public static class StringHelpers
    {
        /// <summary>
        /// Simply makes sure that there is an LDAP:// prefix to the path
        /// </summary>
        public static string FormatLdapPath(string ldapPath)
        {
            if (ldapPath.StartsWith("LDAP://")) return ldapPath;

            return $"LDAP://{ldapPath}";
        }

        /// <summary>
        /// Removes CN=,OU=,DC= prefixes from the Name Property of DirectoryEntry
        /// 
        /// The name of this method and what it does is sort of contradictory
        /// because 
        /// </summary>
        /// <seealso cref="System.DirectoryServices.DirectoryEntry.Name"/>
        public static string RemoveLdapPrefix(string ldapPath)
        {
            if (!ldapPath.Contains("=")) return ldapPath;

            return ldapPath.Substring(3);
        }
    }
}
