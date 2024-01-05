using System.Collections.Generic;
using System.Linq;
using IqSoft.NGGP.Common;
using IqSoft.NGGP.DAL;
using IqSoft.NGGP.DAL.Filters;
using IqSoft.NGGP.DAL.Interfaces;
using IqSoft.NGGP.DAL.Models;
using IqSoft.NGGP.Common.Helpers;

namespace IqSoft.NGGP.BLL
{
    public class ClientMessageBll : PermissionBll, IClientMessageBll
    {
        #region Constructors

        public ClientMessageBll(BlFactory blFactory = null, BaseBll parentBl = null)
            : base(blFactory, parentBl)
        {

        }

        #endregion

        public ClientMessage SaveClientMessage(ClientMessage message)
        {
            CheckPermissionToSaveObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.CreateClientMessage,
                ObjectTypeId = Constants.ObjectTypes.ClientMessage,
                ObjectId = message.Id
            });
            CheckPermissionToCreatePartnerObjects(message.PartnerId);
            message.CreationTime = GetDbDate();
            message.SessionId = SessionId;
            Db.ClientMessages.Add(message);
            SaveChanges();
            CommonFunctions.Publish(Constants.PublishKeys.ClientMessage, message);
            return message;
        }

        public PagedModel<ClientMessage> GetClientMessages(FilterClientMessage filter)
        {
            var checkPermission = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewClientMessage,
                ObjectTypeId = Constants.ObjectTypes.ClientMessage
            });

            if(filter.ClientId.HasValue)
            {
                var client = Db.Clients.Select(x => new { x.Id, x.PartnerId }).FirstOrDefault(x => x.Id == filter.ClientId.Value);
                if (client != null)
                    filter.PartnerId = client.PartnerId;
            }
            var checkPartner = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = Constants.ObjectTypes.Partner
            });

            filter.CheckPermissionResuts = new List<CheckPermissionOutput<ClientMessage>>
            {
                new CheckPermissionOutput<ClientMessage>
                {
                    AccessibleObjects = checkPermission.AccessibleObjects,
                    HaveAccessForAllObjects = checkPermission.HaveAccessForAllObjects,
                    Filter = x => checkPermission.AccessibleObjects.Contains(x.ObjectId)
                },
                new CheckPermissionOutput<ClientMessage>
                {
                    AccessibleObjects = checkPartner.AccessibleObjects,
                    HaveAccessForAllObjects = checkPartner.HaveAccessForAllObjects,
                    Filter = x => checkPartner.AccessibleObjects.Contains(x.PartnerId)
                }
            };
            var clientMessages = new PagedModel<ClientMessage>
            {
                Entities = filter.FilterObjects(Db.ClientMessages, messages => messages.OrderByDescending(y => y.Id)),
                Count = filter.SelectedObjectsCount(Db.ClientMessages)
            };
            return clientMessages;
        }

        public ClientMessage ReadClientMessage(long messageId, int clientId)
        {
            CheckPermission(Constants.Permissions.ViewClientMessage);
            var clientMessage = Db.ClientMessages.FirstOrDefault(x => x.Id == messageId);
            if (clientMessage == null)
                return null;
            var state = Db.ClientMessageStates.FirstOrDefault(x => x.MessageId == messageId && x.ClientId == clientId);
            if (state != null)
                return null;
            var client = Db.Clients.Select(x => new { x.Id, x.PartnerId }).FirstOrDefault(x => x.Id == clientId);
            if (client == null)
                return null;
            if (clientMessage.ClientId != clientId && clientMessage.PartnerId != client.PartnerId)
                return null;

            var currentDate = GetDbDate();
            state = new ClientMessageState
                {
                    MessageId = messageId,
                    State = (int)Constants.ClientMessageStates.Readed,
                    ClientId = clientId,
                    LastUpdateTime = currentDate
                };
            Db.ClientMessageStates.Add(state);
            SaveChanges();
            clientMessage.ClientMessageStates = new List<ClientMessageState> { state };
            return clientMessage;
        }

        public ClientMessage SendMessagesToPartnerClients(int partnerId, string text, string subject)
        {
            CheckPermission(Constants.Permissions.CreateClientMessage);
            CheckPermissionToCreatePartnerObjects(partnerId);
            var message = new ClientMessage
            {
                Message = text,
                PartnerId = partnerId,
                CreationTime = GetDbDate(),
                Subject = subject,
                SessionId = SessionId,
                Type = (int)Constants.ClientMessageTypes.FromUser
            };
            Db.ClientMessages.Add(message);
            SaveChanges();
            return message;
        }

        public List<ClientMessage> SendClientMessages(FilterClient filter, string text, string subject, long? parentId = null)
        {
            CheckPermission(Constants.Permissions.CreateClientMessage);
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.CreatePartnerObjects,
                ObjectTypeId = Constants.ObjectTypes.Partner
            });
            var createionTime = GetDbDate();
            var clients =
                filter.FilterObjects(Db.Clients)
                    .Where(
                        x =>
                            partnerAccess.HaveAccessForAllObjects ||
                            partnerAccess.AccessibleObjects.Contains(x.PartnerId))
                    .ToList();
            var messages = new List<ClientMessage>();
            foreach (var client in clients)
            {
                var message = new ClientMessage
                {
                    ClientId = client.Id,
                    Message = text,
                    PartnerId = client.PartnerId,
                    CreationTime = createionTime,
                    Subject = subject,
                    SessionId = SessionId,
                    Type = (int)Constants.ClientMessageTypes.FromUser,
                    ParentId = parentId
                };
                Db.ClientMessages.Add(message);
                messages.Add(message);
            }
            SaveChanges();
            return messages;
        }
    }
}
