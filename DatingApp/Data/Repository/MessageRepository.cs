﻿using AutoMapper;
using AutoMapper.QueryableExtensions;
using DatingApp.Dtos;
using DatingApp.Entities;
using DatingApp.Helpers;
using DatingApp.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.Data.Repository
{
    public class MessageRepository : IMessageRepository
    {
        //Dependency Injection
        private readonly DatingDbContext _context;
        private readonly IMapper _mapper;
        public MessageRepository(DatingDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper= mapper;
        }

       

        public void AddMessage(Message message)
        {
           _context.Messages.Add(message);
        }

        public void DeleteMessage(Message message)
        {
            _context.Messages.Remove(message);
        }
        public void AddGroup(Group group)
        {
            _context.Groups.Add(group);
        }
        public async Task<Group?> GetMessageGroup(string groupName)
        {
            return await _context.Groups.Include(x => x.Connections)
                .FirstOrDefaultAsync(x => x.Name == groupName);
            
        }
        public async Task<Connection?> GetConnection(string connectionId)
        {
           return await _context.Connections.FindAsync(connectionId);
        }
        public void RemoveConnection(Connection connection)
        {
           _context.Connections.Remove(connection);
        }
        public async  Task<Message?> GetMessage(int id)
        {
            return await _context.Messages.FindAsync(id);
            
        }

       

        public async Task<PagedList<MessageDto>> GetMessagesForUser( MessageParams messageParams)
        {
            var query=_context.Messages.OrderByDescending(m => m.MessageSent).AsQueryable();
            query = messageParams.Container switch
            {
                "Inbox" => query.Where(u => u.Recipient.UserName == messageParams.Username
                && u.RecipientDeleted == false),
                "Outbox" => query.Where(u => u.Sender.UserName == messageParams.Username
                && u.SenderDeleted == false),
                _ => query.Where(u => u.Recipient.UserName == messageParams.Username
                && u.RecipientDeleted == false && u.DateRead == null)
            };

            var messages = query.ProjectTo<MessageDto>(_mapper.ConfigurationProvider);
            return await PagedList<MessageDto>.CreateAsync(messages, 
                messageParams.PageNumber, messageParams.PageSize);
        }

        public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentUsername, string recipientUsername)
        {
            var messages = await _context.Messages
                .Where(m => m.Recipient.UserName == currentUsername && m.RecipientDeleted == false
                && m.Sender.UserName == recipientUsername
                || m.Recipient.UserName == recipientUsername
                && m.Sender.UserName == currentUsername && m.SenderDeleted == false)
                .OrderBy(m => m.MessageSent)
                .ProjectTo<MessageDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

           var unReadMessages = messages.Where(m => m.DateRead == null
                && m.RecipientUsername == currentUsername).ToList();
            if(unReadMessages.Count != 0)
            {
                foreach (var message in unReadMessages)
                {
                    message.DateRead = DateTime.UtcNow;
                }
                await _context.SaveChangesAsync();
            }
            return messages;
        }

      

     
        public async Task<Group?> GetGroupForConnection(string connectionId)
        {
            return await _context.Groups
                .Include(c => c.Connections)
                .Where(c => c.Connections.Any(x => x.ConnectionId == connectionId))
                .FirstOrDefaultAsync();
           
        }
    }
}
