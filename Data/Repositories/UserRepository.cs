using API.DTOs;
using API.Entities;
using API.Helpers;
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

        public async Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams)
        {
            var minDOB = DateOnly.FromDateTime(DateTime.Today.AddYears(-userParams.MaxAge - 1));
            var maxDOB = DateOnly.FromDateTime(DateTime.Today.AddYears(-userParams.MinAge));
            var query = _context.Users.AsQueryable().Where(x=>x.UserName!=userParams.CurrentUsername &&
                                                          x.Gender!=userParams.Gender && 
                                                          (x.DateOfBirth>=minDOB && x.DateOfBirth<=maxDOB));
            query = userParams.OrderBy switch
            {
                "created" => query.OrderByDescending(u => u.Created),
                _=>query.OrderByDescending(u=>u.LastActive)
            };
            return await PagedList<MemberDto>.CreateAsync(query.AsNoTracking().ProjectTo<MemberDto>(mapper.ConfigurationProvider), userParams.PageNumber,userParams.PageSize);
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
