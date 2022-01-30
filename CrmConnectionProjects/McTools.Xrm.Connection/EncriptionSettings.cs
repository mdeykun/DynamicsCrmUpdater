using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace McTools.Xrm.Connection
{
    public class EncriptionSettings
    {
        public const string CryptoHashAlgorythm = "SHA1";
        public const string CryptoInitVector = "ahC3@bCa2Didfc3d";
        public const int CryptoKeySize = 256;
        public const string CryptoPassPhrase = "MsCrmTools";
        public const int CryptoPasswordIterations = 2;
        public const string CryptoSaltValue = "Tanguy 92*";
    }
}
