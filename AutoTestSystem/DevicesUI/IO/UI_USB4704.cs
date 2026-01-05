using Automation.BDaq;
using AutoTestSystem.Equipment.IO;
using LibUsbDotNet.Main;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using ErrorCode = Automation.BDaq.ErrorCode;

namespace AutoTestSystem.DevicesUI.IO
{
    public partial class UI_USB4704 : Form
    {
        #region fields

        SimpleGraph m_simpleGraph;
        ListViewItem m_firstItem = new ListViewItem();
        ListViewItem m_secondItem = new ListViewItem();

        public const int CHANNEL_COUNT_MAX = 16;
        double[] m_dataScaled = new double[CHANNEL_COUNT_MAX];
        int chanCountSet = 0;
        ValueRange m_valueRange;
        private DoubleBufferListView m_listView;

        #endregion

        public UI_USB4704(ADV_USB4704 _USB4704)
        {
            InitializeComponent();

            if (_USB4704 == null)
                return;
            instantAiCtrl1 = _USB4704.AICtrl;
            instantAiCtrl1.SelectedDevice = _USB4704.AICtrl.SelectedDevice;
        }


        private void InstantAiForm_Load(object sender, EventArgs e)
        {
            // 
            // m_listView
            //
            m_listView = new DoubleBufferListView();
            this.Controls.Add(m_listView);
            this.m_listView.ForeColor = System.Drawing.Color.Black;
            this.m_listView.Location = new System.Drawing.Point(37, 405);
            this.m_listView.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.m_listView.Name = "m_listView";
            this.m_listView.Size = new System.Drawing.Size(578, 48);
            this.m_listView.TabIndex = 16;
            this.m_listView.UseCompatibleStateImageBehavior = false;


            //The default device of project is demo device, users can choose other devices according to their needs. 
            if (!instantAiCtrl1.Initialized)
            {
                MessageBox.Show("No device be selected or device open failed!", "AI_InstantAI");
                this.Close();
                return;
            }

            //set title of the form.
            this.Text = "Instant AI(" + instantAiCtrl1.SelectedDevice.Description + ")";

            button_start.Enabled = true;
            button_stop.Enabled = false;
            button_pause.Enabled = false;

            //initialize a graph with a picture box control to draw Ai data. 
            m_simpleGraph = new SimpleGraph(pictureBox.Size, pictureBox);
            //initialize a timer which drive the data acquisition.
            timer_getData.Interval = trackBar.Value;

            textBox.ReadOnly = true;
            textBox.Text = trackBar.Value.ToString();

            //Add combo Box  to select start channel and channel count
            int chanCount = (instantAiCtrl1.ChannelCount <= CHANNEL_COUNT_MAX) ? instantAiCtrl1.ChannelCount : CHANNEL_COUNT_MAX;

            int count = 0;
            for (int i = 0; i < chanCount; ++i)
            {
                comboBox_chanStart.Items.Add(i.ToString());
                count = i + 1;
                comboBox_chanCount.Items.Add(count.ToString());
            }

            comboBox_chanStart.SelectedIndex = 0;
            comboBox_chanCount.SelectedIndex = 1;

            m_valueRange = instantAiCtrl1.Channels[comboBox_chanStart.SelectedIndex].ValueRange;

            ConfigureGraph();
            InitListView();
        }

        private void ConfigureGraph()
        {
            m_simpleGraph.XCordTimeDiv = 1000;
            string[] X_rangeLabels = new string[2];
            Helpers.GetXCordRangeLabels(X_rangeLabels, 10, 0, TimeUnit.Second);
            label_XCoordinateMax.Text = X_rangeLabels[0];
            label_XCoordinateMin.Text = X_rangeLabels[1];

            MathInterval rangeY;
            ValueUnit unit;

            BDaqApi.AdxGetValueRangeInformation(m_valueRange, 0, null, out rangeY, out unit);
            string[] Y_CordLables = new string[3];
            Helpers.GetYCordRangeLabels(Y_CordLables, rangeY.Max, rangeY.Min, unit);
            label_YCoordinateMax.Text = Y_CordLables[0];
            label_YCoordinateMin.Text = Y_CordLables[1];
            label_YCoordinateMiddle.Text = Y_CordLables[2];

            m_simpleGraph.YCordRangeMax = rangeY.Max;
            m_simpleGraph.YCordRangeMin = rangeY.Min;
            m_simpleGraph.Clear();
        }

        private void timer_getData_Tick(object sender, EventArgs e)
        {
            PerformanceCounter performanceCounter = new PerformanceCounter();
            ErrorCode err;

            performanceCounter.Start();
            err = instantAiCtrl1.Read(comboBox_chanStart.SelectedIndex, chanCountSet, m_dataScaled);
            if (err != ErrorCode.Success)
            {
                HandleError(err);
                timer_getData.Stop();
            }
            m_simpleGraph.Chart(m_dataScaled,
                                         chanCountSet,
                                         1,
            1.0 * trackBar.Value / 1000);
            Refreshm_listView();

            performanceCounter.Stop();
            int interval = (int)(trackBar.Value - performanceCounter.Duration * 1000 - 0.5);
            if (interval > 1)
            {
                timer_getData.Interval = interval;
            }
        }

        private void trackBar_Scroll(object sender, EventArgs e)
        {
            m_simpleGraph.Clear();
            timer_getData.Interval = trackBar.Value;
            textBox.Text = trackBar.Value.ToString();
        }

        private void button_start_Click(object sender, EventArgs e)
        {
            timer_getData.Start();
            button_start.Enabled = false;
            button_pause.Enabled = true;
            button_stop.Enabled = true;
        }

        private void button_pause_Click(object sender, EventArgs e)
        {
            timer_getData.Stop();
            button_start.Enabled = true;
            button_pause.Enabled = false;
        }

        private void button_stop_Click(object sender, EventArgs e)
        {
            timer_getData.Stop();
            button_start.Enabled = true;
            button_stop.Enabled = false;
            button_pause.Enabled = false;
            Array.Clear(m_dataScaled, 0, chanCountSet);
            m_simpleGraph.Clear();
        }

        private void InitListView()
        {
            //control list view ,one grid indicates a channel which specials with color and value.
            m_listView.Clear();
            m_listView.FullRowSelect = false;
            m_listView.Width = 578;
            m_listView.Height = 53;
            m_listView.View = System.Windows.Forms.View.Details;// Set the view to show details.
            m_listView.HeaderStyle = ColumnHeaderStyle.None;
            m_listView.GridLines = true;
            // there are 8 columns for every item.
            for (int i = 0; i < 8; i++)
            {
                m_listView.Columns.Add("", 71);
            }

            // modify the grid's height with image Indirectly.
            ImageList imgList = new ImageList();
            imgList.ImageSize = new Size(1, 21);//width and height.
            m_listView.SmallImageList = imgList; //use imgList to modify the height of m_listView grids.

            // create two m_listViewItem objects,so there are 16 grids for m_listView.
            m_firstItem = new ListViewItem();
            m_firstItem.SubItems.Clear();
            m_firstItem.UseItemStyleForSubItems = false;
            m_firstItem.Font = new Font("SimSun", 10);
            m_firstItem.SubItems[0].Text = String.Format("{0:0.0000}", m_dataScaled[0]);
            m_firstItem.SubItems[0].BackColor = m_simpleGraph.Pens[0].Color;
            for (int i = 1; i < 8; i++)
            {
                if (i < chanCountSet)
                {
                    m_firstItem.SubItems.Add((String.Format("{0:0.0000}", m_dataScaled[i])), Color.Black, Color.Honeydew, new Font("SimSun", 10));
                    m_firstItem.SubItems[i].BackColor = m_simpleGraph.Pens[i].Color;

                }
                else
                {
                    m_firstItem.SubItems.Add("");
                    m_firstItem.SubItems[i].BackColor = Color.White;
                }
            }

            m_secondItem = new ListViewItem();
            m_secondItem.SubItems.Clear();
            m_secondItem.Font = new Font("SimSun", 10);
            m_secondItem.UseItemStyleForSubItems = false;
            if (8 < chanCountSet)
            {
                m_secondItem.SubItems[0].Text = String.Format("{0:0.0000}", m_dataScaled[8]);
                m_secondItem.SubItems[0].BackColor = m_simpleGraph.Pens[8].Color;
            }
            else
            {
                m_secondItem.SubItems[0].Text = "";
                m_secondItem.SubItems[0].BackColor = Color.White;
            }
            for (int i = 9; i < 16; i++)
            {
                if (i < chanCountSet)
                {
                    m_secondItem.SubItems.Add((String.Format("{0:0.0000}", m_dataScaled[i])), Color.Black, Color.Honeydew, new Font("SimSun", 10));
                    m_secondItem.SubItems[i - 8].BackColor = m_simpleGraph.Pens[i].Color;
                }
                else
                {
                    m_secondItem.SubItems.Add("");
                    m_secondItem.SubItems[i - 8].BackColor = Color.White;
                }
            }

            ListViewItem[] list = new ListViewItem[] { m_firstItem, m_secondItem };
            m_listView.Items.AddRange(list);
        }

        private void Refreshm_listView()
        {
            for (int i = 0; i < chanCountSet; i++)
            {
                if (i < 8)
                {
                    m_firstItem.SubItems[i].Text = String.Format("{0:0.0000}", m_dataScaled[i]);
                }
                else
                {
                    m_secondItem.SubItems[i % 8].Text = String.Format("{0:0.0000}", m_dataScaled[i]);
                }
            }
        }

        private void HandleError(ErrorCode err)
        {
            if ((err >= ErrorCode.ErrorHandleNotValid) && (err != ErrorCode.Success))
            {
                //MessageBox.Show("Sorry ! some errors happened, the error code is: " + err.ToString(), "AI_InstantAI");
            }
        }

        private void comboBox_chanCount_SelectedIndexChanged(object sender, EventArgs e)
        {
            chanCountSet = comboBox_chanCount.SelectedIndex + 1;
            InitListView();
        }

        private void UI_USB4704_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (timer_getData != null)
            {
                timer_getData.Stop();
            }
        }
    }

    public static class ConstVal
    {
        public const int Channel_Start = 0;
        public const int Channel_Count = 3;
    }
}
