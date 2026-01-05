/*
 * "AutoTestSystem.Base --> Visa"
 *
 * Corpright Jordan
 *
 *.##.......####.########.########..#######..##....##
 *.##........##.....##....##.......##.....##.###...##
 *.##........##.....##....##.......##.....##.####..##
 *.##........##.....##....######...##.....##.##.##.##
 *.##........##.....##....##.......##.....##.##..####
 *.##........##.....##....##.......##.....##.##...###
 *.########.####....##....########..#######..##....##
 *
 *  1. <VisaBase.cs> is Base type for TCP IT Visa communication
 *  2. Visa is a protocol that follow SCPI format
 *  3.EntryPoint: <TestNew.cs> entry point --> <VisaBase.cs>
 *   
 */

/*****************************************************************************
*                    Includes Definitions
*****************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*****************************************************************************
*                    Function code
*****************************************************************************/
namespace AutoTestSystem.Base
{
    public abstract class VisaBase : Manufacture.Equipment,IDisposable
    {
        public abstract void Dispose();
        public abstract bool Init(string strParamInfo);
        public abstract bool Get_Device_Info(string strParamInfo);

    }

}
