using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnarcLabs.WiiSharp.Native
{
    internal class Constants
    {
        /// <summary>
        /// The vendor ID for all Wii remotes.
        /// </summary>
        public const int WMVendorID = 0x057E;
        /// <summary>
        /// The product ID for early Wii remotes.
        /// </summary>
        public const int WMProductID = 0x0306;
        /// <summary>
        /// The product ID for post-2011 Wii remotes.
        /// </summary>
        public const int TRProductID = 0x0330;
    }
}
