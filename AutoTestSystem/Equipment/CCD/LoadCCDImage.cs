using AutoTestSystem.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;
using System.ComponentModel;
using static AutoTestSystem.BLL.Bd;

namespace AutoTestSystem.Equipment.CCD
{
    class LoadCCDImage : CCDBase
    {

        private string _ImagePath;
        [Category("Parameter"), Description("ImagePath"), Editor(typeof(Manufacture.FileSelEditorRelPath), typeof(System.Drawing.Design.UITypeEditor))]
        public string ImagePath
        {
            get { return _ImagePath; }
            set { _ImagePath = value; }
        }

        public override bool Capture(string strSavePath)
        {
            throw new NotImplementedException();
        }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }

        public override bool Init(string strParamInfo)
        {
            return true;
        }

        public override bool Start()
        {
            return true;
        }

        public override bool UnInit()
        {
            return true;
        }

        public override bool Priview(ImageData img)
        {

            img.ImgPath = _ImagePath;
            if(img.ImgPath == null)
                Logger.Warn("Image Path does not exist !!!");



            return true;
        }
    }
}
