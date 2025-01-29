using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.NetworkInformation;
using System.Windows;
using AutoICMPMonitoringService;

namespace Auto_ICMP_Monitoring.Data
{
    internal class Device
    {
        public string title;
        public IPAddress ip;
        public DeviceStatus status;
        public int count; //счётчик для повторных перепроверок
        //public DateTime dtimeLastTest; //дата время последней проверки


        public enum DeviceStatus
        {
            Unknown,
            InTheProcess,
            Available,
            RecheckAvailability,
            NotAvailable
        }

        public Device(string title, IPAddress ip, DeviceStatus status, int count)
        {
            this.title = title;
            this.ip = ip;
            this.status = status;
            this.count = count;
        }

        public async void Check()
        {
                DeviceStatus previousStatus = this.status;
                if (previousStatus != DeviceStatus.InTheProcess)
                {
                    this.status = DeviceStatus.InTheProcess;

                    if (DeviceMonitoring.isPinged(this.ip))
                    {

                        switch (previousStatus)
                        {
                            case DeviceStatus.Unknown:
                                this.status = DeviceStatus.Available; //если успех то помечается как доступно
                                this.count = Config.recheckCount; //счётчик сбрасывается 10;
                                break;
                            case DeviceStatus.Available:
                                this.status = DeviceStatus.Available; //если успех то помечается как доступно
                                break;
                            case DeviceStatus.RecheckAvailability:
                                this.status = DeviceStatus.Available; //если успех то помечается как доступно
                                this.count = Config.recheckCount; //счётчик сбрасывается 10;
                                break;
                            case DeviceStatus.NotAvailable:
                                this.status = DeviceStatus.Available; //если успех то помечается как доступно
                                //отправка сообщения, что устройство поднялось
                                this.count = Config.recheckCount; //счётчик сбрасывается 10;
                                DeviceMonitoring.TGSendMessage("✅ " + this.title);
                                AutoICMPMonitoringService.AutoICMPMonitoringService.WriteLog("[" + DateTime.Now.ToString() + "] " + "✅ " + this.title);
                                break;
                            default:
                                break;
                        }

                    }
                    else
                    {

                        switch (previousStatus)
                        {

                            case DeviceStatus.Unknown:
                                this.status = DeviceStatus.RecheckAvailability; //перепроверить
                                this.Check();
                                break;
                            case DeviceStatus.Available:
                                this.status = DeviceStatus.RecheckAvailability; //перепроверить
                                this.Check();
                                break;
                            case DeviceStatus.RecheckAvailability:
                                //если счётчик
                                if (this.count != 0)
                                {
                                    this.status = DeviceStatus.RecheckAvailability; //перепроверить
                                    this.count--; //вычитаем попытку
                                    this.Check();
                                }
                                else
                                {
                                    this.status = DeviceStatus.NotAvailable;
                                    //отправка сообщения, что устройство упало
                                    DeviceMonitoring.TGSendMessage("⚠️ " + this.title);
                                    AutoICMPMonitoringService.AutoICMPMonitoringService.WriteLog("[" + DateTime.Now.ToString() + "] " + "⚠️ " + this.title);
                                }
                                break;
                            case DeviceStatus.NotAvailable:
                                this.status = DeviceStatus.NotAvailable; //упало
                                break;
                            default:
                                break;
                        }
                        
                    }
                    //AutoICMPMonitoringService.AutoICMPMonitoringService.WriteLog("[" + DateTime.Now.ToString() +"] " + this.status + " " + this.title + " попыток: " + this.count);
                }
            }
        }



    }

