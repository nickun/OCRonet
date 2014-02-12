using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Ocronet.Dynamic.Utils
{
    public class Global
    {
        /// <summary>
        /// a global variable that can be used to override the environment
        /// parameter verbose_params; mainly used for letting us write a command
        /// line program to print the default parameters for components
        /// </summary>
        public static string global_verbose_params;

        /// <summary>
        /// unknown parameters for a class are a fatal error
        /// </summary>
        public static bool fatal_unknown_params = true;

        /// <summary>
        /// global environment parameters
        /// </summary>
        public static Dictionary<string, string> environ = new Dictionary<string, string>();

        static Global()
        {
            environ.Add("debug", "info, error");
        }

        public static string GetEnv(string varname)
        {
            string result = "";
            if (environ.ContainsKey(varname))
                result = environ[varname];
            return result;
        }

        public static void SetEnv(string varname, string value)
        {
            environ[varname] = value;
        }

        /// <summary>
        /// Проверка является ли which отладочным (выводить отладочную информацию)
        /// </summary>
        /// <param name="which">может быть: info, warn, error, iodetail, detail</param>
        /// <returns></returns>
        public static bool IsDebug(string which)
        {
            string env = GetEnv("debug");
            if(env.Length == 0)
                env = "info";
            return env.Contains(which);
        }

        /// <summary>
        /// Вывод диагностического сообщения
        /// </summary>
        /// <param name="which">может быть: info, warn, error, iodetail, training-detail</param>
        /// <param name="fmt">строка форматирования вида: {0}</param>
        /// <param name="arg">аргументы</param>
        public static void Debugf(string which, string fmt, params object[] arg)
        {
            if(!IsDebug(which)) return;
            string message = string.Format("[" + which + "] " + fmt, arg);
            //Debug.WriteLine(message);
            Console.WriteLine(message);
        }
    }
}
