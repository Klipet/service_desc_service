using Amazon.Runtime.Internal;
using DevExpress.Xpo;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using static DevExpress.Data.Helpers.ExpressiveSortInfo;

[ApiController]
[Route("[controller]")]
public class UserController: ControllerBase
{
    private readonly UnitOfWork _uow;
    private readonly GenerateJwtToken _jwtGenerator;

    public UserController(UnitOfWork uow, GenerateJwtToken jwtGenerator)
    {
        _uow = uow;
        _jwtGenerator = jwtGenerator;
    }

    [HttpGet]
    public IActionResult GetAllUser()
    {
        try
        {
            var users = _uow.Query<User>().ToList();
            var resault = users.Select(u => new UserResponseDto
            {
                Oid = u.Oid,
                Name = u.Name,
                Email = u.Email,
                FirstName = u.FirstName,
                Phone = u.Phone,
                Loghin = u.Loghin,
                DateCreated = u.DateCreated,
                WorkSpaceName = u.WorkSpace.Name,
                WorkSpaceId = u.WorkSpace.Oid
            }).ToList();
            return Ok(resault);
        }
        catch (Exception ex) {
            return StatusCode(500, $"Ошибка: {ex.Message}");
        }
    }

    [HttpPost]
    public IActionResult CreateUser([FromBody] UserDto userModel) {
        if (userModel == null)
            return BadRequest("Model is null");

        var wp = _uow.Query<WorkSpace>().FirstOrDefault(u => u.Oid == userModel.WorkSpaceId);
        if (wp == null)
            return NotFound("Рабочее место не найдено");

        var user = new User(_uow)
        {
            Name = userModel.Name,
            Email = userModel.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(userModel.Password),
            FirstName = userModel.FirstName,
            Phone = userModel.Phone,
            Loghin = userModel.Loghin,
            DateCreated = DateTime.UtcNow,
            WorkSpace = wp

        };
        try
        {
            _uow.CommitChanges();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error saving to database: {ex.Message}");
        }
        var resaultUser = new UserResponseDto
        {
            Oid = user.Oid,
            Name = user.Name,
            Email = user.Email,
            FirstName = user.FirstName,
            Phone = user.Phone,
            WorkSpaceName = user.WorkSpace.Name,
            DateCreated = DateTime.UtcNow,
        };

        return Ok(resaultUser);
    }

    [HttpPut("Update[controller]")]
    public IActionResult UpdateUser([FromQuery]int id, [FromBody] UserDto userModel) 
    {
        try
        {
            var user = _uow.Query<User>().FirstOrDefault(u => u.Oid == id);
            if (user == null) return NotFound($"User Not found {id} incorect");

            var wp = _uow.GetObjectByKey<WorkSpace>(user.WorkSpace);
            if (wp == null)
                return NotFound("Пользователь не найден");

            user.Name = userModel.Name;
            user.Email = userModel.Email;
            user.FirstName = userModel.FirstName;
            user.Phone = userModel.Phone;
            user.PasswordHash = userModel.Password;
            user.Loghin = userModel.Loghin;
            user.WorkSpace = wp;
            _uow.CommitChanges();
            return Ok($"User {id}, {user.Name} is update");
        }
        catch (Exception ex) 
        {
            return StatusCode(ex.HResult, $"Ошибка при обновлении: {ex.Message}");
        }
    }


    [HttpGet("Get[controller]ById")]
    public IActionResult UserGetById([FromQuery]int id) 
    {
        var user = _uow.GetObjectByKey<User>(id);
        if (user == null) return NotFound("User not found");

        var wp = _uow.GetObjectByKey<WorkSpace>(user.WorkSpace);
        if (wp == null)
            return NotFound("Пользователь не найден");
        try
        {


            var responseUser = new UserResponseDto
            {
                Oid = user.Oid,
                Name = user.Name,
                Email = user.Email,
                FirstName = user.FirstName,
                Phone = user.Phone,
                Loghin = user.Loghin,
                WorkSpaceName = wp.Name,
                DateCreated = user.DateCreated

            };
            return Ok(responseUser);
        }
        catch (Exception ex) {
            return StatusCode(ex.HResult, $"User error: {ex.Message}");
        }
    }

    [HttpPost("Auth")]
    public IActionResult AuthUser([FromBody] LoginRequestDto reqest)
    {
        if (string.IsNullOrWhiteSpace(reqest.Login) || string.IsNullOrWhiteSpace(reqest.Password))
            return BadRequest("Login and password are required");

        var user = _uow.Query<User>().FirstOrDefault(u => u.Loghin == reqest.Login);
        if (user == null || !BCrypt.Net.BCrypt.Verify(reqest.Password, user.PasswordHash))
            return Unauthorized("Invalid credentials");

        try
        {
            var token = _jwtGenerator.GenerateJwtTokenString(user);

            return Ok(new
            {
                token = token,
                user = new
                {
                    id = user.Oid,
                    name = user.Name,
                    email = user.Email,
                    login = user.Loghin
                }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(ex.HResult, $"User error: {ex.Message}");
        }
    }

    [HttpDelete("Delete[controller]ById")]
    public IActionResult DeleteUser([FromQuery] int id) {
        try
        {
            var user = _uow.Query<User>().FirstOrDefault(u => u.Oid == id);
            if (user == null) return NotFound($"User {id} is incorrect");
            _uow.Delete(user);
            _uow.CommitChanges();
            return Ok("User deleted");
        }
        catch(Exception ex) 
        {
            return StatusCode(ex.HResult, ex.Message);
        }
    }

}