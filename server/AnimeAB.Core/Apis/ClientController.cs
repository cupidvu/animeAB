﻿
using AnimeAB.Reponsitories.Domain;
using AnimeAB.Reponsitories.DTO;
using AnimeAB.Reponsitories.Interface;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;

namespace AnimeAB.Core.Apis
{
    [Route("client")]
    [ApiController]
    [Produces(MediaTypeNames.Application.Json)]
    public class ClientController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public ClientController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpPost]
        [Consumes(MediaTypeNames.Application.Json)]
        public IActionResult SignUpClient([FromBody] ClientDto client)
        {
            try
            {
                AccountSignUpDto account = _mapper.Map<AccountSignUpDto>(client);
                var result = _unitOfWork.AccountEntity.CreateEmailPasswordAsync(account);

                if (!result.Success) return BadRequest(result.Message);

                return Ok(result.Message);
            }
            catch
            {
                return BadRequest("SIGNUP_FAILED");
            }
        }

        [HttpGet]
        public async Task<IActionResult> SignInClient([FromQuery] ClientSignInDto client)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(client.email) || string.IsNullOrWhiteSpace(client.password))
                    return BadRequest("INVALID_EMAIL_PASSWORD");

                AccountLoginDto account = new AccountLoginDto
                {
                    Email = client.email,
                    Password = client.password
                };

                var result = await _unitOfWork.AccountEntity.SignInEmailPasswordAsync(account, true);
                if (!result.Success) return BadRequest(result.Message);
                return Ok(result.Data);
            }
            catch
            {
                return BadRequest("LOGIN_FAILED");
            }
        }

        [Route("{user_uid}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet]
        public async Task<IActionResult> GetClientAsync([FromRoute] string user_uid)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(user_uid))
                {
                    string token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                    var client = await _unitOfWork.AccountEntity.GetUserCurrentAsync(token);
                    return Ok(client);
                }
                return BadRequest("INVALID_USER");
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [Route("password")]
        [HttpGet]
        public IActionResult ForgotPasswordClient([FromQuery] string email)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(email))
                {
                    var result = _unitOfWork.AccountEntity.ForgotPasswordAsync(email);
                    if (!result.Success) return BadRequest(result.Message);

                    return NoContent();
                }
                return BadRequest("INVALID_EMAIL");
            }
            catch
            {
                return BadRequest("EMAIL_EXIST");
            }
        }

        [Route("{user_uid}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Consumes(MediaTypeNames.Application.Json)]
        public async Task<IActionResult> UpdateProfile([FromBody]ProfileClient client)
        {
            try
            {
                string token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

                if (!string.IsNullOrWhiteSpace(client.password) 
                    && !string.IsNullOrWhiteSpace(client.newPassword))
                {
                    if (string.IsNullOrWhiteSpace(client.email)) return BadRequest("Email không được tìm thấy.");
                    var changePassword = await _unitOfWork.AccountEntity.ChangePasswordAsync(token, client.email, client.password, client.newPassword);
                    if (!changePassword.Success) return BadRequest(changePassword.Message);

                    return Ok(changePassword.Data);
                }

                if(!string.IsNullOrWhiteSpace(client.name) 
                    && !string.IsNullOrWhiteSpace(client.email) 
                    && !string.IsNullOrWhiteSpace(client.photo_url))
                {
                   var result = await _unitOfWork.AccountEntity.UpdateProfileAsync(
                   new ProfileDomain
                   {
                       Token = token,
                       Email = client.email,
                       DisplayName = client.name,
                       Avatar = client.photo_url
                   });

                    if (!result.Success) return BadRequest(result.Message);
                    return Ok(client);
                }

                return BadRequest("INVALID_ID_TOKEN");
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
