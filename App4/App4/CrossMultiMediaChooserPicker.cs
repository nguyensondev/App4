using App4.test;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace App4
{
    public class CrossMultiMediaChooserPicker
    {
        private static readonly Lazy<IMultiMediaChooserPicker> Implementation = new Lazy<IMultiMediaChooserPicker>(CreateModalView,
            System.Threading.LazyThreadSafetyMode.PublicationOnly);

        /// <summary>
        /// Current settings to use
        /// </summary>
        public static IMultiMediaChooserPicker Current
        {
            get
            {
                var ret = Implementation.Value;
                if (ret == null)
                {
                    throw NotImplementedInReferenceAssembly();
                }
                return ret;
            }
        }

        private static IMultiMediaChooserPicker CreateModalView()
        {
#if PORTABLE
            return null;
#else
            return DependencyService.Get<IMultiMediaChooserPicker>();
#endif
        }

        internal static Exception NotImplementedInReferenceAssembly()
        {
            return
                new NotImplementedException(
                    "This functionality is not implemented in the portable version of this assembly.  You should reference the NuGet package from your main application project in order to reference the platform-specific implementation.");
        }
    }
}
