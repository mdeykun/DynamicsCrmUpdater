using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmWebResourcesUpdater
{
    class DteInitializer : IVsShellPropertyEvents
    {
        private IVsShell shellService;
        private uint cookie;
        private Action callback;

        internal DteInitializer(IVsShell shellService, Action callback)
        {
            int hr;

            this.shellService = shellService;
            this.callback = callback;

            // Set an event handler to detect when the IDE is fully initialized
            hr = this.shellService.AdviseShellPropertyChanges(this, out this.cookie);

            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(hr);
        }

        int IVsShellPropertyEvents.OnShellPropertyChange(int propid, object var)
        {
            int hr;
            bool isZombie;

            if (propid == (int)__VSSPROPID.VSSPROPID_Zombie)
            {
                isZombie = (bool)var;

                if (!isZombie)
                {
                    // Release the event handler to detect when the IDE is fully initialized
                    hr = this.shellService.UnadviseShellPropertyChanges(this.cookie);

                    Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(hr);

                    this.cookie = 0;

                    callback.Invoke();
                }
            }
            return VSConstants.S_OK;
        }
    }
}
