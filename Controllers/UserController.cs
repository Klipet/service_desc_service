using DevExpress.Xpo;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class UserController: ControllerBase
{
    private readonly UnitOfWork _uow;
    public UserController(UnitOfWork uow)
    {
        _uow = uow;
    }
    [HttpGet]
    public IActionResult GetAllUser()
    {
        try
        {
            var users = _uow.Query<User>().ToList();
            var resault = users.Select(u => new UserDto
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                FirstName = u.FirstName,
                Password = u.Password,
                Phone = u.Phone,
                Loghin = u.Loghin,
                DateCreated = u.DateCreated,
                WorkSpace = u.WorkSpace
            }).ToList();
            return Ok(resault);
        }
        catch (Exception ex) {
            return StatusCode(500, $"Ошибка: {ex.Message}");
        }
    }
    [HttpPost]
    public IActionResult CreateUser() {
        return null;
    }
}