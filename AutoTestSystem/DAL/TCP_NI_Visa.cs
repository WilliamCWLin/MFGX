/*
 * "AutoTestSystem.DAL --> Visa"
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
 *  1. <TCP_NI_VISA.cs> is a Communication for NI_Visa_dll
 *  2. True if connect correctly, false if not connected or execption happens.
 *     <param name="Reource"></param>
 *     <param name="TimeOut"></param>
 * 3. Please install NI VISA driver first "https://www.ni.com/zh-tw/support/downloads/drivers/download/packaged.ni-visa.442805.html"
 */

/*****************************************************************************
*                    Includes Definitions
*****************************************************************************/

using System;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using NationalInstruments.Visa;
using Ivi.Visa;
using static AutoTestSystem.BLL.Bd;


/*****************************************************************************
*                    Function code
*****************************************************************************/
namespace AutoTestSystem.DAL
{
    public class TCP_NI_VISA : Communication
    {

        //////////////////////////////////////////////////////////////////////////////////////
        // 1. Visa communicate Class
        //                                                        
        //////////////////////////////////////////////////////////////////////////////////////
        private MessageBasedSession mbSession;

        //////////////////////////////////////////////////////////////////////////////////////
        // 2. Visa communicate Class
        //                                                        
        //////////////////////////////////////////////////////////////////////////////////////

        public string TCP_ConInfo;
        public int TCP_ConTimeOut;


        //////////////////////////////////////////////////////////////////////////////////////
        // 3. Visa Function
        //                                                        
        //////////////////////////////////////////////////////////////////////////////////////
        public override bool Open()
        {
            bool ret = true;
            using (var rmSession = new ResourceManager())
            {
                try
                {
                    mbSession = (MessageBasedSession)rmSession.Open(TCP_ConInfo);

                    // set timeout (must be set no less than the maximum expected sweep time)
                    mbSession.TimeoutMilliseconds = TCP_ConTimeOut;
                    // = 60000 (note: default is 2 seconds (2000 ms))
                    // enable termination character
                    mbSession.TerminationCharacter = 10; // line feed
                    mbSession.TerminationCharacterEnabled = true;

                    //ret = false;
                }
                catch (Exception ex)
                {
                    ret = false;
                    Logger.Debug($"VISA Open Fail.{ex.Message}");
                }
            }
            return ret;            
        }

        public bool VISA_IsOpen
        {
            get { return true; }
        }

        public override void Close()
        {
            bool ret = false;
            try
            {
                if(mbSession != null)
                {
                    mbSession.Dispose();
                }

                ret = true;
            }
            catch (Exception ex)
            {
                ret = false;
            }
        }

        public override void Dispose()
        {
            throw new System.NotImplementedException();
        }

        public override void Write(string textToWrite)
        {
            bool ret = false;            
            try
            {
                mbSession.RawIO.Write(VISA_ReplaceCommonEscapeSequences(textToWrite));
                Logger.Debug($"Visa Write OK.{textToWrite}");
            }
            catch (Exception ex)
            {
                Logger.Debug($"Visa Write Fail.Error.{ex.Message}");
            }
            return;
        }

        public override string Read()
        {
            string textFromRead= null;       
            try{
                Logger.Debug($"Visa Read OK.");
                textFromRead = VISA_InsertCommonEscapeSequences(mbSession.RawIO.ReadString());
            }
            catch (Exception ex)
            {
                Logger.Debug($"Visa Read Fail.Error.{ex.Message}");

            }
            return textFromRead;   
        }

        public override bool SendCommand(string command, ref string strRecAll, string DataToWaitFor, int timeout = 10)
        {
            throw new NotImplementedException();
        }

        public string Query(string textToWrite)
        {
            IVisaAsyncResult result;
            string textFromRead= null;                 
            try
            {
                // string textFromQuerry = mbSession.RawIO.ReadString();
                Logger.Debug($"Visa Query OK.");

                mbSession.RawIO.Write(VISA_ReplaceCommonEscapeSequences(textToWrite));
                textFromRead = VISA_InsertCommonEscapeSequences(mbSession.RawIO.ReadString());
            }
            catch (Exception ex)
            {
                Logger.Debug($"Visa Query Fail.Can not get device feedback!");
            }
            return textFromRead;
        } 

        //---------------------------------------------------------------------------------------------------

        private string VISA_ReplaceCommonEscapeSequences(string s)
        {
            return s.Replace("\\n", "\n").Replace("\\r", "\r");
        }

        private string VISA_InsertCommonEscapeSequences(string s)
        {
            return s.Replace("\n", "\\n").Replace("\r", "\\r");
        }

    }
}