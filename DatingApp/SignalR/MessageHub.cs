using AutoMapper;
using DatingApp.Data.Repository;
using DatingApp.Dtos;
using DatingApp.Entities;
using DatingApp.Extensions;
using DatingApp.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace DatingApp.SignalR
{
    public class MessageHub:Hub
    {
       private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHubContext<PresenceHub> _presenceHub;
        public MessageHub(IUnitOfWork unitOfWork,
            IMapper mapper, IHubContext<PresenceHub> presenceHub)
        {
             _unitOfWork = unitOfWork;
            _mapper = mapper;
            _presenceHub = presenceHub;
        }

        public override async Task OnConnectedAsync()
        {
           
            var httpContext = Context.GetHttpContext();
            var otherUser = httpContext?.Request.Query["user"];
            if (Context.User == null || string.IsNullOrEmpty(otherUser)) 
                throw new Exception("Error connecting to hub");
            var groupName =GetGroupName(Context.User.GetUsername(),otherUser!);
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
           var group = await AddToGroup(groupName);
            await Clients.Group(groupName).SendAsync("UpdatedGroup", group);
            var messages = await _unitOfWork.MessageRepository.GetMessageThread( Context.User.GetUsername(),otherUser!);
            if (_unitOfWork.HasChanges()) await _unitOfWork.Complete();
            await Clients.Caller.SendAsync("ReceiveMessageThread", messages);

        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
          var group =await RemoveFromMessageGroup();
            await Clients.Group(group.Name).SendAsync("UpdatedGroup", group);
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(CreateMessageDto createMessageDto)
        {
            var username = Context.User?.GetUsername() ?? throw new Exception("User not found");

            if (username == createMessageDto.RecipientUsername.ToLower())
            {
               throw new HubException("You cannot send messages to yourself");
            }
            var sender = await _unitOfWork.UserRepository.GetUserByUsernameAsync(username);
            var recipient = await _unitOfWork.UserRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);
            if (recipient == null || sender == null || sender.UserName == null || recipient.UserName == null)
                throw new HubException(" cannot send message at this moment");
            var message = new Message
            {
                Sender = sender,
                Recipient = recipient,
                SenderUsername = sender.UserName,
                RecipientUsername = recipient.UserName,
                Content = createMessageDto.Content
            };
            var groupName = GetGroupName(sender.UserName, recipient.UserName);
            var group = await _unitOfWork.MessageRepository.GetMessageGroup(groupName);
            if (group != null && group.Connections.Any(x => x.Username == recipient.UserName))
            {
                message.DateRead = DateTime.UtcNow;

            }
            else
            {
                var connections = await PresenceTracker.GetConnectionsForUser(recipient.UserName);
                if (connections != null && connections?.Count != null)
                {
                    await _presenceHub.Clients.Clients(connections).SendAsync("NewMessageReceived",
                        new { username = sender.UserName, knownAs = sender.KnownAs });
                }
            }
            _unitOfWork.MessageRepository.AddMessage(message);
            if (await _unitOfWork.Complete())
            {
              //  var group= GetGroupName(sender.UserName,recipient.UserName);
                
                await Clients.Group(groupName).SendAsync("NewMessage", _mapper.Map<MessageDto>(message));
            }
           
        }
        private async Task<Group> AddToGroup(string groupName)
        {
            var username = Context.User?.GetUsername() ?? throw new Exception("User not found");
            var group = await _unitOfWork.MessageRepository.GetMessageGroup(groupName);
            var connection = new Connection
            {
                ConnectionId = Context.ConnectionId,
                Username = username
            };
            if (group == null)
            {
                group = new Group { Name = groupName };
                _unitOfWork.MessageRepository.AddGroup(group);
            }
            group.Connections.Add(connection);
            if (await _unitOfWork.Complete()) return group;
            throw new HubException("Failed to join group");
        }
        private async Task<Group> RemoveFromMessageGroup()
        {
            var group = await _unitOfWork.MessageRepository.GetGroupForConnection(Context.ConnectionId);

            var connection = group?.Connections.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            if (connection != null && group != null)
            {
                _unitOfWork.MessageRepository.RemoveConnection(connection);
                if (await _unitOfWork.Complete()) return group;
            }
            throw new HubException("Failed to remove from group");
        }

        public async Task Typing(string recipientUsername)
        {
            var senderUsername = Context.User?.GetUsername();

            if (senderUsername != null)
            {
                var groupName = GetGroupName(senderUsername, recipientUsername);
                await Clients.Group(groupName).SendAsync("UserTyping", senderUsername);
            }
        }
        private string GetGroupName(string caller, string other)
        {
            var stringCompare = string.CompareOrdinal(caller, other) < 0;
            return stringCompare ? $"MessagesThread-{caller}-{other}" : $"MessagesThread-{other}-{caller}";
        } 
            
    }
}
