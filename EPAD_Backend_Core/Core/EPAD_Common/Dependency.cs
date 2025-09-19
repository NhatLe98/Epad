using System;
using System.Linq;
using System.Reflection;

namespace EPAD_Common
{
    public class Dependency
    {
        private Type _iFace = null;
        public Type Interface
        {
            get
            {
                return _iFace ?? ((TypeInfo)Implement).ImplementedInterfaces
                    .FirstOrDefault(e =>
                    e.Namespace.EndsWith(".Interface") &&
                    e.Name.EndsWith(((TypeInfo)Implement).Name));
            }
        }

        public Type Implement { get; private set; }
        public Dependency(Type implement, Type iFace = null)
        {
            Implement = implement;
            _iFace = iFace;
        }
    }
}
