using System;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using IqSoft.NGGP.Common;
using IqSoft.NGGP.DAL;
using IqSoft.NGGP.DAL.Interfaces;
using IqSoft.NGGP.DAL.Models;
using Newtonsoft.Json;

namespace IqSoft.NGGP.WindowsServices.JobService
{
    public static class JobManager
    {
        public static void DetectRunableJobs(Object sender)
        {
            using (var jobBl = Program.BlFactory.CreateJobBll())
            {
                var currentTime = jobBl.GetDbDate();
                var jobs = jobBl.GetJobs().Where(x => x.NextExecutionTime <= currentTime && x.State == (int)Constants.JobStates.Active).ToList();
                foreach (var job in jobs)
                {
                    var newThread = new Thread(() => CallJob(job));
                    newThread.Start();
                }
            }
        }

        public static void CallJob(Job job)
        {
            using (var jobBl = Program.BlFactory.CreateJobBll())
            {
                var jobStartTime = DateTime.Now;
                string jobMessage = "Job completed successfully";
                string parameters = null;
                try
                {
                    parameters = CallJobFunction(job, jobBl);
                }
                catch (FaultException<fnErrorType> ex)
                {
                    jobMessage = JsonConvert.SerializeObject(ex);
                    parameters = job.Parameters;
                }
                catch (Exception ex)
                {
                    jobMessage = JsonConvert.SerializeObject(ex);
                    parameters = job.Parameters;
                    jobBl.WriteErrorLog(ex);
                }
                var jobResult = new JobResult
                {
                    JobId = job.Id,
                    Duration = (int)((DateTime.Now - jobStartTime).TotalSeconds),
                    Message = jobMessage
                };
                jobBl.SaveJobResult(jobResult);
                job.NextExecutionTime = job.NextExecutionTime.AddSeconds(job.PeriodInSeconds);
                job.Parameters = parameters;
                jobBl.SaveJob(job);
            }
        }

        public static string CallJobFunction(Job job, IJobBll jobBl)
        {
            switch (job.Id)
            {
                case Constants.Jobs.ClosePeriod:
                    {
                        var closePeriodInput = JsonConvert.DeserializeObject<ClosePeriodInput>(job.Parameters);
                        var result = jobBl.ClosePeriod(closePeriodInput);
                        if(result)
                            closePeriodInput.EndTime = closePeriodInput.EndTime.AddHours(Constants.ClosePeriodPeriodicy);
                        return JsonConvert.SerializeObject(closePeriodInput);
                    }
                case Constants.Jobs.AddMoneyToPartnerAccount:
                    {
                        var addMoneyToPartnerAccountInput = JsonConvert.DeserializeObject<AddMoneyToPartnerAccountInput>(job.Parameters);
                        var result = jobBl.AddMoneyToPartnerAccount(addMoneyToPartnerAccountInput);
                        if (result)
                            addMoneyToPartnerAccountInput.EndTime = addMoneyToPartnerAccountInput.EndTime.AddHours(Constants.AddMoneyToPartnerAccountPeriodicy);
                        return JsonConvert.SerializeObject(addMoneyToPartnerAccountInput);
                    }
                case Constants.Jobs.SendUnsendedPaymentRequests:
                    jobBl.SendUnsendedPaymentRequests();
                    break;
                case Constants.Jobs.CheckNotPayedPaymentRequestStatesInPaymentSystem:
                    jobBl.CheckNotPayedPaymentRequestStatesInPaymentSystem();
                    break;
                case Constants.Jobs.ExpireUserSessions:
                    jobBl.ExpireUserSessions();
                    break;
                case Constants.Jobs.ResetBetShopLimits:
                    jobBl.ResetBetShopLimits();
                    break;
                case Constants.Jobs.ResetBetShopDailyTicketNumber:
                    {
                        var resetBetShopDailyTicketNumberInput = JsonConvert.DeserializeObject<ResetBetShopDailyTicketNumberInput>(job.Parameters);
                        var result = jobBl.ResetBetShopDailyTicketNumber(resetBetShopDailyTicketNumberInput);
                        foreach (var dailyTicketNumberResetSetting in resetBetShopDailyTicketNumberInput.Settings)
                        {
                            if (result.Results.FirstOrDefault(x => x.PartnerId == dailyTicketNumberResetSetting.PartnerId && x.ResetResult) != null)
                                dailyTicketNumberResetSetting.ResetTime =
                                    dailyTicketNumberResetSetting.ResetTime.AddHours(
                                        Constants.BetShopDailyTicketNumberResetPeriodicy);
                        }
                        return JsonConvert.SerializeObject(resetBetShopDailyTicketNumberInput);
                    }
                case Constants.Jobs.SetInvalidUnpaidBets:
                    jobBl.SetInvalidUnpaidWins();
                    break;
            }
            return null;
        }
    }
}
