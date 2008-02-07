using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web.UI;

namespace NHibernate.Burrow.WebUtil {
    public abstract class StatefulFieldProcessor {
        private static readonly IDictionary<Type, IDictionary<FieldInfo, StatefulField>>
            fieldInfoCache = new Dictionary<Type, IDictionary<FieldInfo, StatefulField>>();

        private static readonly object lockObj = new object();

        private readonly Control ctl;
        private StateBag _viewState;

        public StatefulFieldProcessor(Control c) {
            ctl = c;
        }

        protected StateBag ViewState {
            get {
                if (_viewState == null) {
                    PropertyInfo pi =
                        ctl.GetType().GetProperty("ViewState",
                                                  BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    _viewState = (StateBag) pi.GetValue(ctl, new object[0]);
                }
                return _viewState;
            }
        }

        public Control Control {
            get { return ctl; }
        }

        public void Process() {
            //if (Control is UserControl || Control is Page) // don't limit
            DoProcess();
            foreach (Control control in Control.Controls)
                CreateSubProcessor(control).Process();
        }

        protected abstract StatefulFieldProcessor CreateSubProcessor(Control c);

        protected abstract void DoProcess();

        /// <summary>
        /// Get the FieldInfo - Attribute pairs that have the customer attribute of type <typeparamref name="AT"/> 
        /// </summary>
        /// <typeparam name="AT"></typeparam>
        /// <returns></returns>
        protected IDictionary<FieldInfo, AT> GetFieldInfo<AT>() where AT : Attribute {
            IDictionary<FieldInfo, AT> retVal = new Dictionary<FieldInfo, AT>();
            foreach (FieldInfo fi in GetFields())
                foreach (AT a in Attribute.GetCustomAttributes(fi, typeof (AT)))
                    retVal.Add(fi, a);
            return retVal;
        }

        protected IDictionary<FieldInfo, StatefulField> GetStatefulFields() {
            IDictionary<FieldInfo, StatefulField> retVal;
            Type controlType = Control.GetType();
            if (!fieldInfoCache.TryGetValue(controlType, out retVal))
                fieldInfoCache[controlType] = retVal = GetFieldInfo<StatefulField>();
            return retVal;
        }

        private FieldInfo[] GetFields() {
            return Control.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        }
    }

    public class StatefulFieldLoader : StatefulFieldProcessor {
        public StatefulFieldLoader(Control c) : base(c) {}

        protected override void DoProcess() {
            foreach (KeyValuePair<FieldInfo, StatefulField> p in GetStatefulFields()) {
                StatefulField vsf = p.Value;
                object toSet = ViewState[p.Key.Name];
                if (vsf.Interceptor != null)
                    toSet = vsf.Interceptor.OnLoad(toSet);
                p.Key.SetValue(Control, toSet);
            }
        }

        protected override StatefulFieldProcessor CreateSubProcessor(Control c) {
            return new StatefulFieldLoader(c);
        }
    }

    public class StatefulFieldSaver : StatefulFieldProcessor {
        public StatefulFieldSaver(Control c) : base(c) {}

        protected override void DoProcess() {
            foreach (KeyValuePair<FieldInfo, StatefulField> p in GetStatefulFields()) {
                StatefulField vsf = p.Value;
                object toSave = p.Key.GetValue(Control);
                object objectInViewState = ViewState[p.Key.Name];
                if (vsf.Interceptor != null)
                    toSave = vsf.Interceptor.OnSave(toSave, objectInViewState);
                ViewState[p.Key.Name] = toSave;
            }
        }

        protected override StatefulFieldProcessor CreateSubProcessor(Control c) {
            return new StatefulFieldSaver(c);
        }
    }
}