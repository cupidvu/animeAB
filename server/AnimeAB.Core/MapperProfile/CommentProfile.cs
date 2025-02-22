﻿using AnimeAB.Reponsitories.DTO;
using AnimeAB.Reponsitories.Entities;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AnimeAB.Core.MapperProfile
{
    public class CommentProfile : Profile
    {
        public CommentProfile()
        {
            CreateMap<CommentDto, Comment>()
               .ForMember(dto => dto.DisplayName,
                       conf => conf.MapFrom(opt => opt.name))
               .ForMember(dto => dto.Key,
                       conf => conf.MapFrom(opt => opt.key))
               .ForMember(dto => dto.UserLocal,
                       conf => conf.MapFrom(opt => opt.user_local))
               .ForMember(dto => dto.PhotoUrl,
                       conf => conf.MapFrom(opt => opt.photo_url))
               .ForMember(dto => dto.Message,
                       conf => conf.MapFrom(opt => opt.message))
               .ForMember(dto => dto.When,
                       conf => conf.MapFrom(opt => opt.when))
               .ForMember(dto => dto.ReplyComment,
                       conf => conf.MapFrom(opt => opt.reply_comment))
               .ReverseMap();
        }
    }
}
