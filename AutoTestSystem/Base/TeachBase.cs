using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTestSystem.Base
{
    public abstract class TeachBase : Manufacture.Equipment
    {
        public abstract void Dispose();

        public virtual bool Init(string strParamInfo)
        {
            return true;
        }

        public virtual T GetParametersFromJson<T>() where T : class
        {
            return JsonConvert.DeserializeObject<T>(GetJsonParamString());
        }

        protected abstract string GetJsonParamString();
    }
}
