using System;
using System.IO;
using System.Reflection;
using System.Timers;
using log4net.Config;
using Topshelf;

namespace TopshelfLab
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var configFile = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "log4net.config");

            // 載入 log4net 設定檔
            XmlConfigurator.ConfigureAndWatch(new FileInfo(configFile));

            var rc = HostFactory.Run(
                hc =>
                    {
                        hc.SetServiceName("Stuff Service");
                        hc.SetDisplayName("Stuff");
                        hc.SetDescription("Sample Topshelf Host");

                        hc.EnablePauseAndContinue();
                        hc.EnableShutdown();

                        hc.Service<TownCrier>(
                            sc =>
                                {
                                    sc.ConstructUsing(hs => new TownCrier());

                                    sc.WhenStarted(s => s.Start());
                                    sc.WhenStopped(s => s.Stop());
                                    sc.WhenPaused(s => s.Pause());
                                    sc.WhenContinued(s => s.Continue());
                                    sc.WhenShutdown(s => s.Shutdown());
                                });

                        hc.RunAsLocalSystem();

                        hc.EnableServiceRecovery(
                            sr =>
                                {
                                    sr.RestartService(TimeSpan.FromSeconds(30));
                                    sr.RunProgram(1, "notepad.exe");
                                    sr.RestartComputer(TimeSpan.FromSeconds(45), "Computer is restarting.");

                                    sr.SetResetPeriod(1);
                                    sr.OnCrashOnly();
                                });

                        hc.DependsOnEventLog();

                        hc.AfterInstall(ihs => { Console.WriteLine($"{ihs.ServiceName} installed."); });
                    });

            Environment.ExitCode = (int)Convert.ChangeType(rc, rc.GetTypeCode());
        }

        public class TownCrier
        {
            private readonly Timer timer;

            public TownCrier()
            {
                this.timer = new Timer(1000) { AutoReset = true };
                this.timer.Elapsed += (sender, eventArgs) => Console.WriteLine("It is {0} and all is well", DateTime.Now);
            }

            public void Start()
            {
                this.timer.Start();
            }

            public void Stop()
            {
                this.timer.Stop();
            }

            public void Pause()
            {
                File.AppendAllText(@"D:\test.txt", "paused\r\n");
            }

            public void Continue()
            {
                File.AppendAllText(@"D:\test.txt", "continued\r\n");
            }

            public void Shutdown()
            {
                File.AppendAllText(@"D:\test.txt", "shutdown\r\n");
            }
        }
    }
}