using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using NHibernate.Burrow.WebUtil.Impl;

namespace NHibernate.Burrow.WebUtil {
	internal class StatefulFieldPageModule
	{
        private readonly Page page;
        protected bool dataLoaded = false;
		private readonly GlobalPlaceHolder gph;
		public StatefulFieldPageModule(Page page, GlobalPlaceHolder globalPlaceHolder)
		{
            this.page = page;
        	gph = globalPlaceHolder;
            page.PreLoad += new EventHandler(page_PreLoad);
            page.PreRenderComplete += new EventHandler(page_PreRenderComplete);
        }

        private void page_PreRenderComplete(object sender, EventArgs e) {
            new StatefulFieldSaver(page, gph.Holder).Process();
        }

        private void page_PreLoad(object sender, EventArgs e) {
            if (!page.IsPostBack || dataLoaded)
                return;
            dataLoaded = true;
			new StatefulFieldLoader(page,gph.Holder).Process();
        }
    }
}