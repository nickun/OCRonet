using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ocronet.Dynamic.Utils;
using System.IO;
using Ocronet.Dynamic.IOData;
using Ocronet.Dynamic.Component;

namespace Ocronet.Dynamic.Interfaces
{
    /// <summary>
    /// Base class for OCR components.
    /// </summary>
    public abstract class IComponent
    {
        // parameter setting and loading
        private Dictionary<string, string> _params;
        private Dictionary<string, bool> _shown;
        private bool _checked;
        protected static System.Globalization.NumberFormatInfo Ni;  // invariant number format
        // misc information logged about the history of the component
        //private StringBuilder object_history;
        // saving and loading (if implemented)
        //private Narray<string> _wnames;
        //private Narray<IOWrapper> _wrappers;
        private Dictionary<string, IOWrapper> _wrappers_dict;


        public string verbose_pattern;

        public IComponent()
        {
            verbose_pattern = "%";
            _checked = false;
            if (!String.IsNullOrEmpty(Global.global_verbose_params))
            {
                verbose_pattern = Global.global_verbose_params;
            }
            _params = new Dictionary<string, string>();
            _shown = new Dictionary<string, bool>();
            //_wnames = new Narray<string>();
            //_wrappers = new Narray<IOWrapper>();
            _wrappers_dict = new Dictionary<string, IOWrapper>();
            // create invariant number format
            System.Globalization.CultureInfo ci = System.Globalization.CultureInfo.InstalledUICulture;
            Ni = (System.Globalization.NumberFormatInfo)ci.NumberFormat.Clone();
            Ni.NumberDecimalSeparator = ".";
        }

        /// <summary>
        /// reinitialize the component (e.g., after changing some parameters).
        /// </summary>
        public virtual void ReInit() {}

        /// <summary>
        /// interface name
        /// </summary>
        public virtual string Interface
        {
            get { return "IComponent"; }    // should override
        }

        /// <summary>
        /// object name
        /// </summary>
        public virtual string Name
        {
            get { return this.GetType().Name; }
        }

        /// <summary>
        /// brief description
        /// </summary>
        public virtual string Description
        {
            get { return this.GetType().FullName; }
        }

        /// <summary>
        /// print brief description
        /// </summary>
        public virtual void Print()
        {
            Logger.Default.Format("<{0} ({1})>", this.Name, this.GetType().FullName);
        }

        /// <summary>
        /// Print the parameters in some human-readable format.
        /// </summary>
        public virtual void PPrint()
        {
            foreach (string key in _params.Keys)
            {
                Logger.Default.Format("{0}_{1}={2}", this.Name, key, _params[key]);
            }
        }

        /// <summary>
        /// Print longer info to stdout
        /// </summary>
        public virtual void Info()
        {
            Logger.Default.WriteLine(Description);
            //Logger.Default.WriteLine(object_history.ToString());
        }

        #region Methods for manipulating/changing persistent components

        public int PersistLength()
        {
            return _wrappers_dict.Count();
        }

        public IComponent PersistComponentGet(string name)
        {
            if (_wrappers_dict.ContainsKey(name))
            {
                return _wrappers_dict[name].GetComponent();
            }
            else
                return null;
        }

        public void PersistSet(string name, IComponent value)
        {
            if (_wrappers_dict.ContainsKey(name))
            {
                _wrappers_dict[name].Set(value);
            }
        }

        #endregion // Methods for manipulating/changing persistent components

        #region persisting scalars

        public void Persist(ref int data, string name)
        {
            _wrappers_dict.Add(name, new ScalarIOWrapper<int>(ref data));
            //_wnames.Push(name);
            //_wrappers.Push((IOWrapper)new ScalarIOWrapper<int>(ref data));
        }

        public void Persist(ref float data, string name)
        {
            _wrappers_dict.Add(name, new ScalarIOWrapper<float>(ref data));
            //_wnames.Push(name);
            //_wrappers.Push((IOWrapper)new ScalarIOWrapper<float>(ref data));
        }

        public void Persist(ref double data, string name)
        {
            _wrappers_dict.Add(name, new ScalarIOWrapper<double>(ref data));
            //_wnames.Push(name);
            //_wrappers.Push((IOWrapper)new ScalarIOWrapper<double>(ref data));
        }

        public void Persist(ref short data, string name)
        {
            _wrappers_dict.Add(name, new ScalarIOWrapper<short>(ref data));
            //_wnames.Push(name);
            //_wrappers.Push((IOWrapper)new ScalarIOWrapper<short>(ref data));
        }

        public void Persist(ref bool data, string name)
        {
            _wrappers_dict.Add(name, new ScalarIOWrapper<bool>(ref data));
            //_wnames.Push(name);
            //_wrappers.Push((IOWrapper)new ScalarIOWrapper<bool>(ref data));
        }

        #endregion // persisting scalars

        #region persisting Narray

        public void Persist<T>(Narray<T> data, string name)
        {
            _wrappers_dict.Add(name, new NarrayIOWrapper<T>(data));
            //_wnames.Push(name);
            //_wrappers.Push((IOWrapper)new NarrayIOWrapper<T>(data));
        }

        public void Persist(Narray<IComponent> data, string name)
        {
            _wrappers_dict.Add(name, new ComponentListIOWrapper(data));
            //_wnames.Push(name);
            //_wrappers.Push((IOWrapper)new ComponentListIOWrapper(data));
        }

        #endregion // persisting Narray

        #region persisting IComponent

        public void Persist(ComponentContainer<IComponent> data, string name)
        {
            if (!_wrappers_dict.ContainsKey(name))
                _wrappers_dict.Add(name, new ComponentIOWrapper(data));
            else
                _wrappers_dict[name].Set(data.GetComponent());
        }

        #endregion // persisting IComponent

        #region persisting IOWrapper

        public void Persist(IOWrapper wrapper, string name)
        {
            if (!_wrappers_dict.ContainsKey(name))
                _wrappers_dict.Add(name, wrapper);
            else
                _wrappers_dict[name].Set(wrapper.GetComponent());
        }

        #endregion // persisting IComponent

        // saving and loading
        public virtual void Save(BinaryWriter writer)
        {
            BinIO.magic_write(writer, Name);
            this.PSave(writer);
            BinIO.string_write(writer, "<component>");
            foreach (string wname in _wrappers_dict.Keys)
            {
                BinIO.string_write(writer, "<item>");
                Global.Debugf("iodetail", "writing {0} {1}", Name, wname);
                BinIO.string_write(writer, wname);
                _wrappers_dict[wname].Save(writer);
                BinIO.string_write(writer, "</item>");
            }
            BinIO.string_write(writer, "</component>");
        }

        public virtual void Load(BinaryReader reader)
        {
            // before doing anything else, clear all the
            // persistent variables to their default state
            foreach (string wname in _wrappers_dict.Keys)
                _wrappers_dict[wname].Clear();
            BinIO.magic_read(reader, Name);
            this.PLoad(reader);
            string s;
            BinIO.string_read(reader, out s);
            if (s != "<component>")
                throw new Exception("Expected string: <component>");
            while (true)
            {
                // read tag
                BinIO.string_read(reader, out s);
                if (s == "</component>")
                    break;
                if (s != "<item>")
                    throw new Exception("Expected string: <item>");
                // read wrapper name
                BinIO.string_read(reader, out s);
                if (!_wrappers_dict.ContainsKey(s))
                    throw new Exception(String.Format("Wrapper name '{0}' is not persisted", s));
                // read wrapper data
                if (ComponentIO.level == 2)
                    Global.Debugf("info", "{0," + (ComponentIO.level) + "}loading {1}", "", s);
                _wrappers_dict[s].Load(reader);
                // read tag
                BinIO.string_read(reader, out s);
                if (s != "</item>")
                    throw new Exception("Expected string: </item>");
            }
            this.ReImport();
        }

        public virtual void Save(string path)
        {
            FileStream stream = new FileStream(path, FileMode.Create, FileAccess.Write);
            BinaryWriter writer = new BinaryWriter(stream);
            try
            {
                Save(writer);
            }
            finally
            {
                writer.Close();
                stream.Close();
            }
        }

        public virtual void Load(string path)
        {
            FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read);
            BinaryReader reader = new BinaryReader(stream);
            try
            {
                Load(reader);
            }
            finally
            {
                reader.Close();
                stream.Close();
            }
        }

        /// <summary>
        /// Define a string parameter for this component.  Parameters
        /// should be defined once in the constructor, together with
        /// a default value and a documentation string.
        /// Names starting with a '%' are not parameters, but rather
        /// information about the component computed while running
        /// (it's saved along with the parameters when saving the
        /// component).
        /// </summary>
        /// <param name="name">parameter name</param>
        /// <param name="value">parameter value</param>
        /// <param name="doc">comments</param>
        public void PDef(string name, string value, string doc = "")
        {
            if (name[0] == '%')
                throw new Exception(String.Format("pdef: {0} must not start with %", name));
            if (_params.ContainsKey(name))
                throw new Exception(String.Format("pdefs: {0}: parameter already defined", name));
            if (name.Contains('\n') || name.Contains('=') || value.Contains('\n'))
                throw new Exception(String.Format("pdef: '{0}'='{1}' incorrect symbols", name, value));
            _params[name] = value;
            Import(name, doc);
        }

        public void Import(string name, string doc = "")
        {
            string key = this.Name + "_" + name;
            bool altered = false;
            string evalue = Global.GetEnv(key);
            if (!String.IsNullOrEmpty(evalue))
            {
                if (_params.ContainsKey(name) && _params[name] != evalue)
                    altered = true;
                _params[name] = evalue;
            }
            if (!_shown.ContainsKey(key))
            {
                string showdoc = String.IsNullOrEmpty(doc) ? "" : "# " + doc;
                if (altered /*&& verbose_pattern == "?"*/)
                    Logger.Default.Format("param altered {0}={1} {2}",
                        key, _params[name], showdoc);
                else if (key.IndexOf(verbose_pattern) >= 0)
                {
                    Logger.Default.Format("param default {0}={1} {2}",
                        key, _params[name], showdoc);
                }
                _shown[key] = true;
            }
        }

        public void ReImport()
        {
            _shown.Clear();
            string[] keys = _params.Keys.ToArray();
            foreach (string key in keys)
                Import(key);
        }

        /// <summary>
        /// Count of parameters.
        /// </summary>
        public int PLength()
        {
            return _params.Count;
        }

        #region global parameters

        /// <summary>
        /// Set global environment for concrete object
        /// </summary>
        /// <param name="objname">see obj.Name</param>
        /// <param name="name">parameter name</param>
        /// <param name="value">parameter value</param>
        public static void GDef(string objname, string name, string value)
        {
            string varname = objname + "_" + name;
            Global.SetEnv(varname, value);
        }

        public static void GDef(string objname, string name, double value)
        {
            GDef(objname, name, value.ToString(Ni));
        }

        public static void GDef(string objname, string name, int value)
        {
            GDef(objname, name, value.ToString());
        }

        public static void GDef(string objname, string name, bool value)
        {
            GDef(objname, name, Convert.ToInt32(value).ToString());
        }

        #endregion // global parameters

        /// <summary>
        /// Return name of parameter at i position.
        /// </summary>
        /// <param name="i">position of paramener</param>
        /// <returns>name of parameter</returns>
        public string PName(int i)
        {
            return _params.Keys.ElementAt<string>(i);
        }

        public void PDef(string name, double value, string doc = "")
        {
            PDef(name, value.ToString(Ni), doc);
        }

        public void PDef(string name, int value, string doc = "")
        {
            PDef(name, value.ToString(), doc);
        }

        public void PDef(string name, bool value, string doc = "")
        {
            PDef(name, Convert.ToInt32(value).ToString(), doc);
        }

        public virtual bool PExists(string name)
        {
            return _params.ContainsKey(name);
        }


        /// <summary>
        /// Set a parameter; this allows changing the parameter after it
        /// has been defined.  It should be called by other parts of the
        /// system if they want to change a parameter value.
        /// These are virtual so that classes can forward them if necessary.
        /// </summary>
        public virtual void PSet(string name, string value)
        {
            if(name[0]!='%' && !_params.ContainsKey(name))
                throw new Exception(String.Format("pset: {0}: no such parameter", name));
            _params[name] = value;
            if (name.IndexOf(verbose_pattern) >= 0)
                Logger.Default.Format("set {0}_{1}={2}", this.Name, name, value);
        }

        public virtual void PSet(string name, double value)
        {
            PSet(name, value.ToString(Ni));
        }

        public virtual void PSet(string name, int value)
        {
            PSet(name, value.ToString(Ni));
        }

        public virtual void PSet(string name, bool value)
        {
            PSet(name, Convert.ToInt32(value).ToString());
        }

        /// <summary>
        /// Get a string paramter.  This can be called both from within the class
        /// implementation, as well as from external functions, in order to see
        /// what current parameter settings are.
        /// </summary>
        public string PGet(string name)
        {
            if (!_checked)
                CheckParameters();
            if(!_params.ContainsKey(name))
                throw new Exception(String.Format("pget: {0}: no such parameter", name));
            return _params[name];
        }

        public float PGetf(string name)
        {
            float value;
            string sval = PGet(name);
            if (!float.TryParse(sval, System.Globalization.NumberStyles.Any, Ni, out value))
                throw new Exception(String.Format("pgetf: {0}={1}: bad number format", name, sval));
            return value;
        }

        public int PGeti(string name)
        {
            int value;
            string sval = PGet(name);
            if (!int.TryParse(sval, out value))
                throw new Exception(String.Format("pgeti: {0}={1}: bad number format", name, sval));
            return value;
        }

        public bool PGetb(string name)
        {
            int sval = PGeti(name);
            return sval > 0;
        }

        /// <summary>
        /// Save the parameters to the string.
        /// <remarks>This should get called from save().
        /// The format is binary and not necessarily fit for human consumption.</remarks>
        /// </summary>
        public void PSave(BinaryWriter writer)
        {
            foreach (string key in _params.Keys)
            {
                string s = String.Format("{0}={1}", key, _params[key]);
                BinIO.string_write(writer, s);
                //writer.Write(String.Format("{0}={1}\n", key, _params[key]));
            }
            BinIO.string_write(writer, "END_OF_PARAMETERS=HERE");
            //writer.Write("END_OF_PARAMETERS=HERE\n");
        }

        /// <summary>
        /// Load the parameters from the string.
        /// <remarks>This should get called from load().
        /// The format is binary and not necessarily fit for human consumption.</remarks>
        /// </summary>
        public void PLoad(BinaryReader reader)
        {
            string key, value;
            bool ok = false;
            string s;
            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                //s = reader.ReadLine();   // to check me!
                BinIO.string_read(reader, out s);
                string[] parts = s.Trim().Split('=');
                if (parts.Length != 2)
                    break;
                key = parts[0];
                value = parts[1];
                if (String.Equals(key, "END_OF_PARAMETERS", StringComparison.InvariantCulture))
                {
                    ok = true;
                    break;
                }
                _params[key] = value;
            }
            if (!ok) throw new Exception("PLoad: parameters not properly terminated in save file");
        }



        /// <summary>
        /// a global variable that can be used to override the environment
        /// parameter verbose_params; mainly used for letting us write a command
        /// line program to print the default parameters for components
        /// </summary>
        public virtual void CheckParameters()
        {
            if(_checked) return;
            _checked = true;
            string prefix = this.Name + "_";
            foreach (string key in Global.environ.Keys)
            {
                string entry = key;
                if (entry.StartsWith(prefix))
                {
                    int iwhere = entry.IndexOf("=");
                    if (iwhere >= 0)
                    {
                        entry = entry.Remove(iwhere);
                        if (!_params.ContainsKey(entry.Substring(prefix.Length)))
                        {
                            if (Global.fatal_unknown_params)
                                throw new Exception(String.Format("{0}: unknown environment variable for {1}\r\n"+
                                    "(set fatal_unknown_params=0 to disable this check)",
                                    entry, Name));
                            else
                                Global.Debugf("warn", string.Format("{0}: unknown environment variable for {1}",
                                    entry, Name));
                        }
                    } //endif iwhere
                }
            } //end foreach
        }

        /// <summary>
        /// Set a string property or throw an exception if not implemented.
        /// </summary>
        public virtual void Set(string key, string value)
        {
            PSet(key, value);
        }
        public virtual void Set(string key, double value)
        {
            PSet(key, value);
        }
        public virtual void Set(string key, int value)
        {
            PSet(key, value);
        }

        public virtual string Gets(string key)
        {
            return PGet(key);
        }
        public virtual double Getd(string key)
        {
            return PGetf(key);
        }

        protected static void CHECK_ARG(bool condition, string message)
        {
            if (!condition)
                throw new Exception("CHECK_ARG: " + message);
        }

    }
}
