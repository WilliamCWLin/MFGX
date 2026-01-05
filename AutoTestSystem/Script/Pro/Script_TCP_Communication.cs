using AutoTestSystem.Base;
using AutoTestSystem.DAL;
using AutoTestSystem.Equipment.ControlDevice;
using AutoTestSystem.Equipment.Motion;
using AutoTestSystem.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Design;
using System.Windows.Forms.Design;
using static AutoTestSystem.Base.MotionBase;
using static AutoTestSystem.BLL.Bd;
using static AutoTestSystem.Equipment.Motion.Motion_DIOparam;
using AutoTestSystem.DynamicProperty;
using AutoTestSystem.Interface.Config;
using AutoTestSystem.Interface.Capabilities;

namespace AutoTestSystem.Script
{
    internal class Script_TCP_Communication : Script_1MotBase, INotifyPropertyChanged
    {
        private DynamicPropertyManager _propertyManager;
        private TcpConfig _config;

        string jsonStr = string.Empty;
        Dictionary<string, string> Output_Data = new Dictionary<string, string>();

        public enum FILE_FORMAT
        {
            JSON,
            CSV,
            Binary
        }

        public enum CMD_ACTION
        {
            Read,
            Write,
            ReadFromRegister,
            WriteToRegister,
            ReadRegisterToFile,
            WriteRegisterFromFile,
        }


        private CMD_ACTION _action;
        

        [Category("Command"), Description("TCP Communication Command")]
        public CMD_ACTION Action 
        {
            get => _action;
            set
            {
                if (_action != value)
                {
                    _action = value;
                    OnPropertyChanged(nameof(Action));
                    this.RefreshDynamicProperties();
                }
            }
        }

        [Category("Command"), Description("Register type that want to read or write")]
        public string Register { get; set; }

        [Category("Command"), Description("Register number that want to read or write")]
        public int Number { get; set; }

        private string _registerData = "0";

        [Category("Command"), Description("Register data that want to write")]
        public string RegisterData
        {
            get => _registerData;
            set
            {
                //if (Convert.ToDouble(value) < 0)
                //    _registerData = 0;
                //else if (value > 999999)
                //    _registerData = 999999;
                //else
                    _registerData = value;
            }
        }

        private uint _dataLengthh = 1;
        [Category("Command"), Description("Register data read/write length")]
        public uint DataLength
        {
            get => _dataLengthh;
            set
            {
                if (value < 1)
                    _dataLengthh = 1;
                else if (value > 2)
                    _dataLengthh = 2;
                else
                    _dataLengthh = value;
            }
        }

        [Category("Command"), Description("How many register need")]
        public int Count { get; set; }

        [Category("Command"), Description("Toggle write")]
        public bool Toggle { get; set; }

        [Category("Command"), Description("Read/Write file path"), Editor(typeof(FolderSelEditorRelPath), typeof(System.Drawing.Design.UITypeEditor))]
        public string FilePath { get; set; }

        [Category("Command"), Description("Read/Write file format")]
        public FILE_FORMAT FileFormat { get; set; }

        [Category("Command"), Description("false: Integer Value / true: Double Value")]
        public bool Other_Format { get; set; } = false;

        [Browsable(false)]
        public bool ShouldShowRegAndNum => Action == CMD_ACTION.ReadFromRegister || Action == CMD_ACTION.WriteToRegister || Action == CMD_ACTION.ReadRegisterToFile;

        [Browsable(false)]
        public bool ShouldShowRegData => Action == CMD_ACTION.WriteToRegister;

        [Browsable(false)]
        public bool ShouldShowToggle => Action == CMD_ACTION.WriteToRegister;

        [Browsable(false)]
        public bool ShouldShowRegCount => Action == CMD_ACTION.ReadRegisterToFile;

        [Browsable(false)]
        public bool ShouldShowFileProperty => Action == CMD_ACTION.ReadRegisterToFile || Action == CMD_ACTION.WriteRegisterFromFile;

        [Category("Motion TimeOut"), Description("Motion TimeOut")]
        public int TimeOut { get; set; } = 10000;

        private void ConfigurePropertyVisibility()
        {

            var rules = new PropertyVisibilityBuilder()
                .When(nameof(Register), () => ShouldShowRegAndNum)
                .When(nameof(Number), () => ShouldShowRegAndNum)
                .When(nameof(RegisterData), () => ShouldShowRegData)
                .When(nameof(Count), () => ShouldShowRegCount)
                .When(nameof(FilePath), () => ShouldShowFileProperty)
                .When(nameof(FileFormat), () => ShouldShowFileProperty)
                .When(nameof(Toggle), () => ShouldShowToggle)
                .Build();

            _propertyManager.AddVisibilityRules(rules);
            _propertyManager.Initialize();

        }

        // INotifyPropertyChanged 實作
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public Script_TCP_Communication()
        {
            _propertyManager = this.GetDynamicPropertyManager();
            ConfigurePropertyVisibility();
        }

        public override bool PreProcess()
        {
            Output_Data.Clear();
            return true;
        }

        public override bool Process(MotionBase MotionDev, ref string output)
        {
            try
            {
                if (ShouldShowToggle)
                {
                    _config = new TcpConfig(toggle: Toggle);
                    var tcpCapable = MotionDev as ITcpEquipmentOps;
                    if (tcpCapable != null)
                    {
                        tcpCapable.ExecuteTcp(_config);
                    }
                }

                ///add param for ouput spec
                
                double out_v = 0;

                switch (Action)
                {
                    case CMD_ACTION.ReadFromRegister:
                        {
                            bool ret = false;
                            ret = MotionDev.readFromRegister(Register, Number, _dataLengthh, Other_Format, ref out_v);
                            if (ret)
                            {
                                Output_Data.Add(Number.ToString(), out_v.ToString());
                                output = JsonConvert.SerializeObject(Output_Data, Formatting.Indented);
                                jsonStr = output;
                            }
                            return ret;
                        }
                    case CMD_ACTION.WriteToRegister:
                        {
                            string registerData_str = string.Empty;
                            registerData_str = ReplaceProp(_registerData);
                            return MotionDev.writeToRegister(Register, Number, registerData_str, _dataLengthh, Other_Format); 
                        }
                    case CMD_ACTION.ReadRegisterToFile:
                        var _fileName = $"{Register}{Number}_{Count}.{FileFormat.ToString().ToLower()}";
                        var _filePath = Path.Combine(FilePath, _fileName);
                        return MotionDev.readRegisterToFile(Register, Number, Count, _filePath, (int)FileFormat, _dataLengthh);
                    case CMD_ACTION.WriteRegisterFromFile:
                        return MotionDev.writeRegisterFromFile(FilePath, _dataLengthh);
                    case CMD_ACTION.Read:
                        Logger.Error("Not Implement");
                        return false;
                    case CMD_ACTION.Write:
                        Logger.Error("Not Implement");
                        return false;
                    default:
                        Logger.Error("Unknown Action");
                        return false;
                }
            }
            catch (Exception e)
            {
                Logger.Error($"Error : {e}");
                return false;
            }

        }

        public override bool PostProcess()
        {
             if (!string.IsNullOrEmpty(Spec))
            {
                string ret = string.Empty;
                ret = CheckRule(jsonStr, Spec);
                LogMessage($"CheckRule: {ret}", MessageLevel.Info);
                if (ret == "PASS")
                    return true;
                else
                    return false;
            }
            return true;
        }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }

        public override bool PreProcess(string strParamInput)
        {
            throw new NotImplementedException();
        }
        public override bool Process(MotionBase MotionDevice)
        {
            throw new NotImplementedException();
        }
        public override bool PostProcess(string TestKeyword, string strCheckSpec, ref string strDataout)
        {
            throw new NotImplementedException();
        }

    }
}
