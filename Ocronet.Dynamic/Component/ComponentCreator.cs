using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Ocronet.Dynamic.Interfaces;

namespace Ocronet.Dynamic.Component
{
    public class ComponentCreator
    {

        /// <summary>
        /// Создает экземпляр обьекта наследованого от IComponent
        /// по типу.
        /// </summary>
        /// <param name="cType">тип класcа</param>
        /// <returns></returns>
        public static IComponent MakeComponent(Type cType)
        {
            Assembly assem = Assembly.GetExecutingAssembly();
            object objComponent = assem.CreateInstance(cType.FullName);
            IComponent result = objComponent as IComponent;
            if (result == null)
                throw new Exception(String.Format("{0} does not inherit IComponent", cType.Name));
            return result;
        }

        /// <summary>
        /// Создает экземпляр обьекта наследованого от IComponent
        /// по псевдониму.
        /// </summary>
        /// <param name="name">псевдоним</param>
        public static IComponent MakeComponent(string name)
        {
            Assembly assem = Assembly.GetExecutingAssembly();
            string className = (Init.ClassAliases.ContainsKey(name) ? Init.ClassAliases[name] : "");
            if (className.Length == 0)
                throw new Exception("Not found component for alias: " + name);
            Type[] types = assem.GetTypes();
            Type cType = null;
            foreach (Type type in types)
            {
                if (type.Name == className)
                {
                    cType = type;
                    break;
                }
            }
            return MakeComponent(cType);
        }


        public static T MakeComponent<T>(string name)
        {
            IComponent comp = MakeComponent(name);
            Type type = typeof(T);
            T result = default(T);

            if (!(comp is T))
                throw new Exception(String.Format("{0} does not {1}", comp.GetType().Name, type.Name));
            result = (T) ((Object)comp);

            return result;
        }
    }
}
