﻿using AnimeAB.Reponsitories.DTO;
using AnimeAB.Reponsitories.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using AnimeAB.Core.ChatHubs;
using AnimeAB.Reponsitories.Interface;
using AnimeAB.Reponsitories.Entities;
using AutoMapper;

namespace AnimeAB.Core.Apis
{
    [Route("api")]
    [ApiController]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    public class MessageController : ControllerBase
    {
        private readonly IHubContext<CommentHub> _hubContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public MessageController(
            [NotNull] IHubContext<CommentHub> hubContext, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _hubContext = hubContext;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [Route("comments")]
        [HttpGet]
        public async Task<IActionResult> GetComments(
            [FromQuery]string id, [FromQuery]string sort = "", 
            [FromQuery]string user_reply = "")
        {
            try
            {
                IEnumerable<Comment> comments = await _unitOfWork.CommentPlugin.GetAsync(id);
                if (sort == "lastest") comments = comments.OrderByDescending(x => x.When).ToList();
                if (sort == "oldest") comments = comments.OrderBy(x => x.When).ToList();
                if (!string.IsNullOrWhiteSpace(user_reply)) comments = comments.Where(x => x.ReplyComment.Equals(user_reply)).ToList();

                return Ok(comments);
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Route("comments")]
        [HttpPost]
        public IActionResult PostComment(
            [FromBody]CommentDto commentDto, [FromQuery]string id, [FromQuery]string receiver = "", [FromQuery]string link_notify = "")
        {
            try
            {
                Comment comment = _mapper.Map<Comment>(commentDto);
                Comment commentAdded = _unitOfWork.CommentPlugin.CreateAsync(comment, id);

                Task.Run(() => _hubContext.Clients.All.SendAsync(id, commentAdded));
                if(!string.IsNullOrWhiteSpace(receiver) && !string.IsNullOrWhiteSpace(link_notify) && !commentDto.user_local.Equals(receiver))
                {
                    var notification = new Notification
                    {
                        UserRevice = receiver,
                        Message = commentDto.name + " đã nhắc bạn trong bình luận của họ.",
                        LinkNotify = link_notify
                    };

                    Notification notifyAdded = _unitOfWork.CommentPlugin.AddNotifiAsync(notification);
                    Task.Run(() => _hubContext.Clients.All.SendAsync(notification.UserRevice, notifyAdded));
                }
                return NoContent();
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Route("notification")]
        [HttpGet]
        public async Task<IActionResult> GetNotifications([FromQuery]string user, [FromQuery] string notify = "", [FromQuery]bool count = false)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(user)) return BadRequest("User is not valid!");
                
                if(!string.IsNullOrWhiteSpace(notify))
                {
                    await _unitOfWork.CommentPlugin.ReadNotifiAsync(user, notify);
                }

                var notifies = await _unitOfWork.CommentPlugin.GetNotificationAsync(user);
                if (count)
                {
                    int notifiesNotRead = notifies.Where(x => !x.IsRead).Count();
                    return Ok(notifiesNotRead);
                }
                return Ok(notifies);
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
