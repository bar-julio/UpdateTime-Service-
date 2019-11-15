using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace UpdateTime_Service_
{
    public partial class Service1 : ServiceBase
    {
        Timer timer;
        public Service1()
        {
            InitializeComponent();
        }
        public void Debug()
        {
            AdjustTime();
        }
        /// <summary>
        /// Start function
        /// </summary>
        /// <param name="args"></param>
        protected override void OnStart(string[] args)
        {
            AdjustTime();


            timer = new Timer();
            timer.Elapsed += new ElapsedEventHandler(OnElapsedTime);
            timer.Interval = 240000; //number in milisecinds  
            timer.Enabled = true;
        }

        private void OnElapsedTime(object sender, ElapsedEventArgs e)
        {
            AdjustTime();
        }

        protected override void OnStop()
        {
            timer.Dispose();
        }

        public void AdjustTime()
        {

            ProcessStartInfo startInfo = new ProcessStartInfo("CMD");
#if RELEASE
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
#endif
            startInfo.FileName = "cmd.exe";
            startInfo.Verb = "runas";
#if DEBUG
            System.Diagnostics.Debug.WriteLine(Status(),"**Debug**");
#endif
            if (Status().CompareTo("Stopped") == 0)
            {
                SendCom(ref startInfo, "/C net start w32time");
            }
            if (Status().CompareTo("Running") == 0)
            {
                //Code to config and resync time
                SendCom(ref startInfo, "/C w32tm /config /manualpeerlist:time.windows.com,0x9 /syncfromflags:manual /reliable:yes /update");

                SendCom(ref startInfo, "/C net stop w32time && net start w32time");

                SendCom(ref startInfo, "/C w32tm /resync /force");
            }
        }
        private void SendCom(ref ProcessStartInfo args, string command)
        {
            args.Arguments = command;
            Process p = new Process
            {
                StartInfo = args,
                
            };
            
            p.Start();
            p.WaitForExit(15000);
            p.Dispose();
        }
        /// <summary>
        /// Returns status of service W32Time
        /// </summary>
        /// <returns>Status</returns>
        private string Status()
        {

            ServiceController sc = new ServiceController("W32Time");

            switch (sc.Status)
            {
                case ServiceControllerStatus.Running:
                    return "Running";
                case ServiceControllerStatus.Stopped:
                    return "Stopped";
                case ServiceControllerStatus.Paused:
                    return "Paused";
                case ServiceControllerStatus.StopPending:
                    return "Stopping";
                case ServiceControllerStatus.StartPending:
                    return "Starting";
                default:
                    return "Status Changing";
            }
        }
    }
}
