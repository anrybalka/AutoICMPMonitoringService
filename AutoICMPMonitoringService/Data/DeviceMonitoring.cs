using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using System.Threading;
using System.Windows;
using System.Net;
using System.Net.NetworkInformation;



namespace Auto_ICMP_Monitoring.Data
{
    internal class DeviceMonitoring
    {
        public static bool isPinged(IPAddress deviceAddress)
        {
            PingReply reply = null;
            Ping pinger = null;
            bool pingSuccess = false;

            try
            {
                pinger = new Ping();
                reply = pinger.Send(deviceAddress);
                if (reply.Status == IPStatus.Success)
                {
                    pingSuccess = true;
                }
            }
            catch (PingException)
            {
                //MessageBox.Show(pEx.Message);
            }
            finally
            {
                if (pinger != null)
                {
                    pinger.Dispose();
                }
            }
            return pingSuccess;
        }

        public static void TGSendMessage(string msg)
        {
            TelegramBotClient bot = new TelegramBotClient(Config.tokenBot);
            bot.SendTextMessageAsync(Config.idGroup, msg);

        }

    }

}
