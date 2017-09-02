using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;

namespace Bro3AutoAttackTool
{
    public class WebBrowserEx : WebBrowser
    {

        private AxHost.ConnectionPointCookie cookie;
        private WebBrowser2EventHelper helper;
        public delegate void WebBrowserNewWindow3EventHandler(object sender, WebBrowserNewWindow3EventArgs e);
        public event WebBrowserNewWindow3EventHandler NewWindow3;

        private class WebBrowser2EventHelper : StandardOleMarshalObject, DWebBrowserEvents2
        {
            private WebBrowserEx parent;

            public WebBrowser2EventHelper(WebBrowserEx obj) { parent = obj; }

            public void NewWindow3(ref object ppDisp, ref bool cancel, UInt32 dwFlags, string bstrUrlContext, string bstrUrl)
            {
                WebBrowserNewWindow3EventArgs e = new WebBrowserNewWindow3EventArgs(ref ppDisp, bstrUrlContext, bstrUrl);
                parent.OnNewWindow3(e);
                ppDisp = e.ppDisp;
                cancel = e.Cancel;
            }
        }

        protected void OnNewWindow3(WebBrowserNewWindow3EventArgs e)
        {
            if (NewWindow3 != null) { NewWindow3(this, e); }
        }

        protected override void CreateSink()
        {
            base.CreateSink();
            helper = new WebBrowser2EventHelper(this);
            cookie = new AxHost.ConnectionPointCookie(this.ActiveXInstance, helper, typeof(DWebBrowserEvents2));
        }

        protected override void DetachSink()
        {
            if (cookie != null)
            {
                cookie.Disconnect();
                cookie = null;
            }
            base.DetachSink();
        }
    }

    [ComImport, Guid("34A715A0-6587-11D0-924A-0020AFC7AC4D"), InterfaceType(ComInterfaceType.InterfaceIsIDispatch), TypeLibType(TypeLibTypeFlags.FHidden)]
    public interface DWebBrowserEvents2
    {
        [DispId(273)]
        void NewWindow3([In, Out, MarshalAs(UnmanagedType.IDispatch)] ref object pDisp, [In, Out] ref bool cancel, [In] UInt32 dwFlags, [In] string bstrUrlContext, [In] string bstrUrl);
    }

    public class WebBrowserNewWindow3EventArgs : CancelEventArgs
    {
        private object ppDispValue;
        private string bstrUrlContextValue;
        private string bstrUrlValue;
        
        public object ppDisp
        {
            get { return ppDispValue; }
            set { ppDispValue = value; }
        }

        public string bstrUrlContext
        {
            get { return bstrUrlContextValue; }
            set { bstrUrlContextValue = value; }
        }

        public string bstrUrl
        {
            get { return bstrUrlValue; }
            set { bstrUrlValue = value; }
        }

        public WebBrowserNewWindow3EventArgs(ref object ppDisp, string bstrUrlContext, string bstrUrl)
        {
            ppDispValue = ppDisp;
            bstrUrlContextValue = bstrUrlContext;
            bstrUrlValue = bstrUrl;
        }
    }
}