using AutoTestSystem.Model;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using log4net.Layout;
using System;
using System.Windows.Forms;

namespace AutoTestSystem.BLL
{
    public class Log4NetHelper
    {
        #region Field

        private static ILog _Ilogger;
        private static FileAppender _fileAppender;
        private static RollingFileAppender _RollingFileAppender;
        private static RichTextBoxAppender _RichTextBoxAppender;
        private static string layout = "%date{yyyy-MM-dd hh:mm:ss}-[%level]-[%method] - %message " + Environment.NewLine;
        private static string logFilePath = "Log4Net_testError.log";

        #endregion Field

        public Log4NetHelper(string _logFilePath)
        {
            logFilePath = _logFilePath;
        }

        #region Property

        public static string Layout { set { layout = value; } }

        #endregion Property

        private static FileAppender GetFileAppender()
        {
            var fileAppender = new FileAppender()
            {
                Name = "FileAppender",
                Layout = GetPatternLayout(),
                Threshold = Level.Error,
                AppendToFile = true,
                File = logFilePath,
            };
            fileAppender.ActivateOptions();
            return fileAppender;

        }

        private static RollingFileAppender GetRollingFileAppender()
        {
            var rollingAppender = new RollingFileAppender()
            {
                Name = "Rolling File Appender",
                Layout = GetPatternLayout(),
                Threshold = Level.All,
                AppendToFile = true,
                File = "RollingLog.log",
                MaximumFileSize = "1MB",
                MaxSizeRollBackups = short.Parse(Global.MaxSizeRollBackups),
            };
            rollingAppender.ActivateOptions();
            return rollingAppender;
        }

        private static RichTextBoxAppender GetRichTextBoxAppender()
        {
            layout = "%date{yyyy-MM-dd hh:mm:ss} [%level] - %message";
            var richTextBoxAppender = new RichTextBoxAppender
            {
                Name = "RichTextBox Appender",
                Layout = GetPatternLayout(),
                Threshold = Level.All,
                FormName = "MainForm",
                RichTextBoxName = "richTextBox1",
            };
            var consoleAppender = new ConsoleAppender { Layout = new SimpleLayout() };
            IAppender[] list = { richTextBoxAppender, consoleAppender };
            BasicConfigurator.Configure(list);
            richTextBoxAppender.ActivateOptions();
            return richTextBoxAppender;
        }

        private static PatternLayout GetPatternLayout()
        {
            var patterLayout = new PatternLayout()
            {
                ConversionPattern = layout
            };
            patterLayout.ActivateOptions();
            return patterLayout;
        }

        public static ILog GetLogger(Type type)
        {
            if (_fileAppender == null)
            {
                _fileAppender = GetFileAppender();
            }

            if (_RollingFileAppender == null)
            {
                _RollingFileAppender = GetRollingFileAppender();
            }

            if (_RichTextBoxAppender == null)
            {
                _RichTextBoxAppender = GetRichTextBoxAppender();
            }

            if (_Ilogger != null)
            {
                return _Ilogger;
            }

            BasicConfigurator.Configure(_RollingFileAppender, _RichTextBoxAppender);
            _Ilogger = LogManager.GetLogger(type);
            return _Ilogger;
        }
    }

    public class RichTextBoxAppender : AppenderSkeleton
    {
        private RichTextBox _textBox;
        public string FormName { get; set; }
        public string RichTextBoxName { get; set; }

        protected override void Append(LoggingEvent loggingEvent)
        {
            if (_textBox == null)
            {
                if (string.IsNullOrEmpty(FormName) || string.IsNullOrEmpty(RichTextBoxName))
                    return;

                Form form = Application.OpenForms[FormName];
                if (form == null)
                    return;

                Control[] controls = form.Controls.Find(RichTextBoxName, true);
                if (controls.Length == 0) return;
                _textBox = controls[0] as RichTextBox;

                form.FormClosing += (s, e) => _textBox = null;
            }

            System.Drawing.Color textColor;
            switch (loggingEvent.Level.DisplayName.ToUpper())
            {
                case "FATAL"://test Exception
                    textColor = System.Drawing.Color.DarkRed;
                    break;

                case "ERROR": //test fail
                    textColor = System.Drawing.Color.Red;
                    break;

                case "INFO": //test pass
                    textColor = System.Drawing.Color.Green;
                    break;

                case "WARN":
                    textColor = System.Drawing.Color.DarkOrange;
                    break;

                case "DEBUG"://command
                    textColor = System.Drawing.Color.Black;
                    break;

                default:
                    textColor = System.Drawing.Color.Black;
                    break;
            }

            _textBox.BeginInvoke((MethodInvoker)delegate
            {
                _textBox.SelectionColor = textColor;
                //_textBox.AppendText(loggingEvent.RenderedMessage + Environment.NewLine);
                _textBox.AppendText(RenderLoggingEvent(loggingEvent)/* + Environment.NewLine*/);
                _textBox.ScrollToCaret();
            });
        }
    }

    public class TextBoxAppender : AppenderSkeleton
    {
        public RichTextBox RichTextBox { get; set; }

        protected override void Append(LoggingEvent loggingEvent)
        {
            Action operation = () => { this.RichTextBox.AppendText(RenderLoggingEvent(loggingEvent)); };
            this.RichTextBox.Invoke(operation);
        }
    }

    //var appender = LogManager.GetRepository().GetAppenders().Where(a => a.Name == "TextBoxAppender").FirstOrDefault();
    //if (appender != null)
    //((TextBoxAppender) appender).RichTextBox = this.richTextBoxLog;

    //The configuration

    //<log4net debug = "false" >
    //< appender name= "TextBoxAppender" type= "SecurityAudit.UI.TextBoxAppender" >
    //< layout type= "log4net.Layout.PatternLayout" >
    //< conversionPattern value= "%date [%thread] %-5level %logger [%property{NDC}] - %message%newline" />
    //</ layout >
    //</ appender >
    //< root >
    //< priority value= "DEBUG" />
    //< appender -ref ref= "TextBoxAppender" />
    //</ root >
    //</ log4net >
}