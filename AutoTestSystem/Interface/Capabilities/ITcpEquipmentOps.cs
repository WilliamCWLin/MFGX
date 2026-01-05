using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoTestSystem.Interface.Config;

namespace AutoTestSystem.Interface.Capabilities
{
    // 能力介面：只有支援 TCP Script 的設備需要實作
    public interface ITcpEquipmentOps
    {
        void ExecuteTcp(TcpConfig config);
    }
}
