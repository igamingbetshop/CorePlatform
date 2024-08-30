using System;
using System.ServiceModel;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.AdminWebApi.Helpers;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.AdminWebApi.Models.CommonModels;
using IqSoft.CP.AdminWebApi.Filters.Messages;
using IqSoft.CP.AdminWebApi.Models.ClientModels;
using Newtonsoft.Json;
using IqSoft.CP.BLL.Interfaces;
using System.Linq;
using System.Collections.Concurrent;
using IqSoft.CP.Common.Models.CacheModels;

namespace IqSoft.CP.AdminWebApi.Hubs
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
					if (string.IsNullOrEmpty(filter.LanguageId))
						filter.LanguageId = Constants.DefaultLanguageId;
					var identity = CheckToken(filter.LanguageId, filter.TimeZone, filter.Token, userBl);

					using (var clientBl = new ClientBll(identity, WebApiApplication.DbLogger))
					{
						var resp = clientBl.GetTickets(filter.MapToFilterTicket(), true);
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
					if (string.IsNullOrEmpty(ticketInput.LanguageId))
						ticketInput.LanguageId = Constants.DefaultLanguageId;
					var identity = CheckToken(ticketInput.LanguageId, ticketInput.TimeZone, ticketInput.Token, userBl);

					using (var clientBl = new ClientBll(identity, WebApiApplication.DbLogger))
					{
						if (string.IsNullOrWhiteSpace(ticketInput.Message) || string.IsNullOrWhiteSpace(ticketInput.Subject))
							throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.WrongInputParameters);
						var ticket = new Ticket
						{
							PartnerId = ticketInput.PartnerId,
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

						var resp = clientBl.OpenTickets(ticket, message, ticketInput.ClientIds, ticketInput.SegmentIds, true);
						if (ticketInput.ClientIds != null)
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
					if (string.IsNullOrEmpty(createMessageInput.LanguageId))
						createMessageInput.LanguageId = Constants.DefaultLanguageId;
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
					if (string.IsNullOrEmpty(ticketInput.LanguageId))
						ticketInput.LanguageId = Constants.DefaultLanguageId;
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
					if (string.IsNullOrEmpty(ticketInput.LanguageId))
						ticketInput.LanguageId = Constants.DefaultLanguageId;
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

		public static void PaymentRequst(int paymentRequestId)
		{
			using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
			{
				var result = paymentSystemBl.GetfnPaymentRequestById(paymentRequestId);
				foreach (var user in ConnectedUsers)
				{
					using (var partnerBll = new PartnerBll(user.Value, WebApiApplication.DbLogger))
					{
						var partnerAccess = partnerBll.GetPermissionsToObject(new CheckPermissionInput
						{
							Permission = Constants.Permissions.ViewPartner,
							ObjectTypeId = (int)ObjectTypes.Partner
						});
						var clientAccess = partnerBll.GetPermissionsToObject(new CheckPermissionInput
						{
							Permission = Constants.Permissions.ViewClient,
							ObjectTypeId = (int)ObjectTypes.Client
						});
						if ((!partnerAccess.HaveAccessForAllObjects && partnerAccess.AccessibleIntegerObjects.All(x => x != result.PartnerId) ||
							(!clientAccess.HaveAccessForAllObjects && clientAccess.AccessibleObjects.All(x => x != result.ClientId))))
							continue;

						_connectedClients.Client(user.Key).OnPaymentRequest(result.MapToApiPaymentRequest(user.Value.TimeZone));
					}
				}
			}
		}

		public ApiResponseBase SubscribeToPaymentRequests(ApiRequestBase input)
		{
			try
			{
				using (var userBl = new UserBll(new SessionIdentity(), WebApiApplication.DbLogger))
				{
					if (string.IsNullOrEmpty(input.LanguageId))
						input.LanguageId = Constants.DefaultLanguageId;
					var identity = CheckToken(input.LanguageId, input.TimeZone, input.Token, userBl);
					using (var partnerBll = new PartnerBll(identity, WebApiApplication.DbLogger))
					{
						partnerBll.CheckPermission(Constants.Permissions.ViewPaymentRequests);
						var context = GlobalHost.ConnectionManager.GetHubContext<BaseHub>();
						context.Groups.Add(Context.ConnectionId, "PaymentHub");
						ConnectedUsers.TryRemove(Context.ConnectionId, out SessionIdentity session);
						ConnectedUsers.TryAdd(Context.ConnectionId, identity);
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

		public bool UnSubscribeFromPaymentRequests()
		{
			try
			{
				var context = GlobalHost.ConnectionManager.GetHubContext<BaseHub>();
				context.Groups.Remove(Context.ConnectionId, "PaymentHub");
				ConnectedUsers.TryRemove(Context.ConnectionId, out SessionIdentity session);
				return true;
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
			}
			catch (Exception e)
			{
				WebApiApplication.DbLogger.Error(e);
			}
			return false;

		}

		private SessionIdentity CheckToken(string languageId, double timeZone, string token, IUserBll userBl)
		{
			var session = userBl.GetUserSession(token);
			var user = userBl.GetUserById(session.UserId.Value);
			var userIdentity = new SessionIdentity
			{
				LanguageId = languageId,
				LoginIp = session.Ip,
				PartnerId = user.PartnerId,
				SessionId = session.Id,
				Token = session.Token,
				Id = session.UserId.Value,
				TimeZone = timeZone,
				CurrencyId = user.CurrencyId,
				IsAdminUser = true
			};
			return userIdentity;
		}
	}
}