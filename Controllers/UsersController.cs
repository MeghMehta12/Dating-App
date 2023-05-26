using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interface;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Security.Claims;

namespace API.Controllers
{
    [Authorize]
    public class UsersController :BaseApiController
    {
        private readonly IMapper mapper;
        private readonly IPhotoService _photoService;

        public IUserRepository _userRepository { get; }

        public UsersController(IUserRepository userRepository,IMapper Mapper,
                               IPhotoService PhotoService)
        {
            _userRepository = userRepository;
            mapper = Mapper;
            _photoService = PhotoService;
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

        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
        {
            var user = await _userRepository.GetUserByUserNameAsync(User.GetUserName());
            if(user == null) { return NotFound(); }
            mapper.Map(memberUpdateDto, user);
            if (await _userRepository.SaveAllAsync()) return NoContent();
            return BadRequest("Failed To Update User");
        }

        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {
            var currentUser = await _userRepository.GetUserByUserNameAsync(User.GetUserName());
            if(currentUser == null) { return NotFound();}
            var result = await _photoService.AddPhotoAsync(file);
            if (result.Error != null) return BadRequest(result.Error.Message);

            var photo = new Photo
            {
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId
            };
            if(currentUser.Photos.Count ==0) photo.IsMain =true;
            currentUser.Photos.Add(photo); 
            if(await _userRepository.SaveAllAsync())
            {
                return CreatedAtAction(nameof(GetUser), new { username = currentUser.UserName },mapper.Map<PhotoDto>(photo));
            }
            return BadRequest("Problem Adding Photo");
        }
        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            var user =await _userRepository.GetUserByUserNameAsync(User.GetUserName());
            if(user == null) return NotFound(); 
            var photo = user.Photos.FirstOrDefault(x=>x.Id== photoId);
            if(photo == null) return NotFound();
            if(photo.IsMain) return BadRequest("Already Main");
            var currentMain = user.Photos.FirstOrDefault(x => x.IsMain);
            if (currentMain != null) currentMain.IsMain = false ;
            photo.IsMain = true;

            if (await _userRepository.SaveAllAsync()) return NoContent();
            return BadRequest("Error Saving Photo");
        }

        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            var user = await _userRepository.GetUserByUserNameAsync(User.GetUserName());
            var photo = user.Photos.FirstOrDefault(X=>X.Id ==photoId);
            if(photo ==null) return NotFound();
            if (photo.IsMain) return BadRequest("You cannot delete your main photo");
            if (photo.PublicId != null)
            {
                var result = await _photoService.DeletPhotoAsync(photo.PublicId);
                if (result.Error != null) return BadRequest(result.Error);  
            }
            user.Photos.Remove(photo);
            if (await _userRepository.SaveAllAsync()) return Ok();
            return BadRequest("Problem Deleting The Photo");
        }
    }
}
