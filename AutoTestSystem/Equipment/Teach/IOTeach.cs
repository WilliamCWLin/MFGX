using AutoTestSystem.Base;
using AutoTestSystem.DUT;
using AutoTestSystem.DevicesUI.Teach;
using AutoTestSystem.DevicesUI.IO;
using AutoTestSystem.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Linq;
using System.Windows.Forms;

namespace AutoTestSystem.Equipment.Teach
{
    public class IOTeach : TeachBase, IDisposable
    {
        [Category("Select IO Devices")]
        [Description("編輯路徑清單")]
        [Editor(typeof(SelectIODevices), typeof(UITypeEditor))]
        public List<string> SelectedDevices { get; set; } = new List<string>();

        private List<IOBase> ioCards = new List<IOBase>();
        private Dictionary<string, IOEntry> getIOMap = new Dictionary<string, IOEntry>();
        private Dictionary<string, IOEntry> getDOMap = new Dictionary<string, IOEntry>();
        private Dictionary<string, IOEntry> setIOMap = new Dictionary<string, IOEntry>();

        private Dictionary<string, IOEntry> getAIOMap = new Dictionary<string, IOEntry>();
        private Dictionary<string, IOEntry> setAIOMap = new Dictionary<string, IOEntry>();
        public List<string> GetGetIOKeys()
        {
            return new List<string>(getIOMap.Keys);
        }
        public List<string> GetAIKeys()
        {
            return new List<string>(getAIOMap.Keys);
        }
        public List<string> GetDOKeys()
        {
            return new List<string>(getDOMap.Keys);
        }

        public List<string> GetSetIOKeys()
        {
            return new List<string>(setIOMap.Keys);
        }

        public override bool Init(string jsonParam)
        {
            ioCards.Clear();
            getIOMap.Clear();
            setIOMap.Clear();
            getDOMap.Clear();

            getAIOMap.Clear();
            setAIOMap.Clear();
            foreach (string deviceKey in SelectedDevices)
            {
                if (GlobalNew.Devices.TryGetValue(deviceKey, out var device) && device is IOBase ioCard)
                {
                    ioCards.Add(ioCard);
                    RegisterIO(ioCard);
                }
                else
                {
                    LogMessage($"Device '{deviceKey}' not found or is not IOBase.");
                }
            }




            return true;
        }

        public override bool UnInit()
        {
            ioCards.Clear();
            getIOMap.Clear();
            setIOMap.Clear();
            getDOMap.Clear();

                        getAIOMap.Clear();
            setAIOMap.Clear();
            return true;
        }

        public override bool Show()
        {
            if (!EnsureDevicesSelected()) return false;

            using (var form = new IOViewerForm(this))
            {
                form.ShowDialog();
            }

            return true;
        }

        private bool EnsureDevicesSelected()
        {
            if (SelectedDevices.Any()) return true;

            using (var dlg = new DeviceSelectionForm(SelectedDevices))
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    SelectedDevices = dlg.GetSelectedKeys();
                }
            }

            if (!SelectedDevices.Any())
            {
                MessageBox.Show("請至少選擇一個控制軸或IO點再進行教學", "提醒", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        public override void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public bool GetIO(string sensorName, ref bool status)
        {
            if (getIOMap.TryGetValue(sensorName, out IOEntry entry))
            {
                return entry.Card.GETIO(entry.PortNum, entry.Channel, ref status);
            }
            throw new KeyNotFoundException($"Sensor '{sensorName}' not found in GetIO map.");
        }
        public bool GetDO(string sensorName, ref bool status)
        {
            if (getDOMap.TryGetValue(sensorName, out IOEntry entry))
            {
                return entry.Card.GETDO(0, entry.Channel, ref status);
            }
            throw new KeyNotFoundException($"Sensor '{sensorName}' not found in GetIO map.");
        }
        public bool SetIO(string sensorName, bool output)
        {
            if (setIOMap.TryGetValue(sensorName, out IOEntry entry))
            {
                return entry.Card.SETIO(entry.Channel, output);
            }
            throw new KeyNotFoundException($"Sensor '{sensorName}' not found in SetIO map.");
        }

        public bool GetAI(string sensorName, ref string status)
        {
            if (getAIOMap.TryGetValue(sensorName, out IOEntry entry))
            {
                return entry.Card.InstantAI(entry.Channel, ref status);
            }
            throw new KeyNotFoundException($"Sensor '{sensorName}' not found in GetAI map.");
        }
        public Dictionary<string, bool> GetAllInputStatusFromCards()
        {
            var result = new Dictionary<string, bool>();

            // 對每張卡片執行一次 GETIOs
            foreach (var card in ioCards)
            {
                // 只選這張卡的所有點位
                var cardPoints = getIOMap.Where(kvp => kvp.Value.Card == card).ToList();
                if (cardPoints.Count == 0)
                    continue; // 沒有任何點位就跳過

                int maxChannel = cardPoints.Max(kvp => kvp.Value.Channel);

                var statusArray = new bool[maxChannel + 1]; // 假設通道是從 0 開始連續編號

                try
                {
                    card.GETALLIO(ref statusArray);
                }
                catch (Exception ex)
                {
                    LogMessage($"卡片 {card.Description} 批次讀取失敗: {ex.Message}");
                    continue;
                }

                // 將該卡片的狀態填入 result
                foreach (var kvp in getIOMap)
                {
                    var entry = kvp.Value;
                    if (entry.Card == card && entry.Channel < statusArray.Length)
                    {
                        result[kvp.Key] = statusArray[entry.Channel];
                    }
                }
            }

            return result;
        }

        public Dictionary<string, string> GetAllAIInputVoltageFromCards()
        {
            var result = new Dictionary<string, string>();

            foreach (var card in ioCards)
            {
                // 只選這張卡的所有AI點位
                var cardPoints = getAIOMap.Where(kvp => kvp.Value.Card == card).ToList();
                if (cardPoints.Count == 0)
                    continue;

                string dataOut = string.Empty;
                try
                {
                    card.GetAll_AI(ref dataOut);
                    //LogMessage(dataOut);
                }
                catch (Exception ex)
                {
                    LogMessage($"卡片 {card.Description} 批次讀取AI失敗: {ex.Message}");
                    foreach (var kvp in cardPoints)
                    {
                        result[kvp.Key] = "讀取失敗";
                    }
                    continue;
                }

                Dictionary<string, string> voltagesDict = null;
                try
                {
                    voltagesDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(dataOut);
                }
                catch (Exception ex)
                {
                    LogMessage($"卡片 {card.Description} 解析AI JSON失敗: {ex.Message}");
                    foreach (var kvp in cardPoints)
                    {
                        result[kvp.Key] = "解析失敗";
                    }
                    continue;
                }

                foreach (var kvp in cardPoints)
                {
                    var entry = kvp.Value;
                    string channelKey = entry.Channel.ToString();
                    if (voltagesDict != null && voltagesDict.ContainsKey(channelKey))
                    {
                        result[kvp.Key] = voltagesDict[channelKey];
                    }
                    else
                    {
                        result[kvp.Key] = "無資料";
                    }
                }
            }

            return result;
        }
        public Dictionary<string, bool> GetAllOutputStatusFromCards()
        {
            var result = new Dictionary<string, bool>();

            // 對每張卡片執行一次 GETIOs
            foreach (var card in ioCards)
            {
                // 只選這張卡的所有點位
                var cardPoints = getDOMap.Where(kvp => kvp.Value.Card == card).ToList();
                if (cardPoints.Count == 0)
                    continue; // 沒有任何點位就跳過

                int maxChannel = cardPoints.Max(kvp => kvp.Value.Channel);

                var statusArray = new bool[maxChannel + 1]; // 假設通道是從 0 開始連續編號

                try
                {
                    card.GETALLDO(ref statusArray);
                }
                catch (Exception ex)
                {
                    LogMessage($"卡片 {card.Description} 批次讀取失敗: {ex.Message}");
                    continue;
                }

                // 將該卡片的狀態填入 result
                foreach (var kvp in getDOMap)
                {
                    var entry = kvp.Value;
                    if (entry.Card == card && entry.Channel < statusArray.Length)
                    {
                        result[kvp.Key] = statusArray[entry.Channel];
                    }
                }
            }

            return result;
        }

        public IEnumerable<string> GetOutputDOSensorNames() => getDOMap.Keys;
        public IEnumerable<string> GetInputSensorNames() => getIOMap.Keys;
        public IEnumerable<string> GetOutputSensorNames() => setIOMap.Keys;

        private void RegisterIO(IOBase card)
        {
            // 處理 GetIO_List
            var getListJson = card.GetIO_List;
            var getItems = !string.IsNullOrWhiteSpace(getListJson)
                ? JsonConvert.DeserializeObject<List<IOItem>>(getListJson)
                : new List<IOItem>();

            foreach (var item in getItems)
            {
                if (int.TryParse(item.Channel, out int channel))
                {
                    if (!getIOMap.ContainsKey(item.SensorName))
                        getIOMap[item.SensorName] = new IOEntry { Card = card, Channel = channel };
                    else
                        MessageBox.Show($"Not Found GetIO SensorName: {item.SensorName}");
                }
            }

            // 處理 SetIO_List
            var setListJson = card.SetIO_List;
            var setItems = !string.IsNullOrWhiteSpace(setListJson)
                ? JsonConvert.DeserializeObject<List<IOItem>>(setListJson)
                : new List<IOItem>();

            foreach (var item in setItems)
            {
                if (int.TryParse(item.Channel, out int channel))
                {
                    if (!setIOMap.ContainsKey(item.SensorName))
                    {
                        setIOMap[item.SensorName] = new IOEntry { Card = card, Channel = channel };
                        getDOMap[item.SensorName] = new IOEntry { Card = card, Channel = channel };
                    }
                    else
                        MessageBox.Show($"Not found SetIO SensorName: {item.SensorName}");
                }
            }


            // 處理 GetAIO_List
            var getAIOJson = card.GetAIO_List;
            var getAIOItems = !string.IsNullOrWhiteSpace(getAIOJson)
                ? JsonConvert.DeserializeObject<List<IOItem>>(getAIOJson)
                : new List<IOItem>();

            foreach (var item in getAIOItems)
            {
                if (int.TryParse(item.Channel, out int channel))
                {
                    if (!getAIOMap.ContainsKey(item.SensorName))
                        getAIOMap[item.SensorName] = new IOEntry { Card = card, Channel = channel };
                    else
                        MessageBox.Show($"Duplicate GetAIO SensorName: {item.SensorName}");
                }
            }

            // 處理 SetAIO_List
            var setAIOJson = card.SetAIO_List;
            var setAIOItems = !string.IsNullOrWhiteSpace(setAIOJson)
                ? JsonConvert.DeserializeObject<List<IOItem>>(setAIOJson)
                : new List<IOItem>();

            foreach (var item in setAIOItems)
            {
                if (int.TryParse(item.Channel, out int channel))
                {
                    if (!setAIOMap.ContainsKey(item.SensorName))
                        setAIOMap[item.SensorName] = new IOEntry { Card = card, Channel = channel };
                    else
                        MessageBox.Show($"Duplicate SetAIO SensorName: {item.SensorName}");
                }
            }
        }
        public string DescribeSensorLocation(string sensorName)
        {
            IOEntry entry;

            if (getIOMap.TryGetValue(sensorName, out entry))
                return $"[{entry.Card.Description}({entry.Channel})]";

            return $"Sensor not found: {sensorName}";
        }


        protected override string GetJsonParamString()
        {
            throw new NotImplementedException();
        }

        public IOEntry GetEntry(string sensorName, bool isGet)
        {
            if (isGet && getIOMap.TryGetValue(sensorName, out IOEntry getEntry))
                return getEntry;
            if (!isGet && setIOMap.TryGetValue(sensorName, out IOEntry setEntry))
                return setEntry;
            return null;
        }

    }

    public class IOItem
    {
        public string SensorName { get; set; }
        public string Channel { get; set; }
    }

    public class IOEntry
    {
        public IOBase Card { get; set; }
        public int Channel { get; set; }
        public int PortNum { get; set; } = 0;
    }

    public class SelectIODevices : System.Drawing.Design.UITypeEditor
    {
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            var IOteach = context.Instance as IOTeach;

            using (var dlg = new DeviceSelectionForm(IOteach.SelectedDevices))
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    return dlg.GetSelectedKeys();
                }
            }

            return IOteach.SelectedDevices; // 如果用戶取消選擇，返回原始值
        }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }
    }
}
