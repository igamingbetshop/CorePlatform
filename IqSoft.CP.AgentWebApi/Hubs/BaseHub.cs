using System;
using System.ServiceModel;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.DAL.Models.Cache;
using Newtonsoft.Json;
using IqSoft.CP.BLL.Interfaces;
using System.Linq;
using System.Collections.Concurrent;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.AgentWebApi.Models;
using IqSoft.CP.AgentWebApi.Filters.Messages;
using IqSoft.CP.AgentWebApi.Models.ClientModels;
using IqSoft.CP.AgentWebApi.Helpers;
using IqSoft.CP.BLL.Caching;

namespace IqSoft.CP.AgentWebApi.Hubs
{
	[HubName("basehub")]
	public class BaseHub : Hub
	{
		public static ConcurrentDictionary<string, SessionIdentity> ConnectedUsers = new ConcurrentDictionary<string, SessionIdentity>();
		public static readonly dynamic _connectedClients = GlobalHost.ConnectionManager.GetHubContext<BaseHub>().Clients;
		public ApiResponseBase GetTickets(ApiFilterTicket filter)
		{
			try
			{
				using (var userBl = new UserBll(new SessionIdentity(), WebApiApplication.DbLogger))
				{
					var identity = CheckToken(filter.LanguageId, filter.TimeZone, filter.Token, userBl);
					var agentUser = CacheManager.GetUserById(identity.Id);
					var isAgentEmploye = agentUser.Type == (int)UserTypes.AgentEmployee;
					if (isAgentEmploye)
						agentUser = CacheManager.GetUserById(agentUser.ParentId.Value);

					using (var clientBl = new ClientBll(identity, WebApiApplication.DbLogger))
					{
						filter.PartnerId = identity.PartnerId;
						filter.UserId = agentUser.Id;
						var resp = clientBl.GetTickets(filter.MapToFilterTicket(), false);
						return new ApiResponseBase
						{
							ResponseObject = new
							{
								Tickets = resp.Entities.MapToTickets(identity.TimeZone, filter.LanguageId),
								Count = resp.Count
							}
						};
					}
				}
			}
			catch (FaultException<BllFnErrorType> ex)
			{
				ApiResponseBase response;

				if (ex.Detail != null)
					response = new ApiResponseBase
					{
						ResponseCode = ex.Detail.Id,
						Description = ex.Detail.Message
					};
				else
					response = new ApiResponseBase
					{
						ResponseCode = Constants.Errors.GeneralException,
						Description = ex.Message
					};
				WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(response));
				return response;
			}
			catch (Exception e)
			{
				WebApiApplication.DbLogger.Error(e);
				return new ApiResponseBase
				{
					ResponseCode = Constants.Errors.GeneralException,
					Description = e.Message
				};
			}
		}

		public ApiResponseBase CreateTicket(ApiOpenTicketInput ticketInput)
		{
			try
			{
				using (var userBl = new UserBll(new SessionIdentity(), WebApiApplication.DbLogger))
				{
					var identity = CheckToken(ticketInput.LanguageId, ticketInput.TimeZone, ticketInput.Token, userBl);

					using (var clientBl = new ClientBll(identity, WebApiApplication.DbLogger))
					{
						if (string.IsNullOrWhiteSpace(ticketInput.Message) || string.IsNullOrWhiteSpace(ticketInput.Subject))
							throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.WrongInputParameters);
						var ticket = new Ticket
						{
							PartnerId = identity.PartnerId,
							Type = (int)TicketTypes.Discussion,
							Subject = ticketInput.Subject,
							Status = (int)MessageTicketState.Active,
							LastMessageUserId = identity.Id
						};
						var message = new TicketMessage
						{
							Message = ticketInput.Message,
							Type = (int)ClientMessageTypes.MessageFromUser,
							UserId = identity.Id
						};

						var resp = clientBl.OpenTickets(ticket, message, ticketInput.ClientIds, null, false);
						foreach (var c in ticketInput.ClientIds)
						{
							Helpers.Helpers.InvokeMessage("UpdateCacheItem", string.Format("{0}_{1}", Constants.CacheItems.ClientUnreadTicketsCount, c),
								new BllUnreadTicketsCount { Count = 1 }, TimeSpan.FromHours(6));
						}
						return new ApiResponseBase { ResponseObject = resp.MapToTickets(identity.TimeZone, identity.LanguageId) };
					}
				}
			}
			catch (FaultException<BllFnErrorType> ex)
			{
				ApiResponseBase response;

				if (ex.Detail != null)
					response = new ApiResponseBase
					{
						ResponseCode = ex.Detail.Id,
						Description = ex.Detail.Message
					};
				else
					response = new ApiResponseBase
					{
						ResponseCode = Constants.Errors.GeneralException,
						Description = ex.Message
					};
				WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(response));
				return response;
			}
			catch (Exception e)
			{
				WebApiApplication.DbLogger.Error(e);
				return new ApiResponseBase
				{
					ResponseCode = Constants.Errors.GeneralException,
					Description = e.Message
				};
			}
		}

		public ApiResponseBase CreateMessage(ApiCreateMessageInput createMessageInput)
		{
			try
			{
				using (var userBl = new UserBll(new SessionIdentity(), WebApiApplication.DbLogger))
				{
					var identity = CheckToken(createMessageInput.LanguageId, createMessageInput.TimeZone, createMessageInput.Token, userBl);
					using (var clientBl = new ClientBll(identity, WebApiApplication.DbLogger))
					{
						if (string.IsNullOrWhiteSpace(createMessageInput.Message))
							throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.WrongInputParameters);
						var message = new TicketMessage
						{
							Message = createMessageInput.Message,
							Type = (int)ClientMessageTypes.MessageFromUser,
							TicketId = createMessageInput.TicketId,
							CreationTime = clientBl.GetServerDate(),
							UserId = identity.Id
						};

						var resp = clientBl.AddMessageToTicket(message, out int clientId, out int unreadMessageCount);

						Helpers.Helpers.InvokeMessage("UpdateCacheItem", string.Format("{0}_{1}", Constants.CacheItems.ClientUnreadTicketsCount, clientId),
																										new BllUnreadTicketsCount { Count = unreadMessageCount }, TimeSpan.FromHours(6));
						return new ApiResponseBase
						{
							ResponseObject = resp.ToApiTicketMessage(createMessageInput.TimeZone)
						};
					}
				}
			}
			catch (FaultException<BllFnErrorType> ex)
			{
				ApiResponseBase response;

				if (ex.Detail != null)
					response = new ApiResponseBase
					{
						ResponseCode = ex.Detail.Id,
						Description = ex.Detail.Message
					};
				else
					response = new ApiResponseBase
					{
						ResponseCode = Constants.Errors.GeneralException,
						Description = ex.Message
					};
				WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(response));
				return response;
			}
			catch (Exception e)
			{
				WebApiApplication.DbLogger.Error(e);
				return new ApiResponseBase
				{
					ResponseCode = Constants.Errors.GeneralException,
					Description = e.Message
				};
			}
		}

		public ApiResponseBase GetTicketMessages(ApiTicketInput ticketInput)
		{
			try
			{
				using (var userBl = new UserBll(new SessionIdentity(), WebApiApplication.DbLogger))
				{
					CheckToken(ticketInput.LanguageId, ticketInput.TimeZone, ticketInput.Token, userBl);

					using (var clientBl = new ClientBll(userBl))
					{
						var resp = clientBl.GetMessagesByTicket(ticketInput.TicketId, false);
						return new ApiResponseBase
						{
							ResponseObject = resp.Select(x => x.MapToTicketMessageModel(ticketInput.TimeZone)).OrderBy(x => x.CreationTime).ToList()
						};
					}
				}
			}
			catch (FaultException<BllFnErrorType> ex)
			{
				ApiResponseBase response;
				if (ex.Detail != null)
					response = new ApiResponseBase
					{
						ResponseCode = ex.Detail.Id,
						Description = ex.Detail.Message
					};
				else
					response = new ApiResponseBase
					{
						ResponseCode = Constants.Errors.GeneralException,
						Description = ex.Message
					};
				WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(response));
				return response;
			}
			catch (Exception e)
			{
				WebApiApplication.DbLogger.Error(e);
				return new ApiResponseBase
				{
					ResponseCode = Constants.Errors.GeneralException,
					Description = e.Message
				};
			}
		}

		public ApiResponseBase CloseTicket(ApiTicketInput ticketInput)
		{
			try
			{
				using (var userBl = new UserBll(new SessionIdentity(), WebApiApplication.DbLogger))
				{
					CheckToken(ticketInput.LanguageId, ticketInput.TimeZone, ticketInput.Token, userBl);
					using (var clientBl = new ClientBll(userBl))
					{
						clientBl.ChangeTicketStatus(ticketInput.TicketId, null, MessageTicketState.Closed);
						return new ApiResponseBase();
					}
				}
			}
			catch (FaultException<BllFnErrorType> ex)
			{
				ApiResponseBase response;
				if (ex.Detail != null)
					response = new ApiResponseBase
					{
						ResponseCode = ex.Detail.Id,
						Description = ex.Detail.Message
					};
				else
					response = new ApiResponseBase
					{
						ResponseCode = Constants.Errors.GeneralException,
						Description = ex.Message
					};
				WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(response));
				return response;
			}
			catch (Exception e)
			{
				WebApiApplication.DbLogger.Error(e);
				return new ApiResponseBase
				{
					ResponseCode = Constants.Errors.GeneralException,
					Description = e.Message
				};
			}
		}

		private SessionIdentity CheckToken(string languageId, double timeZone, string token, IUserBll userBl)
		{
			var session = userBl.GetUserSession(token);
			var userIdentity = new SessionIdentity
			{
				LanguageId = languageId,
				LoginIp = session.Ip,
				SessionId = session.Id,
				Token = session.Token,
				Id = session.UserId.Value,
				TimeZone = timeZone,
				IsAdminUser = true
			};
			if (session.UserId != null)
			{
				var user = userBl.GetUserById(session.UserId.Value);
				userIdentity.PartnerId = user.PartnerId;
				userIdentity.CurrencyId = user.CurrencyId;
			}
			
			return userIdentity;
		}
	}
}