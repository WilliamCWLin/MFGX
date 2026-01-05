/*
 * "Not use"
 *
 * Corpright William & Zhibin
 *
 *.##.......####.########.########..#######..##....##
 *.##........##.....##....##.......##.....##.###...##
 *.##........##.....##....##.......##.....##.####..##
 *.##........##.....##....######...##.....##.##.##.##
 *.##........##.....##....##.......##.....##.##..####
 *.##........##.....##....##.......##.....##.##...###
 *.########.####....##....########..#######..##....##
 *
 *  1. <ISavelog.cs> is out of service   
 * 
 */


namespace AutoTestSystem
{
    /// <summary>
    /// 定义打印log的接口
    /// </summary>
    internal interface ISavelog
    {
        //object Lock { get; set; }          //!互斥锁
        // object PrintToObject { get; set; }          //!打印到哪里，txt文件or richbox控件
        /// <summary>
        /// 打印日志
        /// </summary>
        /// <param name="log">日志内容</param>
        /// <param name="type">打印设置:是否换行打印、打印字体</param>
        void SaveLog(string log, int type = 1);
    }
}