using API.DTOs;
using API.Entities;
using API.Interface;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IMapper mapper;

        public DataContext _context { get; }
        public UserRepository(DataContext context,IMapper Mapper)
        {
            _context = context;
            mapper = Mapper;
        }


        public async Task<IEnumerable<AppUser>> GetUserAsync()
        {
            return await _context.Users.Include(p=>p.Photos).ToListAsync();  
        }

        public async Task<AppUser> GetUserById(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<AppUser> GetUserByUserNameAsync(string userName)
        {
            return await _context.Users.Include(p => p.Photos).FirstOrDefaultAsync(x=>x.UserName == userName); 
        }

        public async Task<bool> SaveAllAsync()
        {
            return await _context.SaveChangesAsync()>0; 
        }

        public void Update(AppUser user)
        {
            _context.Entry(user).State = EntityState.Modified;  
        }

        public async Task<IEnumerable<MemberDto>> GetMembersAsync()
        {
            return await _context.Users.
                        ProjectTo<MemberDto>(mapper.ConfigurationProvider)
                        .ToListAsync();
        }

        public async Task<MemberDto> GetMemberAsync(string username)
        {
            return await _context.Users.
                     Where(x=>x.UserName==username).
                    ProjectTo<MemberDto>(mapper.ConfigurationProvider)
                   .SingleOrDefaultAsync();
        }
    }
}
