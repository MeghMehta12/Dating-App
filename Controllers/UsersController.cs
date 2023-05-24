using API.Data;
using API.DTOs;
using API.Entities;
using API.Interface;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace API.Controllers
{
    [Authorize]
    public class UsersController :BaseApiController
    {
        private readonly IMapper mapper;

        public IUserRepository _userRepository { get; }

        public UsersController(IUserRepository userRepository,IMapper Mapper)
        {
            _userRepository = userRepository;
            mapper = Mapper;
        }
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers()
        {
            return Ok(mapper.Map<IEnumerable<MemberDto>>(await _userRepository.GetMembersAsync()));
        }

        [HttpGet("{username}")]
        public async Task<ActionResult<MemberDto>> GetUser(string username)
        {
            return mapper.Map<MemberDto>(await _userRepository.GetMemberAsync(username));
        }
    }
}
